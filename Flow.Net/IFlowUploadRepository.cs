using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flow.Net
{
    public interface IFlowUploadRepository
    {
        Task<FlowFile> GetUploadAsync(FlowChunk chunk);
        Task AddAsync(FlowFile flowFileMeta);
        Task RemoveAsync(FlowChunk chunk);
    }

    public class FlowUploadRepository : IFlowUploadRepository
    {
        /// <summary>
        /// Track our in progress uploads, by using a cache, we make sure we don't accumulate memory
        /// </summary>
        // todo Inject this via constructor
        private readonly MemoryCache _uploadChunkCache = MemoryCache.Default;

        /// <summary>
        /// Ensures the thread safety of our static methods.
        /// </summary>
        private readonly object _chunkCacheLock = new object();

        public Task<FlowFile> GetUploadAsync(FlowChunk chunk)
        {
            lock (_chunkCacheLock)
            {
                var upload = _uploadChunkCache[chunk.FlowIdentifier] as FlowFile;
                return Task.FromResult(upload);
            }
        }

        public Task AddAsync(FlowFile flowFileMeta)
        {
            lock (_chunkCacheLock)
            {
                // todo refactor out to the data store
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
            lock (_chunkCacheLock)
            {
                _uploadChunkCache.Remove(chunk.FlowIdentifier);
                return Task.FromResult((object) null);
            }
        }
    }
}