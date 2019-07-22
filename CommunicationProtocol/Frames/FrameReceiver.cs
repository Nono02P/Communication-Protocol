using CommunicationProtocol.Frames.Packets;
using CommunicationProtocol.Serialization;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames
{
    public class FrameReceiver : Frame
    {
        public FrameReceiver() : base()
        {
            Serializer = new ReaderSerialize();
        }

        public List<Packet> Receive(byte[] pData)
        {
            return null;
        }
    }
}
