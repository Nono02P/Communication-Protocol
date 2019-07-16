using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Tank : IActor
    {

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


        public bool ShouldBeSend { get; set; }



        private bool _sendName;
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    _sendName = true;
                }
            }
        }


        private bool _sendLife;
        private int _life;

        public int Life
        {
            get { return _life; }
            set
            {
                if (_life != value)
                {
                    _life = value;
                    _sendLife = true;
                }
            }
        }



        public bool Serialize(Serializer pSerializer)
        {
            Vector2 position = Position;
            string name = Name;
            int life = Life;
            if (!pSerializer.Error)
            {
                pSerializer.Serialize(ref _sendName);
                if(_sendName)
                    pSerializer.Serialize(ref name, 50);

                pSerializer.Serialize(ref _sendPosition);
                if (_sendPosition)
                    pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));

                pSerializer.Serialize(ref _sendLife);
                if (_sendLife)
                    pSerializer.Serialize(ref life, 0, 100);
            }

            if (pSerializer is ReaderSerialize && !pSerializer.Error)
            {
                if (_sendName)
                    Name = name;
                if (_sendPosition)
                    Position = position;
                if (_sendLife)
                    Life = life;
            }

            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }
    }
}
