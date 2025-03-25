using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OMS.Application.Interfaces;

namespace OMS.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache get işlemi sırasında hata oluştu. Anahtar: {Key}", key);
                return default;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedValue = await _distributedCache.GetAsync(key);
                return cachedValue != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache exists kontrolü sırasında hata oluştu. Anahtar: {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiry.HasValue)
                {
                    options.SetAbsoluteExpiration(expiry.Value);
                }

                var serializedValue = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, serializedValue, options);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache set işlemi sırasında hata oluştu. Anahtar: {Key}", key);
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache remove işlemi sırasında hata oluştu. Anahtar: {Key}", key);
                return false;
            }
        }

        public async Task<bool> RefreshAsync(string key, TimeSpan expiry)
        {
            try
            {
                var cachedValue = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(cachedValue))
                {
                    return false;
                }

                var options = new DistributedCacheEntryOptions();
                options.SetAbsoluteExpiration(expiry);
                await _distributedCache.SetStringAsync(key, cachedValue, options);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis cache refresh işlemi sırasında hata oluştu. Anahtar: {Key}", key);
                return false;
            }
        }
    }
}
