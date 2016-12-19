using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Flow.Net
{
    public class FlowChunk
    {
        public FlowChunk() { }

        public FlowChunk(Dictionary<string, string> values)
        {
            //todo don't parse these values here
            Number = Int64.Parse(values["flowChunkNumber"]);
            MaximumChunkSize = Int64.Parse(values["flowChunkSize"]);
            FlowCurrentChunkSize = Int64.Parse(values["flowCurrentChunkSize"]);
            FlowTotalSize = Int64.Parse(values["flowTotalSize"]);
            FileIdentifier = values["flowIdentifier"];
            FlowFilename = values["flowFilename"];
            FlowRelativePath = values["flowRelativePath"];
            TotalChunks = Int64.Parse(values["flowTotalChunks"]);


            //todo come up with a way to have dynamic values
            // Oh no! I forgot to add "RequestVerificationToken" and "BlueElephant" to this list.
            // Good thing I dynamically parse the object when I create this instead of using this method.
            // otherwise, those properties will be null!
        }



        /// <summary>
        /// The index of the chunk in the current upload. First chunk is 1 (no base-0 counting here).
        /// </summary>
        [JsonProperty("flowChunkNumber")]
        public long Number { get; set; }

        /// <summary>
        /// The general chunk size. Using this value and flowTotalSize you can calculate the total number of 
        /// chunks. Please note that the size of the data received in the HTTP might be lower than flowChunkSize 
        /// of this for the last chunk for a file.
        /// </summary>
        [JsonProperty("flowChunkSize")]
        public long MaximumChunkSize { get; set; }

        /// <summary>
        /// The general chunk size. Using this value and flowTotalSize you can calculate the total number of chunks. Please note that the size of the data received in the HTTP might be lower than flowChunkSize of this for the last chunk for a file.
        /// </summary>
        public long FlowCurrentChunkSize { get; set; }

        /// <summary>
        /// The total file size.
        /// </summary>
        public long FlowTotalSize { get; set; }
        /// <summary>
        /// A unique identifier for the file contained in the request.
        /// </summary>
        public string FileIdentifier { get; set; }
        /// <summary>
        /// The original file name (since a bug in Firefox results in the file name not being transmitted in chunk multipart posts).
        /// </summary>
        public string FlowFilename { get; set; }
        /// <summary>
        /// The file's relative path when selecting a directory (defaults to file name in all browsers except Chrome).
        /// </summary>
        public string FlowRelativePath { get; set; }

        public long TotalChunks { get; set; }

        public long FileOffset
        {
            get { return MaximumChunkSize * (Number - 1); }
        }

        #region Custom data for the Flow Query parameter

        public string RequestVerificationToken { get; set; }

        public int BlueElephant { get; set; }

        #endregion

    }
}