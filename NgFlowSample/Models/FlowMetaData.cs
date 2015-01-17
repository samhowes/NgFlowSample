using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace NgFlowSample.Models
{
    public class FlowMetaData
    {
        public FlowMetaData() { }

        public FlowMetaData(Dictionary<string, string> values)
        {
            FlowChunkNumber = Int64.Parse(values["flowChunkNumber"]);
            FlowChunkSize = Int64.Parse(values["flowChunkSize"]);
            FlowCurrentChunkSize = Int64.Parse(values["flowCurrentChunkSize"]);
            FlowTotalSize = Int64.Parse(values["flowTotalSize"]);
            FlowIdentifier = values["flowIdentifier"];
            FlowFilename = values["flowFilename"];
            FlowRelativePath = values["flowRelativePath"];
            FlowTotalChunks = Int64.Parse(values["flowTotalChunks"]);
            // Oh no! I forgot to add "RequestVerificationToken" and "BlueElephant" to this list.
            // Good thing I dynamically parse the object when I create this instead of using this method.
            // otherwise, those properties will be null!
        }

        

        /// <summary>
        /// The index of the chunk in the current upload. First chunk is 1 (no base-0 counting here).
        /// </summary>
        public long FlowChunkNumber { get; set; }

        /// <summary>
        /// The total number of chunks.
        /// </summary>
        public long FlowChunkSize { get; set; }
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
        public string FlowIdentifier { get; set; }
        /// <summary>
        /// The original file name (since a bug in Firefox results in the file name not being transmitted in chunk multipart posts).
        /// </summary>
        public string FlowFilename { get; set; }
        /// <summary>
        /// The file's relative path when selecting a directory (defaults to file name in all browsers except Chrome).
        /// </summary>
        public string FlowRelativePath { get; set; }

        public long FlowTotalChunks { get; set; }

        public long FileOffset
        {
            get { return FlowChunkSize * (FlowChunkNumber - 1); }
        }

        #region Custom data for the Flow Query parameter

        public string RequestVerificationToken { get; set; }

        public int BlueElephant { get; set; }

        #endregion

    }
}