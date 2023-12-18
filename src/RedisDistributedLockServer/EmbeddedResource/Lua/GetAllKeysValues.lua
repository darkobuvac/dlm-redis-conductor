-- Lua script to get all values of keys in the Redis database
local keys = redis.call('KEYS', '*')
local result = {}

for _, key in ipairs(keys) do
    local value = redis.call('GET', key)
    table.insert(result, { key, value })
end

return result
