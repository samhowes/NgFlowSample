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

        /// <summary>
        /// Process the form values that were submitted to construct a file name.
        /// Make sure all form items come before the actual file data payload.
        /// </summary>
        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            var flowMetaDictionary = new Dictionary<string, string>();
            foreach (var item in this.Contents)
            {
                var param = item.Headers.ContentDisposition.Parameters.FirstOrDefault();
                string key = param.Value.Trim('\"');
                string value = item.ReadAsStringAsync().Result;

                // Possible way to parse as JSON:
                //MyObject myObj = JsonConvert.DeserializeObject<MyObject>(value);

                flowMetaDictionary[key] = value;
            }

            MetaData = new FlowMetaData(flowMetaDictionary);

            //var flowChunkName = String.Format("{0}.{1}", MetaData.flowFilename, MetaData.FlowChunkNumber);
            //return flowChunkName;
            return MetaData.FlowFilename;
        }

        /// <summary>
        /// <para>This method assumes that all form segments come before the actual file data.</para>
        /// <para>Otherwise, GetLocalFileName will fail.</para>
        /// </summary>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (headers.ContentDisposition.FileName == null)
            {
                return base.GetStream(parent, headers);
            }

            string flowFileName = null;
            try
            {
                flowFileName = GetLocalFileName(headers);
            }
            catch (Exception ex)
            {
                throw new Exception("Flow chunk information was not properly transmitted before the chunk payload.", ex);
            }

            

            FileStream flowFileStream;

            var path = Path.Combine(RootPath, flowFileName);


            flowFileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
            flowFileStream.SetLength(MetaData.FlowTotalSize);

            flowFileStream.Seek(MetaData.FileOffset, 0);

            return flowFileStream;
        }

        /// <summary>
        /// <para>The default implementation does what GetLocalFileName() does in here.</para>
        /// <para>Make sure to override it so it doesn't try to do those things a second time.</para>
        /// </summary>
        public override async Task ExecutePostProcessingAsync()
        {
            // Nothing to do, we already processed the information in GetFileName
        }
    }
    
}