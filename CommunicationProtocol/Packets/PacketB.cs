using CommunicationProtocol.Serialiser;
using System.Collections.Generic;

namespace CommunicationProtocol.Packets
{
    public struct PacketB : IPacket
    {
        private int _alreadyWriteCounter;

        public List<IActor> Actors;
        const int SerializationCheck = -1431655766;

        public bool Serialize(Serializer pSerializer)
        {
            // TODO : Ajouter un compteur de bits pour retirer les bits écrits en cas d'erreur à la sérialisation.
            bool result = true;
            int bitCounter = 0;

            pSerializer.Serialize(ref bitCounter, ref result, Actors, 255);

            int checkValue = SerializationCheck;
            //pSerializer.EndOfPacket(ref result, ref checkValue, 32);
            return result;
        }
    }
}
