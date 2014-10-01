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
using NgFlowSample.Models;
using NgFlowSample.Services;

namespace NgFlowSample.Controllers
{
    
    
    [RoutePrefix("api")]
    public class FileUploadController : ApiController
    {
        
        [Route("Upload"), HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var uploadProcessor = new FlowUploadProcessor("~/App_Data/Tmp/FileUploads");
            await uploadProcessor.ProcessUploadChunkRequest(Request);
           
            return Ok();
        }

        [Route("Upload"), HttpGet]
        public IHttpActionResult TestFlowChunk([FromUri]FlowMetaData flowMeta)
        {
            if (FlowUploadProcessor.HasRecievedChunk(flowMeta))
            {
                return Ok();
            }
            
            return NotFound();
        }

        
    }
}