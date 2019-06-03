using CommunicationProtocol.Serialization;
using System.Numerics;

namespace CommunicationProtocol
{
    public class Loot : IActor
    {
        public enum eAmmoType
        {
            Bullet,
            Grenada
        }

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
                pSerializer.Serialize(ref isActive);
                pSerializer.Serialize(ref _sendPosition);
                if (_sendPosition)
                    pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));
                pSerializer.Serialize(ref _sendNbOfAmmo);
                if (_sendNbOfAmmo)
                    pSerializer.Serialize(ref nbOfAmmo, 0, 100);
                pSerializer.Serialize(ref ammoType, 0, 1);
            }

            if (pSerializer is ReaderSerialize && !pSerializer.Error)
            {
                IsActive = isActive;
                Position = position;
                NbOfAmmo = nbOfAmmo;
                AmmoType = (eAmmoType)ammoType;
            }

            ShouldBeSend = pSerializer.Error;
            return pSerializer.Error;
        }
    }
}
