using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            _uploadRepository = new FlowUploadRepository();
            _flowUploadProcessor = new FlowUploadProcessor(streamProvider, _uploadRepository);
        }
        
        [Route("Upload"), HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }
            
            bool isComplete = await _flowUploadProcessor.ProcessUploadChunkRequestAsync(Request);

            if (isComplete)
            {
                // Do post processing here:
                // - Move the file to a permanent location
                // - Persist information to a database
                // - Raise an event to signal it was completed (if you are really feeling up to it)
                //      - http://www.udidahan.com/2009/06/14/domain-\events-salvation/
                //      - http://msdn.microsoft.com/en-gb/magazine/ee236415.aspx#id0400079
            }
           
            return Ok();
        }

        [Route("Upload"), HttpGet]
        public async Task<IHttpActionResult> TestFlowChunk([FromUri]FlowMetaData flowMeta)
        {
            var upload = await _uploadRepository.GetUploadAsync(flowMeta);

            bool wasRecieved = upload != null && upload.HasChunk(flowMeta);
            if (wasRecieved)
            {
                return Ok();
            }
            
            return NotFound();
        }

        
    }
}