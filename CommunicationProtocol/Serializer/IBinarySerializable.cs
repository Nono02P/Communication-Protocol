namespace CommunicationProtocol.Serialiser
{
    public interface IBinarySerializable
    {
        /// <summary>
        /// Permet d'indiquer que l'objet doit être envoyé
        /// </summary>
        bool ShouldBeSend { get; }

        /// <summary>
        /// Méthode permettant de définir quelles sont les données que l'objet doit sérialiser/désérialiser
        /// </summary>
        /// <param name="pSerializer">Sérialiseur (pouvant être de lecture ou d'écriture).</param>
        /// <param name="pBitCounter">Compteur de bits écrits dans le bitPacker à transmettre par référence tel quel.</param>
        /// <param name="pResult">Résultat de sérialisation à transmettre par référence tel quel.</param>
        /// <returns></returns>
        bool Serialize(Serializer pSerializer, ref int pBitCounter, ref bool pResult);
    }
}
