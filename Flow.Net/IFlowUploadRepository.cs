using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flow.Net
{
    public interface IFlowUploadRepository
    {
        Task<FileMetaData> GetUploadAsync(FlowMetaData metaData);
        Task AddAsync(FileMetaData fileMeta);
        Task RemoveAsync(FlowMetaData metaData);
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

        public Task<FileMetaData> GetUploadAsync(FlowMetaData metaData)
        {
            lock (_chunkCacheLock)
            {
                var upload = _uploadChunkCache[metaData.FlowIdentifier] as FileMetaData;
                return Task.FromResult(upload);
            }
        }

        public Task AddAsync(FileMetaData fileMeta)
        {
            lock (_chunkCacheLock)
            {
                // todo refactor out to the data store
                var cachePolicy = new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(120)
                };
        
                _uploadChunkCache.Add(fileMeta.FlowIdentifier, fileMeta, cachePolicy);
                return Task.FromResult((object)null);
            }
        }

        public Task RemoveAsync(FlowMetaData metaData)
        {
            lock (_chunkCacheLock)
            {
                _uploadChunkCache.Remove(metaData.FlowIdentifier);
                return Task.FromResult((object) null);
            }
        }
    }
}