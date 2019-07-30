using System;
using System.Numerics;

namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionVector2
    {
        public static Vector2 ApplyResolution(this ref Vector2 pVector, float pResolution = 1)
        {
            return new Vector2(pVector.X.ApplyResolution(pResolution), pVector.Y.ApplyResolution(pResolution));
        }

        public static Vector2 Randomize(this ref Vector2 pVector, float pMinX, float pMaxX, float pMinY, float pMaxY)
        {
            Random rnd = Program.Rnd;
            float xRange = pMaxX - pMinX;
            float yRange = pMaxY - pMinY;
            float xVal = (float)(rnd.NextDouble() * xRange + pMinX);
            float yVal = (float)(rnd.NextDouble() * yRange + pMinY);
            return pVector = new Vector2(xVal, yVal);
        }

        public static Vector2 Randomize(this ref Vector2 pVector, Vector2 pMax)
        {
            return Randomize(ref pVector, 0, pMax.X, 0, pMax.Y);
        }

        public static Vector2 Randomize(this ref Vector2 pVector, Vector2 pMin, Vector2 pMax)
        {
            return Randomize(ref pVector, pMin.X, pMax.X, pMin.Y, pMax.Y);
        }
    }
}
