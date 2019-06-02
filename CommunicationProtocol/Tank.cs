using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Tank : IActor
    {
        public bool ShouldBeSend { get; set; }
        public Vector2 Position { get; set; }
        public int Life { get; set; }
        

        public bool Serialize(Serializer pSerializer)
        {
            Vector2 position = Position;
            int life = Life;
            if (!pSerializer.Error)
            {
                pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));
                pSerializer.Serialize(ref life, 0, 100);
            }

            if (pSerializer is ReaderSerialize && !pSerializer.Error)
            {
                Position = position;
                Life = life;
            }

            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }
    }
}
