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
        /// <returns></returns>
        bool Serialize(Serializer pSerializer);

        /// <summary>
        /// Méthode permettant de remplir l'objet avec des valeurs au hasard.
        /// </summary>
        void Random();
    }
}
