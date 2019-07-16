using CommunicationProtocol.Serialization;
using System.Collections.Generic;

namespace CommunicationProtocol.Frames.Packets
{
    public struct PacketB : IPacket
    {
        public List<IActor> Actors;
        const int SerializationCheck = -1431655766;

        public bool Serialize(Serializer pSerializer)
        {            
            bool error = pSerializer.Serialize(Actors, 255, true);

            if (pSerializer is WriterSerialize && !error)
            {
                // TODO : vider le sérialiseur du nombre de bits à virer.
                // bitCounter
            }

            int checkValue = SerializationCheck;
            // pSerializer.EndOfPacket(ref result, ref checkValue, 32);
            return !error;
        }
    }
}
