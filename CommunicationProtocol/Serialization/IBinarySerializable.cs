namespace CommunicationProtocol.Serialization
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
        /// <param name="BitCounter">Compteur de bits écrits dans le bitPacker à transmettre par référence tel quel.</param>
        /// <param name="Error">Résultat de sérialisation à transmettre par référence tel quel.</param>
        /// <returns></returns>
        bool Serialize(Serializer pSerializer);
    }
}
