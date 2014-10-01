using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NgFlowSample.Models
{
    public class FlowMetaData
    {
        public FlowMetaData() { }

        public FlowMetaData(Dictionary<string, string> values)
        {
            FlowChunkNumber = Int32.Parse(values["flowChunkNumber"]);
            FlowChunkSize = Int32.Parse(values["flowChunkSize"]);
            FlowCurrentChunkSize = Int32.Parse(values["flowCurrentChunkSize"]);
            FlowTotalSize = Int32.Parse(values["flowTotalSize"]);
            FlowIdentifier = values["flowIdentifier"];
            FlowFilename = values["flowFilename"];
            FlowRelativePath = values["flowRelativePath"];
            FlowTotalChunks = Int32.Parse(values["flowTotalChunks"]);
        }

        /// <summary>
        /// The index of the chunk in the current upload. First chunk is 1 (no base-0 counting here).
        /// </summary>
        public int FlowChunkNumber { get; set; }

        /// <summary>
        /// The total number of chunks.
        /// </summary>
        public int FlowChunkSize { get; set; }
        /// <summary>
        /// The general chunk size. Using this value and flowTotalSize you can calculate the total number of chunks. Please note that the size of the data received in the HTTP might be lower than flowChunkSize of this for the last chunk for a file.
        /// </summary>
        public int FlowCurrentChunkSize { get; set; }
        /// <summary>
        /// The total file size.
        /// </summary>
        public int FlowTotalSize { get; set; }
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

        public int FlowTotalChunks { get; set; }

        public long FileOffset
        {
            get { return FlowChunkSize * (FlowChunkNumber - 1); }
        }

    }
}