using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Flow.Net
{

    public class FlowUploadOptions
    {
        public string UploadDirectoryPath { get; set; }
    }

    public interface IFlowUploadProcessor
    {
        Task<bool> ProcessChunkRequestAsync(HttpRequestMessage request);
    }

    /// <summary>
    /// A Service to handle an upload from Flow. Contains thread safe static methods
    /// to handle upload tracking, and instance methods to handle the upload of a single
    /// chunk.
    /// </summary>
    //todo consider the necessity of this class
    public class FlowUploadProcessor : IFlowUploadProcessor
    {
        private readonly FlowMultipartFormDataStreamProvider _streamProvider;
        private readonly IFlowUploadRepository _uploadRepository;
        
        public FlowUploadProcessor(FlowMultipartFormDataStreamProvider streamProvider, IFlowUploadRepository uploadRepository)
        {
            _streamProvider = streamProvider;
            _uploadRepository = uploadRepository;
        }

        //todo consider the necessity of this method on this class
        public async Task<bool> ProcessChunkRequestAsync(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(_streamProvider);

            var chunk = _streamProvider.Chunk;
            
            var upload = await _uploadRepository.GetAsync(chunk.FileIdentifier);

            if (upload == null)
            {
                upload = new FlowFile(chunk.FileIdentifier, chunk.TotalChunks);
                await _uploadRepository.AddAsync(upload);
            }

            upload.RegisterChunk(_streamProvider.Chunk);
            if (upload.IsComplete)
            {
                // Since we are using a cache and memory is automatically disposed,
                // we don't need to do this, so we won't so we can keep a record of
                // our completed uploads.
                await _uploadRepository.RemoveAsync(_streamProvider.Chunk);
            }

            return upload.IsComplete;
        }
        
    }
}