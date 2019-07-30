namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionFloat
    {
        public static float ApplyResolution(this ref float pValue, float pResolution = 1)
        {
            return (float)((int)((decimal)pValue / (decimal)pResolution) * (decimal)pResolution);
        }
    }
}
