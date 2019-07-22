using CommunicationProtocol.Serialization;
using System;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Bullet : IActor
    {
        public bool ShouldBeSend { get; set; }
        private Tank _parent;

        public Tank Parent
        {
            get { return _parent; }
            set { if (_parent == null) _parent = value; }
        }


        private bool _sendPosition;
        private Vector2 _position;

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _sendPosition = true;
                }
            }
        }


        private bool _sendVelocity;
        private Vector2 _velocity;

        public Vector2 Velocity
        {
            get { return _velocity; }
            set
            {
                if (_velocity != value)
                {
                    _velocity = value;
                    _sendVelocity = true;
                }
            }
        }


        private bool _sendType;
        private eAmmoType _type;

        public eAmmoType Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    _sendType = true;
                }
            }
        }


        public bool Serialize(Serializer pSerializer)
        {
            Vector2 position = Position;
            Vector2 velocity = Velocity;
            int type = (int)Type;

            if (!pSerializer.Error)
            {
                pSerializer.Serialize(ref _sendPosition);
                if (_sendPosition)
                    pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));

                pSerializer.Serialize(ref _sendVelocity);
                if (_sendVelocity)
                    pSerializer.Serialize(ref velocity, Vector2.Zero, new Vector2(30));

                pSerializer.Serialize(ref _sendType);
                if (_sendType)
                    pSerializer.Serialize(ref type, 0, 1);
            }

            if (pSerializer is ReaderSerialize && !pSerializer.Error)
            {
                if (_sendPosition)
                    Position = position;
                if (_sendVelocity)
                    Velocity = velocity;
                if (_sendType)
                    Type = (eAmmoType)type;
            }

            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }

        public void Random()
        {
            Position = _position.Randomize(new Vector2(4096));
            Velocity = _velocity.Randomize(new Vector2(30));
            Type = (eAmmoType)Math.Round(Program.Rnd.NextDouble());
        }
    }
}
