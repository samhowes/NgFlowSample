using System;

namespace Flow.Net
{
    public class FlowFile
    {
        public FlowFile(string flowIdentifier, long numberOfChunks)
        {
            FlowIdentifier = flowIdentifier;
            ChunkArray = new bool[numberOfChunks];
            TotalChunksReceived = 0;
        }

        public bool[] ChunkArray { get; private set; }

        /// <summary>
        /// Chunks can come out of order, so we must track how many chunks 
        /// we have successfully recieved to determine if the download is complete.
        /// </summary>
        public int TotalChunksReceived { get; private set; }

        public string FlowIdentifier { get; private set; }

        public DateTime CompletedDateTime { get; set; }

        public bool IsComplete => TotalChunksReceived == ChunkArray.Length;

        public virtual void RegisterChunk(FlowChunk chunk)
        {
            //todo lock this
            ChunkArray[ChunkIndex(chunk.Number)] = true;
            TotalChunksReceived++;
            if (IsComplete)
            {
                CompletedDateTime = DateTime.Now;
            }
        }
        
        public bool HasChunk(FlowChunk chunk)
        {
            // todo lock this
            var chunkIndex = ChunkIndex(chunk.Number);
            return ChunkArray[chunkIndex];
        }

        private long ChunkIndex(long chunkNumber)
        {
            return chunkNumber - 1;
        }
    }
}