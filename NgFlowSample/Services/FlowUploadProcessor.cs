using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Ajax.Utilities;
using NgFlowSample.Models;

namespace NgFlowSample.Services
{

    /// <summary>
    /// A Service to handle an upload from Flow. Contains thread safe static methods
    /// to handle upload tracking, and instance methods to handle the upload of a single
    /// chunk.
    /// </summary>
    public class FlowUploadProcessor
    {

        //================================================================================
        // Class Methods
        //================================================================================
        #region Static Methods
        /// <summary>
        /// Ensures the thread safety of our static methods.
        /// </summary>
        private static Object chunkCacheLock = new Object();

        /// <summary>
        /// Track our in progress uploads, by using a cache, we make sure we don't accumulate memory
        /// </summary>
        private static MemoryCache uploadChunkCache = MemoryCache.Default;

        private static FileMetaData GetFileMetaData(string flowIdentifier)
        {
            lock (chunkCacheLock)
            {
                return uploadChunkCache[flowIdentifier] as FileMetaData;
            }
        }

        /// <summary>
        /// Keep an upload in cache for two hours after it is last used
        /// </summary>
        private static CacheItemPolicy DefaultCacheItemPolicy()
        {
            return new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(120)
            };
        }


        /// <summary>
        /// Creates a Stream Provider suitable for handling a single upload chunk.
        /// </summary>
        public static FlowMultipartFormDataStreamProvider GetMultipartProvider(string uploadPath)
        {
            if (String.IsNullOrEmpty(uploadPath))
            {
                throw new ArgumentNullException("uploadPath");
            }

            var root = HttpContext.Current.Server.MapPath(uploadPath);
            Directory.CreateDirectory(root);

            return new FlowMultipartFormDataStreamProvider(root);
        }

        /// <summary>
        /// (Thread Safe) Marks a chunk as recieved.
        /// </summary>
        private static bool RegisterSuccessfulChunk(FlowMetaData chunkMeta)
        {
            FileMetaData fileMeta;
            lock (chunkCacheLock)
            {
                fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);

                if (fileMeta == null)
                {
                    fileMeta = new FileMetaData(chunkMeta);
                    uploadChunkCache.Add(chunkMeta.FlowIdentifier, fileMeta, DefaultCacheItemPolicy());
                }

                fileMeta.RegisterChunkAsReceived(chunkMeta);
                if (fileMeta.IsComplete)
                {
                    // Since we are using a cache and memory is automatically disposed,
                    // we don't need to do this, so we won't so we can keep a record of
                    // our completed uploads.
                    //uploadChunkCache.Remove(chunkMeta.FlowIdentifier);
                }
            }
            return fileMeta.IsComplete;
        }

        public static bool HasRecievedChunk(FlowMetaData chunkMeta)
        {
            var fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);

            bool wasRecieved = fileMeta != null && fileMeta.HasChunk(chunkMeta);

            return wasRecieved;
        }
        #endregion
        //================================================================================
        // Instance Methods
        //================================================================================
        #region Instance Methods
        private FlowMultipartFormDataStreamProvider StreamProvider { get; set; }

        public FlowMetaData MetaData
        {
            get { return this.StreamProvider.MetaData; }
        }

        public bool IsComplete { get; private set; }

        public DateTime CompletedDateTime { get; private set; }

        public FlowUploadProcessor(string uploadPath)
        {
            StreamProvider = GetMultipartProvider(uploadPath);
        }

        public async Task<bool> ProcessUploadChunkRequest(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(StreamProvider);

            IsComplete = RegisterSuccessfulChunk(MetaData);

            if (IsComplete)
            {
                CompletedDateTime = DateTime.Now;
            }

            return IsComplete;
        }
        #endregion
    }

    /// <summary>
    /// Our own internal metadata to track the progress of a download. 
    /// </summary>
    class FileMetaData
    {
        private static long ChunkIndex(long chunkNumber)
        {
            return chunkNumber - 1;
        }

        public FileMetaData(FlowMetaData flowMeta)
        {
            FlowMeta = flowMeta;
            ChunkArray = new bool[flowMeta.FlowTotalChunks];
            TotalChunksReceived = 0;
        }

        public bool[] ChunkArray { get; set; }

        /// <summary>
        /// Chunks can come out of order, so we must track how many chunks 
        /// we have successfully recieved to determine if the download is complete.
        /// </summary>
        public int TotalChunksReceived { get; set; }

        public FlowMetaData FlowMeta { get; set; }

        public bool IsComplete
        {
            get
            {
                return (TotalChunksReceived == FlowMeta.FlowTotalChunks);
            }
        }

        public void RegisterChunkAsReceived(FlowMetaData flowMeta)
        {
            ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)] = true;
            TotalChunksReceived++;
        }

        public bool HasChunk(FlowMetaData flowMeta)
        {
            return ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)];
        }
    }
}