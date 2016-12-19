using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flow.Net
{
    public interface IFlowUploadRepository
    {
        Task<FlowFile> GetAsync(string flowFileIdentifier);
        Task AddAsync(FlowFile flowFileMeta);
        Task RemoveAsync(FlowChunk chunk);
    }

    public class MemoryCacheFlowUploadRepository : IFlowUploadRepository
    {
        /// <summary>
        /// Track our in progress uploads, by using a cache, we make sure we don't accumulate memory
        /// </summary>
        private readonly MemoryCache _uploadChunkCache;
        
        public MemoryCacheFlowUploadRepository(MemoryCache uploadChunkCache)
        {
            _uploadChunkCache = uploadChunkCache;
        }

        public Task<FlowFile> GetAsync(string flowFileIdentifier)
        {
            lock (_uploadChunkCache)
            {
                var upload = _uploadChunkCache[flowFileIdentifier] as FlowFile;
                return Task.FromResult(upload);
            }
        }

        public Task AddAsync(FlowFile flowFileMeta)
        {
            lock (_uploadChunkCache)
            {
                var cachePolicy = new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(120)
                };
        
                _uploadChunkCache.Add(flowFileMeta.FlowIdentifier, flowFileMeta, cachePolicy);
                return Task.FromResult((object)null);
            }
        }

        public Task RemoveAsync(FlowChunk chunk)
        {
            lock (_uploadChunkCache)
            {
                _uploadChunkCache.Remove(chunk.FileIdentifier);
                return Task.FromResult((object) null);
            }
        }
    }
}