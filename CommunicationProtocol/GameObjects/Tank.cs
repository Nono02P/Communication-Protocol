using CommunicationProtocol.Serialization;
using System;
using System.Collections.Generic;
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
            Random rnd = new Random();
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
#if TRACE_LOG
                LogHelper.WriteToFile("Serialize Tank : ", this, Program.FileName);
                LogHelper.WriteToFile("     Bullets : ", this, Program.FileName);
#endif
                pSerializer.Serialize(Bullets, 255, true, delegate (Bullet b) { b.Parent = this; });

#if TRACE_LOG
                LogHelper.WriteToFile("     SendName : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref _sendName);
                if (_sendName)
                {
#if TRACE_LOG
                    LogHelper.WriteToFile("     Name : ", this, Program.FileName);
#endif
                    pSerializer.Serialize(ref name, 50);
                }

#if TRACE_LOG
                LogHelper.WriteToFile("     SendPosition : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref _sendPosition);
                if (_sendPosition)
                {
#if TRACE_LOG
                    LogHelper.WriteToFile("     Position : ", this, Program.FileName);
#endif
                    pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));
                }

#if TRACE_LOG
                LogHelper.WriteToFile("     SendLife : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref _sendLife);
                if (_sendLife)
                {
#if TRACE_LOG
                    LogHelper.WriteToFile("     Life : ", this, Program.FileName);
#endif
                    pSerializer.Serialize(ref life, 0, 100);
                }
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
#if TRACE_LOG
            LogHelper.WriteToFile("End of Tank : ", this, Program.FileName);
#endif
            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }

        public void Random()
        {
            Position = _position.Randomize(new Vector2(0, 4096));
            Life = Program.Rnd.Next(100);
            Shoot();
        }
    }
}
