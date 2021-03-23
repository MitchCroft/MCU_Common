using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to the UnityEngine Quaternion object
    /// </summary>
    public static class QuaternionExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Retrieve the rotation required to take the current Quaternion to the supplied
        /// </summary>
        /// <param name="one">The quaternion object that this function is being called on</param>
        /// <param name="two">The rotation that the object is being checked against</param>
        /// <returns>Returns a quaternion that contains the difference between the current and the supplied</returns>
        public static Quaternion GetDifference(this Quaternion one, Quaternion two) { return Quaternion.Inverse(one) * two; }

        /// <summary>
        /// Convert the supplied quaternion rotation into a matrix definition
        /// </summary>
        /// <param name="quaternion">The quaternion that is to be converted</param>
        /// <returns>Returns a rotational matrix based off the supplied rotation</returns>
        public static Matrix4x4 ToMatrix(this Quaternion quaternion) { return Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one); }
    }
}