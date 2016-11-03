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
        Task<bool> ProcessUploadChunkRequestAsync(HttpRequestMessage request);
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
        
        public DateTime CompletedDateTime { get; private set; }

        public FlowUploadProcessor(FlowMultipartFormDataStreamProvider streamProvider, IFlowUploadRepository uploadRepository)
        {
            _streamProvider = streamProvider;
            _uploadRepository = uploadRepository;
        }

        //todo consider the necessity of this method on this class
        public async Task<bool> ProcessUploadChunkRequestAsync(HttpRequestMessage request)
        {
            await request.Content.ReadAsMultipartAsync(_streamProvider);

            var upload = await _uploadRepository.GetUploadAsync(_streamProvider.Chunk);

            if (upload == null)
            {
                upload = new FlowFile(_streamProvider.Chunk);
                await _uploadRepository.AddAsync(upload);
            }

            upload.RegisterChunkAsReceived(_streamProvider.Chunk);
            if (upload.IsComplete)
            {
                // Since we are using a cache and memory is automatically disposed,
                // we don't need to do this, so we won't so we can keep a record of
                // our completed uploads.
                await _uploadRepository.RemoveAsync(_streamProvider.Chunk);
           
                CompletedDateTime = DateTime.Now;
            }

            return upload.IsComplete;
        }
        
    }
}