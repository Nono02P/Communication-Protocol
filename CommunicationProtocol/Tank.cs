using CommunicationProtocol.Serialiser;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Tank : IActor
    {
        public bool ShouldBeSend { get; set; }
        public Vector2 Position { get; set; }
        public int Life { get; set; }
        

        public bool Serialize(Serializer pSerializer, ref int pBitCounter, ref bool pResult)
        {
            Vector2 position = Position;
            int life = Life;
            if (pResult)
            {
                pSerializer.Serialize(ref pBitCounter, ref pResult, ref position, Vector2.Zero, new Vector2(4096));
                pSerializer.Serialize(ref pBitCounter, ref pResult, ref life, 0, 100);
            }

            if (pSerializer is ReaderSerialize && pResult)
            {
                Position = position;
                Life = life;
            }

            ShouldBeSend = pResult;
            return pResult;
        }
    }
}
