using CommunicationProtocol.Serialiser;
using System.Numerics;

namespace CommunicationProtocol
{
    public interface IActor : IBinarySerializable
    {
        Vector2 Position { get; }
    }
}
