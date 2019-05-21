
using System;

namespace CommunicationProtocol
{
    public struct PaquetA
    {
        int x;
        int y;
        int z;
        const int SerializationCheck = -1431655766;

        public bool Serialize(Serializer pSerializer)
        {
            bool result = true;
            pSerializer.Serialize(ref result, ref x, 0, 100);   // Ajouter les attributs min/max
            pSerializer.Serialize(ref result, ref y, 0, 4200);  // Ajouter les attributs min/max
            pSerializer.Serialize(ref result, ref z, 0, 800);   // Ajouter les attributs min/max

            int check = SerializationCheck;
            pSerializer.Serialize(ref result, ref check, 0, int.MaxValue);
            return result;
        }
    }
}
