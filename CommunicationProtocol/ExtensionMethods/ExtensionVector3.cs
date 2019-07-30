using System;
using System.Numerics;

namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionVector3
    {
        public static Vector3 ApplyResolution(this ref Vector3 pVector, float pResolution = 1)
        {
            return new Vector3(pVector.X.ApplyResolution(pResolution), pVector.Y.ApplyResolution(pResolution), pVector.Z.ApplyResolution(pResolution));
        }

        public static Vector3 Randomize(this ref Vector3 pVector, float pMinX, float pMaxX, float pMinY, float pMaxY, float pMinZ, float pMaxZ)
        {
            Random rnd = Program.Rnd;
            float xRange = pMaxX - pMinX;
            float yRange = pMaxY - pMinY;
            float zRange = pMaxZ - pMinZ;
            float xVal = (float)(rnd.NextDouble() * xRange + pMinX);
            float yVal = (float)(rnd.NextDouble() * yRange + pMinY);
            float zVal = (float)(rnd.NextDouble() * zRange + pMinZ);

            pVector.X = xVal;
            pVector.Y = yVal;
            pVector.Z = zVal;
            return pVector;
        }

        public static Vector3 Randomize(this ref Vector3 pVector, Vector3 pMax)
        {
            return Randomize(ref pVector, 0, pMax.X, 0, pMax.Y, 0, pMax.Z);
        }

        public static Vector3 Randomize(this ref Vector3 pVector, Vector3 pMin, Vector3 pMax)
        {
            return Randomize(ref pVector, pMin.X, pMax.X, pMin.Y, pMax.Y, pMin.Z, pMax.Z);
        }
    }
}
