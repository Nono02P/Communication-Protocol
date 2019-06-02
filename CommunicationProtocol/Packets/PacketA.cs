using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol.Packets
{
    public struct PacketA : IPacket
    {
        private int _alreadyWriteCounter;

        public Vector3 Position;
        public float f;
        public string comment;
        const int SerializationCheck = -1431655766;

        public bool Serialize(Serializer pSerializer)
        {
            // TODO : Ajouter un compteur de bits pour retirer les bits écrits en cas d'erreur à la sérialisation.
            bool error = false;
            if (!pSerializer.Serialize(ref Position, new Vector3(-30, 0, 0), new Vector3(100, 700, 190), 0.01f))
                error = true;
            if (pSerializer.Serialize(ref f, 0, 100.2f, 0.01f))
                error = true;
            if (pSerializer.Serialize(ref comment, 16))
                error = true;

            int checkValue = SerializationCheck;
            //pSerializer.EndOfPacket(ref result, ref checkValue, 32);
            return !error;
        }
    }
}
