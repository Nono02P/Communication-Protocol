namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionFloat
    {
        /// <summary>
        /// Round the value depending of the applied resolution.
        /// </summary>
        /// <param name="pValue">Value to round.</param>
        /// <param name="pResolution">Resolution to apply. 
        /// Exemple : If value is 21.453f and resolution is 0.01f, the result will be 21.45f</param>
        /// <returns>Return the result of the resolution applied on the value.</returns>
        public static float ApplyResolution(this ref float pValue, float pResolution = 1)
        {
            return (float)((int)((decimal)pValue / (decimal)pResolution) * (decimal)pResolution);
        }
    }
}
