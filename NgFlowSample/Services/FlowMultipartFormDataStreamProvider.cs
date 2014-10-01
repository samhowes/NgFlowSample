using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NgFlowSample.Models;

namespace NgFlowSample.Services
{
    public class FlowMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public FlowMetaData MetaData;
        public readonly string Filename;
        public static Dictionary<string, FileStream> FlowFileStreams { get; set; }

        public FlowMultipartFormDataStreamProvider(string path): base(path)
        {}

        public override string GetLocalFileName(HttpContentHeaders headers)
        {

            /*HttpContent requestContent = Request.Content;
            string jsonContent = requestContent.ReadAsStringAsync().Result;
            CONTACT contact = JsonConvert.DeserializeObject<CONTACT>(jsonContent);*/
            var flowMetaDictionary = new Dictionary<string, string>();
            foreach (var item in this.Contents)
            {
                var param = item.Headers.ContentDisposition.Parameters.FirstOrDefault();
                string key = param.Value.Trim('\"');
                string value = item.ReadAsStringAsync().Result;
                flowMetaDictionary[key] = value;
            }

            MetaData = new FlowMetaData(flowMetaDictionary);

            //var flowChunkName = String.Format("{0}.{1}", MetaData.flowFilename, MetaData.FlowChunkNumber);
            //return flowChunkName;
            return MetaData.FlowFilename;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentDisposition.FileName == null)
            {
                return base.GetStream(parent, headers);
            }
            var flowFileName = GetLocalFileName(headers);

            FileStream flowFileStream;

            var path = Path.Combine(RootPath, flowFileName);


            flowFileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            flowFileStream.SetLength(MetaData.FlowTotalSize);

            flowFileStream.Seek(MetaData.FileOffset, 0);

            return flowFileStream;
        }

        public override async Task ExecutePostProcessingAsync()
        {
        }
    }
    
}