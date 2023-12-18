-- Lua script to remove a list of keys from the Redis database
local keysToDelete = ARGV

local result = {}
for _, key in ipairs(keysToDelete) do
    local deleted = redis.call('DEL', key)
    table.insert(result, { key, deleted })
end

return result
