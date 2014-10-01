using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        /// <summary>
        /// Ensures the thread safety of our static methods.
        /// </summary>
        private static Object chunkDictionaryLock = new Object();
        
        /// <summary>
        /// Track our in progress downloads.
        /// </summary>
        private static Dictionary<string, FileMetaData> uploadChunkDictionary = new Dictionary<string, FileMetaData>();

        
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
        private static void RegisterSuccessfulChunk(FlowMetaData chunkMeta)
        {
            lock (chunkDictionaryLock)
            {
                FileMetaData fileMeta;

                if (!uploadChunkDictionary.TryGetValue(chunkMeta.FlowIdentifier, out fileMeta))
                {
                    fileMeta = new FileMetaData(chunkMeta);
                    uploadChunkDictionary[chunkMeta.FlowIdentifier] = fileMeta;
                }

                fileMeta.RegisterChunkAsReceived(chunkMeta);
                if (fileMeta.IsComplete)
                {
                    uploadChunkDictionary.Remove(chunkMeta.FlowIdentifier);
                }
            }   
        }

        public static bool HasRecievedChunk(FlowMetaData chunkMeta)
        {
            bool wasRecieved;
            
            lock (chunkDictionaryLock)
            {
                FileMetaData fileMeta;
                wasRecieved = uploadChunkDictionary.TryGetValue(chunkMeta.FlowIdentifier, out fileMeta) &&
                   fileMeta.HasChunk(chunkMeta);
            }

            return wasRecieved;
        }

        //================================================================================
        // Instance Methods
        //================================================================================

        private FlowMultipartFormDataStreamProvider StreamProvider { get; set; }
        
        public FlowMetaData MetaData
        {
            get { return this.StreamProvider.MetaData; }
        }


        public FlowUploadProcessor(string uploadPath)
        {
            StreamProvider = GetMultipartProvider(uploadPath);
        }


        public async Task ProcessUploadChunkRequest(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(StreamProvider);

            RegisterSuccessfulChunk(MetaData);

        }

    }

    /// <summary>
    /// Our own internal metadata to track the progress of a download. 
    /// </summary>
    class FileMetaData
    {
        private static int ChunkIndex(int chunkNumber)
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