using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Flow.Net;

namespace NgFlowSample.Controllers
{
    
    
    [RoutePrefix("api")]
    public class FileUploadController : ApiController
    {
        private readonly IFlowUploadProcessor _flowUploadProcessor;
        private readonly IFlowUploadRepository _uploadRepository;

        public FileUploadController()
        {
            //todo dependency injection
            var options = new FlowUploadOptions()
            {
                UploadDirectoryPath = "~/App_Data/Tmp/FileUploads" //todo make sure upload path is created somewhere
            };
            var streamProvider = new FlowMultipartFormDataStreamProvider(options.UploadDirectoryPath);
            _uploadRepository = new MemoryCacheFlowUploadRepository(MemoryCache.Default);
            _flowUploadProcessor = new FlowUploadProcessor(streamProvider, _uploadRepository);
        }
        
        [Route("Upload"), HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }
            
            bool isComplete = await _flowUploadProcessor.ProcessChunkRequestAsync(Request);

            if (isComplete)
            {
                // Do post processing here:
                // - Move the file to a permanent location
                // - Persist information to a database
            }
           
            return Ok();
        }

        [Route("Upload"), HttpGet]
        public async Task<IHttpActionResult> TestFlowChunk([FromUri]FlowChunk chunk)
        {
            FlowFile upload = await _uploadRepository.GetAsync(chunk.FileIdentifier);

            bool wasRecieved = upload != null && upload.HasChunk(chunk);
            if (wasRecieved)
            {
                return Ok();
            }
            
            return NotFound();
        }
    }
}