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
        public Vector2 Position { get; set; }
        public int NbOfAmmo { get; set; }
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
                pSerializer.Serialize(ref position, Vector2.Zero, new Vector2(4096));
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
