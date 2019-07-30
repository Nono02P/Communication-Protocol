using System;
using System.Numerics;

namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionVector3
    {
        /// <summary>
        /// Round the vector depending of the applied resolution.
        /// </summary>
        /// <param name="pVector">Vector to round.</param>
        /// <param name="pResolution">Resolution to apply. 
        /// Exemple : If value is ''21.453f, 14.124f, 1.0334f'' and resolution is 0.01f, the result will be ''21.45f, 14.12f, 1.03f''.</param>
        /// <returns>Return the result of the resolution applied on the vector.</returns>
        public static Vector3 ApplyResolution(this ref Vector3 pVector, float pResolution = 1)
        {
            return new Vector3(pVector.X.ApplyResolution(pResolution), pVector.Y.ApplyResolution(pResolution), pVector.Z.ApplyResolution(pResolution));
        }

        /// <summary>
        /// Randomize a vector between min and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMinX">The min value of X.</param>
        /// <param name="pMaxX">The max value of X.</param>
        /// <param name="pMinY">The min value of Y.</param>
        /// <param name="pMaxY">The max value of Y.</param>
        /// <param name="pMinZ">The min value of Z.</param>
        /// <param name="pMaxZ">The max value of Z.</param>
        /// <returns>Return a randomized vector.</returns>
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

        /// <summary>
        /// Randomize a vector between 0, 0, 0 and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMax">The max value for X, Y, Z.</param>
        /// <returns>Return a randomized vector.</returns>
        public static Vector3 Randomize(this ref Vector3 pVector, Vector3 pMax)
        {
            return Randomize(ref pVector, 0, pMax.X, 0, pMax.Y, 0, pMax.Z);
        }

        /// <summary>
        /// Randomize a vector between min and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMin">The min value for X, Y, Z.</param>
        /// <param name="pMax">The max value for X, Y, Z.</param>
        /// <returns>Return a randomized vector.</returns>
        public static Vector3 Randomize(this ref Vector3 pVector, Vector3 pMin, Vector3 pMax)
        {
            return Randomize(ref pVector, pMin.X, pMax.X, pMin.Y, pMax.Y, pMin.Z, pMax.Z);
        }
    }
}
