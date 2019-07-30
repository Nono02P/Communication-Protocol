using CommunicationProtocol.Serialization;
using System;
using System.Numerics;

namespace CommunicationProtocol
{
    public interface IActor : IBinarySerializable, IEquatable<IActor>
    {
        Vector2 Position { get; }
    }
}
