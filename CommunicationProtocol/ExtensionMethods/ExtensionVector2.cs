using System;
using System.Numerics;

namespace CommunicationProtocol.ExtensionMethods
{
    public static class ExtensionVector2
    {
        /// <summary>
        /// Round the vector depending of the applied resolution.
        /// </summary>
        /// <param name="pVector">Vector to round.</param>
        /// <param name="pResolution">Resolution to apply. 
        /// Exemple : If value is ''21.453f, 14.124f'' and resolution is 0.01f, the result will be ''21.45f, 14.12f''.</param>
        /// <returns>Return the result of the resolution applied on the vector.</returns>
        public static Vector2 ApplyResolution(this ref Vector2 pVector, float pResolution = 1)
        {
            return new Vector2(pVector.X.ApplyResolution(pResolution), pVector.Y.ApplyResolution(pResolution));
        }

        /// <summary>
        /// Randomize a vector between min and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMinX">The min value of X.</param>
        /// <param name="pMaxX">The max value of X.</param>
        /// <param name="pMinY">The min value of Y.</param>
        /// <param name="pMaxY">The max value of Y.</param>
        /// <returns>Return a randomized vector.</returns>
        public static Vector2 Randomize(this ref Vector2 pVector, float pMinX, float pMaxX, float pMinY, float pMaxY)
        {
            Random rnd = Program.Rnd;
            float xRange = pMaxX - pMinX;
            float yRange = pMaxY - pMinY;
            float xVal = (float)(rnd.NextDouble() * xRange + pMinX);
            float yVal = (float)(rnd.NextDouble() * yRange + pMinY);
            return pVector = new Vector2(xVal, yVal);
        }

        /// <summary>
        /// Randomize a vector between 0, 0 and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMax">The max value for X, Y.</param>
        /// <returns>Return a randomized vector.</returns>
        public static Vector2 Randomize(this ref Vector2 pVector, Vector2 pMax)
        {
            return Randomize(ref pVector, 0, pMax.X, 0, pMax.Y);
        }


        /// <summary>
        /// Randomize a vector between min and max.
        /// </summary>
        /// <param name="pVector">The vector reference to randomize.</param>
        /// <param name="pMin">The min value for X, Y.</param>
        /// <param name="pMax">The max value for X, Y.</param>
        /// <returns>Return a randomized vector.</returns>
        public static Vector2 Randomize(this ref Vector2 pVector, Vector2 pMin, Vector2 pMax)
        {
            return Randomize(ref pVector, pMin.X, pMax.X, pMin.Y, pMax.Y);
        }
    }
}
