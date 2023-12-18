using Autofac.Extensions.DependencyInjection;
using MediatR;
using RedisDistributedLockServer;
using RedisDistributedLockServer.BackgroundServices;
using RedisDistributedLockServer.Handlers;
using RedisDistributedLockServer.Options;
using RedisDistributedLockServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.ConfigureOptions<RedisOptionsSetup>();

//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.RegisterServices(builder.Configuration);

builder.Host.AddWorkflows(builder.Configuration);

builder.Services.AddHostedService<LockAutoreleaseService>();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet(
    "/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"
);

//var result = await RunHandler(app);

app.Run();

static async Task<LockResource.Response> RunHandler(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var result = await mediator.Send(
        new LockResource
        {
            Resource = "REG-01-CPE-01",
            WorkflowId = Guid.NewGuid().ToString(),
            TotalAcquireLockTime = 10,
            ExpiryTime = 60
        }
    );

    return result;
}
