using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to the Unity Matrix4x4 object
    /// </summary>
    public static class Matrix4x4Extensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Transform the supplied scale by the current matrix
        /// </summary>
        /// <param name="mat">The matrix that this operation is utilising</param>
        /// <param name="scale">The scale vector that will be multiplied</param>
        /// <returns>Returns the supplied scale converted by the current matrix</returns>
        public static Vector3 MultiplyScale(this Matrix4x4 mat, Vector3 scale) {
            // Create a scale matrix
            Matrix4x4 sclMat = Matrix4x4.Scale(new Vector3(
                mat.GetColumn(0).magnitude,
                mat.GetColumn(1).magnitude,
                mat.GetColumn(2).magnitude
            ));

            // Convert the scale with the matrix
            return sclMat.MultiplyVector(scale);
        }

        /// <summary>
        /// Convert the supplied matrix information into a quaternion object
        /// </summary>
        /// <param name="rotMat">The rotational matrix that will be converted into a quaternion description</param>
        /// <returns>Returns a quaternion representation of the supplied matrix</returns>
        /// <remarks>
        /// Implementation taken from:
        /// http://answers.unity.com/answers/11372/view.html
        /// </remarks>
        public static Quaternion ToQuaternion(this Matrix4x4 rotMat) {
            Quaternion q = new Quaternion();
            q.w  = Mathf.Sqrt(Mathf.Max(0, 1 + rotMat[0, 0] + rotMat[1, 1] + rotMat[2, 2])) / 2;
            q.x  = Mathf.Sqrt(Mathf.Max(0, 1 + rotMat[0, 0] - rotMat[1, 1] - rotMat[2, 2])) / 2;
            q.y  = Mathf.Sqrt(Mathf.Max(0, 1 - rotMat[0, 0] + rotMat[1, 1] - rotMat[2, 2])) / 2;
            q.z  = Mathf.Sqrt(Mathf.Max(0, 1 - rotMat[0, 0] - rotMat[1, 1] + rotMat[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (rotMat[2, 1] - rotMat[1, 2]));
            q.y *= Mathf.Sign(q.y * (rotMat[0, 2] - rotMat[2, 0]));
            q.z *= Mathf.Sign(q.z * (rotMat[1, 0] - rotMat[0, 1]));
            return q;
        }
    }
}