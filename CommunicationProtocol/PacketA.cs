namespace CommunicationProtocol
{
    public struct PacketA : IPacket
    {
        public int x;
        public int y;
        public int z;
        public float f;
        public string comment;
        const int SerializationCheck = -1431655766;

        public bool Serialize(Serializer pSerializer)
        {
            bool result = true;
            pSerializer.Serialize(ref result, ref x, -30, 100);
            pSerializer.Serialize(ref result, ref y, 0, 4200);
            pSerializer.Serialize(ref result, ref z, 0, 800);
            pSerializer.Serialize(ref result, ref f, 0, 100.2f, 0.100f);
            pSerializer.Serialize(ref result, ref comment, 16);

            int checkValue = SerializationCheck;
            //pSerializer.EndOfPacket(ref result, ref checkValue, 32);
            return result;
        }
    }
}
