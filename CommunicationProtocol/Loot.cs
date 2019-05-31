using CommunicationProtocol.Serialiser;
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
        public Vector2 Position { get; set; }
        public int NbOfAmmo { get; set; }
        public eAmmoType AmmoType { get; set; }

        public bool Serialize(Serializer pSerializer, ref int pBitCounter, ref bool pResult)
        {
            Vector2 position = Position;
            int nbOfAmmo = NbOfAmmo;
            int ammoType = (int)AmmoType;
            if (pResult)
            {
                pSerializer.Serialize(ref pBitCounter, ref pResult, ref position, Vector2.Zero, new Vector2(4096));
                pSerializer.Serialize(ref pBitCounter, ref pResult, ref nbOfAmmo, 0, 100);
                pSerializer.Serialize(ref pBitCounter, ref pResult, ref ammoType, 0, 1);
            }

            if (pSerializer is ReaderSerialize && pResult)
            {
                Position = position;
                NbOfAmmo = nbOfAmmo;
                AmmoType = (eAmmoType)ammoType;
            }

            ShouldBeSend = pResult;
            return pResult;
        }
    }
}
