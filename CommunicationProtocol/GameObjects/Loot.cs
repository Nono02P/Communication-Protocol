using CommunicationProtocol.ExtensionMethods;
using CommunicationProtocol.Serialization;
using System;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Loot : IActor
    {
        public bool ShouldBeSend { get; set; }
        public bool IsActive { get; set; }

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

        private bool _sendNbOfAmmo;
        private int _nbOfAmmo;
        public int NbOfAmmo
        {
            get { return _nbOfAmmo; }
            set
            {
                if (_nbOfAmmo != value)
                {
                    _nbOfAmmo = value;
                    _sendNbOfAmmo = true;
                }
            }
        }
        
        public eAmmoType AmmoType { get; set; }

        public bool Serialize(Serializer pSerializer)
        {
            bool isActive = IsActive;
            Vector2 position = Position;
            int nbOfAmmo = NbOfAmmo;
            int ammoType = (int)AmmoType;
            if (!pSerializer.Error)
            {
#if TRACE_LOG
                LogHelper.WriteToFile("Serialize Loot : ", this, Program.FileName);
                LogHelper.WriteToFile("     IsActive : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref isActive);
#if TRACE_LOG
                LogHelper.WriteToFile("     SendPosition : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref _sendPosition);
                if (_sendPosition)
                {
#if TRACE_LOG
                LogHelper.WriteToFile("     SendPosition : ", this, Program.FileName);
#endif
                    pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));
                }

#if TRACE_LOG
                LogHelper.WriteToFile("     SendNbOfAmmo : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref _sendNbOfAmmo);
                if (_sendNbOfAmmo)
                {
#if TRACE_LOG
                LogHelper.WriteToFile("     NbOfAmmo : ", this, Program.FileName);
#endif
                    pSerializer.Serialize(ref nbOfAmmo, 0, 100);
                }
#if TRACE_LOG
                LogHelper.WriteToFile("     AmmoType : ", this, Program.FileName);
#endif
                pSerializer.Serialize(ref ammoType, 0, 1);
            }

            if (pSerializer is ReaderSerialize && !pSerializer.Error)
            {
                IsActive = isActive;
                if (_sendPosition)
                    Position = position;

                if (_sendNbOfAmmo)
                    NbOfAmmo = nbOfAmmo;

                AmmoType = (eAmmoType)ammoType;
            }
#if TRACE_LOG
            LogHelper.WriteToFile("End of Bullet : ", this, Program.FileName);
#endif
            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }

        public void Random()
        {
            Random rnd = Program.Rnd;
            _sendPosition = true;
            Position = _position.Randomize(new Vector2(4096));
            NbOfAmmo = rnd.Next(100);
            AmmoType = (eAmmoType)Math.Round(rnd.NextDouble());
        }

        public bool Equals(IActor other)
        {
            if (other is Loot)
            {
                Loot o = (Loot)other;
                return IsActive == o.IsActive &
                    AmmoType == o.AmmoType &
                    NbOfAmmo == o.NbOfAmmo &
                    _position.ApplyResolution() == o._position.ApplyResolution();
            }
            return false;
        }
    }
}
