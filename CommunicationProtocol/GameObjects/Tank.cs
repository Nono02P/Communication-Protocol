using CommunicationProtocol.ExtensionMethods;
using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Tank : IActor
    {
        public List<Bullet> Bullets { get; private set; }

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

        public Tank()
        {
            Bullets = new List<Bullet>();
        }

        public void Shoot()
        {
            Random rnd = Program.Rnd;
            int nb = 10; //rnd.Next(100);
            for (int i = 0; i < nb; i++)
            {
                Bullets.Add(new Bullet() { Position = new Vector2(rnd.Next(4096)), Velocity = new Vector2(rnd.Next(30)), Type = (eAmmoType)rnd.Next(2), ShouldBeSend = true } );
            }
        }

        public bool Serialize(Serializer pSerializer)
        {
            Vector2 position = Position;
            string name = Name;
            int life = Life;

            if (!pSerializer.Error)
            {
#if TRACE
                Trace.WriteLine("Serialize Tank : ");
                Trace.Indent();
                Trace.WriteLine("Bullets : ");
#endif
                pSerializer.Serialize(Bullets, 255, true, delegate (Bullet b) { b.Parent = this; });

#if TRACE
                Trace.WriteLine("SendName : ");
#endif
                pSerializer.Serialize(ref name, 50, ref _sendName);

#if TRACE
                Trace.WriteLine("SendPosition : ");
#endif
                pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096), ref _sendPosition);

#if TRACE
                Trace.WriteLine("SendLife : ");
#endif
                pSerializer.Serialize(ref life, 0, 100, ref _sendLife);
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
#if TRACE
            Trace.Unindent();
            Trace.WriteLine("End of Tank : ");
#endif

            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }

        public void Random()
        {
            _sendPosition = true;
            Position = _position.Randomize(new Vector2(0, 4096));
            Life = Program.Rnd.Next(100);
            Shoot();
        }

        public bool Equals(IActor other)
        {
            if (other is Tank)
            {
                Tank o = (Tank)other;
                bool checkString = false;
                if (string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(o._name))
                    checkString = true;
                else
                    checkString = _name == o._name;
                if (_life == o._life & checkString & _position.ApplyResolution() == o._position.ApplyResolution() & Bullets.Count == o.Bullets.Count)
                {
                    for (int i = 0; i < Bullets.Count; i++)
                    {
                        Bullet itemA = Bullets[i];
                        Bullet itemB = Bullets[i];
                        if (!itemA.Equals(itemB))
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
