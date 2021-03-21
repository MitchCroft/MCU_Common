using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to the Unity Engine Vector objects
    /// </summary>
    public static class VectorExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Retrieve the minimum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the smallest float stored within the vector</returns>
        public static float GetMin(this Vector2 vector) {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>
        /// Retrieve the minimum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the smallest float stored within the vector</returns>
        public static float GetMin(this Vector3 vector) {
            return Mathf.Min(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Retrieve the minimum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the smallest float stored within the vector</returns>
        public static float GetMin(this Vector4 vector) {
            return Mathf.Min(vector.x, vector.y, vector.z, vector.w);
        }

        /// <summary>
        /// Retrieve the maximum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the largest flaot stored within the vector</returns>
        public static float GetMax(this Vector2 vector) {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>
        /// Retrieve the maximum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the largest flaot stored within the vector</returns>
        public static float GetMax(this Vector3 vector) {
            return Mathf.Max(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Retrieve the maximum axis value from the current vector
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the largest flaot stored within the vector</returns>
        public static float GetMax(this Vector4 vector) {
            return Mathf.Max(vector.x, vector.y, vector.z, vector.w);
        }

        /// <summary>
        /// Retrieve the absolute values of the current Vector object
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the same values of the current Vector component, forced to positive</returns>
        public static Vector2 GetAbs(this Vector2 vector) {
            return new Vector2(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y)
            );
        }

        /// <summary>
        /// Retrieve the absolute values of the current Vector object
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the same values of the current Vector component, forced to positive</returns>
        public static Vector3 GetAbs(this Vector3 vector) {
            return new Vector3(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y),
                Mathf.Abs(vector.z)
            );
        }

        /// <summary>
        /// Retrieve the absolute values of the current Vector object
        /// </summary>
        /// <param name="vector">The Vector object to operate on</param>
        /// <returns>Returns the same values of the current Vector component, forced to positive</returns>
        public static Vector3 GetAbs(this Vector4 vector) {
            return new Vector4(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y),
                Mathf.Abs(vector.z),
                Mathf.Abs(vector.w)
            );
        }

        /// <summary>
        /// Retrieve an axis that is perpendicular to the current direction
        /// </summary>
        /// <param name="direction">The direction vector to be evaluated</param>
        /// <returns>Returns a perpendicular vector to the supplied direction</returns>
        /// <remarks>
        /// There are no guarantees as to the direction of the returned vector, only that it is perpendicular
        /// </remarks>
        public static Vector3 GetPerpendicularAxis(this Vector3 direction) {
            // Check that the direction is normalised
            if (!Mathf.Approximately(direction.sqrMagnitude, 1f))
                direction = direction.normalized;

            // Find the axis with the smallest distance to 0
            int i = -1;
            float smallest = float.MaxValue, buffer;
            for (int a = 0; a < 3; a++) {
                // Check the distance to 0
                if ((buffer = Mathf.Abs(direction[a])) < smallest) {
                    i = a;
                    smallest = buffer;
                }
            }

            // Create a cardinal direction to cross against
            Vector3 cardinal = Vector3.zero; cardinal[i] = 1f;

            // Get a perpendicular axis
            return Vector3.Cross(direction, cardinal);
        }

        /// <summary>
        /// Round the supplied Vector on all axis to the nearest increment value
        /// </summary>
        /// <param name="vector">The vector that is being processed</param>
        /// <param name="increment">The increment step that is to be round to</param>
        /// <returns>Returns a Vector with the values modified to the nearest increment</returns>
        public static Vector2 RoundToNearest(this Vector2 vector, float increment) {
            Vector2 rounded = Vector2.zero;
            for (int a = 0; a < 2; ++a)
                rounded[a] = (Mathf.FloorToInt(vector[a] / increment) + Mathf.RoundToInt((vector[a] % increment) / increment)) * increment;
            return rounded;
        }

        /// <summary>
        /// Round the supplied Vector on all axis to the nearest increment value
        /// </summary>
        /// <param name="vector">The vector that is being processed</param>
        /// <param name="increment">The increment step that is to be round to</param>
        /// <returns>Returns a Vector with the values modified to the nearest increment</returns>
        public static Vector3 RoundToNearest(this Vector3 vector, float increment) {
            Vector3 rounded = Vector3.zero;
            for (int a = 0; a < 3; ++a)
                rounded[a] = (Mathf.FloorToInt(vector[a] / increment) + Mathf.RoundToInt((vector[a] % increment) / increment)) * increment;
            return rounded;
        }

        /// <summary>
        /// Round the supplied Vector on all axis to the nearest increment value
        /// </summary>
        /// <param name="vector">The vector that is being processed</param>
        /// <param name="increment">The increment step that is to be round to</param>
        /// <returns>Returns a Vector with the values modified to the nearest increment</returns>
        public static Vector4 RoundToNearest(this Vector4 vector, float increment) {
            Vector4 rounded = Vector4.zero;
            for (int a = 0; a < 4; ++a)
                rounded[a] = (Mathf.FloorToInt(vector[a] / increment) + Mathf.RoundToInt((vector[a] % increment) / increment)) * increment;
            return rounded;
        }

        /// <summary>
        /// Check to see if the current Vector has any NaN values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is NaN</returns>
        public static bool IsNaN(this Vector2 vector) {
            return (float.IsNaN(vector.x) || float.IsNaN(vector.y));
        }

        /// <summary>
        /// Check to see if the current Vector has any NaN values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is NaN</returns>
        public static bool IsNaN(this Vector3 vector) {
            return (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z));
        }

        /// <summary>
        /// Check to see if the current Vector has any NaN values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is NaN</returns>
        public static bool IsNaN(this Vector4 vector) {
            return (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z) || float.IsNaN(vector.w));
        }

        /// <summary>
        /// Check to see if the current Vector has any infinite values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is infinite</returns>
        public static bool IsInfinite(this Vector2 vector) {
            return (float.IsInfinity(vector.x) || float.IsInfinity(vector.y));
        }

        /// <summary>
        /// Check to see if the current Vector has any infinite values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is infinite</returns>
        public static bool IsInfinite(this Vector3 vector) {
            return (float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z));
        }

        /// <summary>
        /// Check to see if the current Vector has any infinite values
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if any axis is infinite</returns>
        public static bool IsInfinite(this Vector4 vector) {
            return (float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z) || float.IsInfinity(vector.w));
        }

        /// <summary>
        /// Check to see if the current Vector is usable
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if the none of the axis of this vector are NaN or infinite</returns>
        public static bool IsUsable(this Vector2 vector) {
            return (!vector.IsNaN() && !vector.IsInfinite());
        }

        /// <summary>
        /// Check to see if the current Vector is usable
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if the none of the axis of this vector are NaN or infinite</returns>
        public static bool IsUsable(this Vector3 vector) {
            return (!vector.IsNaN() && !vector.IsInfinite());
        }

        /// <summary>
        /// Check to see if the current Vector is usable
        /// </summary>
        /// <param name="vector">The vector being checked</param>
        /// <returns>Returns true if the none of the axis of this vector are NaN or infinite</returns>
        public static bool IsUsable(this Vector4 vector) {
            return (!vector.IsNaN() && !vector.IsInfinite());
        }

        /// <summary>
        /// Determine if the current point is contained within the defined **2D** polygon
        /// </summary>
        /// <param name="point">The point in 2D space that is being tested</param>
        /// <param name="polygon">The points in 2D space that define the bounds of the polygon to test against</param>
        /// <returns>Returns true if the point is contained within the defined polygon</returns>
        public static bool IsPointInPolygon(this Vector3 point, params Vector3[] polygon) { return IsPointInPolygon(polygon, point); }

        /// <summary>
        /// Determine if the defined point is contained within the current **2D** polygon definition
        /// </summary>
        /// <param name="polygon">The points in 2D space that define the bounds of the polygon to test against</param>
        /// <param name="point">The point in 2D space that is being tested</param>
        /// <returns>Returns true if the point is contained within the defined polygon</returns>
        /// <remarks>
        /// This implementation is taken from TriangleNet.Geometry.Contour's IsPointInPolygon function
        /// </remarks>
        public static bool IsPointInPolygon(this Vector3[] polygon, Vector3 point) {
            bool inside = false;

            float x = point.x,
                  y = point.y;

            int count = polygon.Length;

            for (int i = 0, j = count - 1; i < count; i++) {
                if (((polygon[i].y < y && polygon[j].y >= y) || (polygon[j].y < y && polygon[i].y >= y))
                    && (polygon[i].x <= x || polygon[j].x <= x)) {
                    inside ^= (polygon[i].x + (y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < x);
                }

                j = i;
            }

            return inside;
        }
    }
}