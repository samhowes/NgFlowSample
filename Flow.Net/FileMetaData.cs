namespace Flow.Net
{
    /// <summary>
    /// Our own internal metadata to track the progress of a download. 
    /// </summary>
    public class FileMetaData
    {
        private static long ChunkIndex(long chunkNumber)
        {
            return chunkNumber - 1;
        }

        public FileMetaData(FlowMetaData flowMeta)
        {
            FlowMeta = flowMeta;
            ChunkArray = new bool[flowMeta.FlowTotalChunks];
            TotalChunksReceived = 0;
        }

        public bool[] ChunkArray { get; set; }

        /// <summary>
        /// Chunks can come out of order, so we must track how many chunks 
        /// we have successfully recieved to determine if the download is complete.
        /// </summary>
        public int TotalChunksReceived { get; set; }

        public FlowMetaData FlowMeta { get; set; }

        public string FlowIdentifier => FlowMeta.FlowIdentifier;

        public bool IsComplete
        {
            get
            {
                return (TotalChunksReceived == FlowMeta.FlowTotalChunks);
            }
        }

        public void RegisterChunkAsReceived(FlowMetaData flowMeta)
        {
            ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)] = true;
            TotalChunksReceived++;
        }

        public bool HasChunk(FlowMetaData flowMeta)
        {
            // todo lock this
            return ChunkArray[ChunkIndex(flowMeta.FlowChunkNumber)];
        }
    }
}