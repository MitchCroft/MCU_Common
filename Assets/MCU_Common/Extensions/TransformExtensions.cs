using System;

using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Indicate the different axis with directional information embedded in the value
    /// </summary>
    /// <remarks>
    /// The enum values are set to be +- of each other that can be brought one closer to 0 to
    /// be able to retrieve the axis index of a Vector3 object that will be modified.
    /// 
    /// The Unity spacial system has:
    ///     Right == -Left
    ///     Down  == -Up
    ///     Back  == -Forward
    /// 
    /// The enum value stores this directional information as well as the index conversion
    /// </remarks>
    public enum EAxisDirection {
        Left = -1, Right = 1,
        Down = -2, Up = 2,
        Back = -3, Forward = 3
    }

    /// <summary>
    /// Provide additional functionality for <see cref="EAxisDirection"/> values
    /// </summary>
    public static class EAxisDirectionExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Get a unit vector from the supplied axis direction
        /// </summary>
        /// <param name="axisDirection">The axis direction that will be converted into a directional vector</param>
        /// <returns>Returns a local space direction based on the axis direction</returns>
        public static Vector3 ToUnitVector(this EAxisDirection axisDirection) {
            Vector3 dir = Vector3.zero;
            dir[Mathf.Abs((int)axisDirection) - 1] = Mathf.Sign((int)axisDirection);
            return dir;
        }
    }

    /// <summary>
    /// Provide additional functionality to the Transform component
    /// </summary>
    public static class TransformExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Reset the local transform values of the current object
        /// </summary>
        /// <param name="transform">The transform that is to be reset</param>
        public static void Reset(this Transform transform) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Set the lossy scale of the Transform component
        /// </summary>
        /// <param name="transform">The transform that is to be modified</param>
        /// <param name="scale">The new scale value to apply to the transform</param>
        public static void SetLossyScale(this Transform transform, Vector3 scale) {
            //Stash the previous parent
            Transform parent = transform.parent;

            //Remove the old parent
            transform.SetParent(null, true);

            //Set the new scale value
            transform.localScale = scale;

            //Re-apply the old parent
            if (parent) transform.SetParent(parent, true);
        }

        /// <summary>
        /// Transform a local scale value to a world scale
        /// </summary>
        /// <param name="transform">The transform that this operation will be operating on</param>
        /// <param name="scale">The local-space scale value that is to be converted</param>
        /// <returns>Returns the scale value converted to world-space</returns>
        public static Vector3 TransformScale(this Transform transform, Vector3 scale) { return transform.localToWorldMatrix.MultiplyScale(scale); }

        /// <summary>
        /// Inversely transform a world scale to local space
        /// </summary>
        /// <param name="transform">The transform that this operation will be operating on</param>
        /// <param name="scale">The world-space scale value that is to be converted</param>
        /// <returns>Returns the scale value converted to local-space</returns>
        public static Vector3 InverseTransformScale(this Transform transform, Vector3 scale) { return transform.worldToLocalMatrix.MultiplyScale(scale); }

        /// <summary>
        /// Retrieve an object describing the current local Transform state values
        /// </summary>
        /// <param name="transform">The transform that is to be summarised</param>
        /// <returns>Returns a <see cref="TransformState"/> object that contains the current local values</returns>
        public static TransformState GetLocalTransformState(this Transform transform) {
            return new TransformState(
                transform.localPosition,
                transform.localRotation,
                transform.localScale,
                transform.parent,
                transform.GetSiblingIndex()
            );
        }

        /// <summary>
        /// Retrieve an object describing the current world Transform state values
        /// </summary>
        /// <param name="transform">The transform that is to be summarised</param>
        /// <returns>Returns a <see cref="TransformState"/> object that contains the current world values</returns>
        public static TransformState GetWorldTransformState(this Transform transform) {
            return new TransformState(
                transform.position,
                transform.rotation,
                transform.lossyScale,
                transform.parent,
                transform.GetSiblingIndex()
            );
        }

        /// <summary>
        /// Retrieve a matrix object that represents the transformation applied to this object from it's local origin
        /// </summary>
        /// <param name="transform">The transform that is be used to retrieve the information</param>
        /// <returns>Returns a Matrix4x4 object containing the local transformation values</returns>
        public static Matrix4x4 GetLocalMatrix(this Transform transform) {
            return Matrix4x4.TRS(
                transform.localPosition,
                transform.localRotation,
                transform.localScale
            );
        }

        /// <summary>
        /// Retrieve a matrix object that represents the transformation applied to this object from the world origin
        /// </summary>
        /// <param name="transform">The transform that is be used to retrieve the information</param>
        /// <returns>Returns a Matrix4x4 object containing the world transformation values</returns>
        public static Matrix4x4 GetWorldTransform(this Transform transform) {
            return Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                transform.lossyScale
            );
        }

        /// <summary>
        /// Set the local state values of the current Transform
        /// </summary>
        /// <param name="transform">The transform that is to be modified</param>
        /// <param name="state">The Transform State values that are to be applied locally</param>
        /// <param name="modifyParent">Flags if the parent should also be modified by this operation</param>
        public static void SetLocalTransformState(this Transform transform, TransformState state, bool modifyParent = false) {
            //Reset the parent value
            if (modifyParent) {
                transform.parent = state.parent;
                transform.SetSiblingIndex(state.siblingIndex);
            }

            //Set the default values
            transform.localPosition = state.position;
            transform.localRotation = state.rotation;
            transform.localScale = state.scale;
        }

        /// <summary>
        /// Set the world state values of the current Transform
        /// </summary>
        /// <param name="transform">The transform that is to be modified</param>
        /// <param name="state">The Transform State values that are to be applied locally</param>
        /// <param name="modifyParent">Flags if the parent should also be modified by this operation</param>
        public static void SetWorldTransformState(this Transform transform, TransformState state, bool modifyParent = false) {
            //Stash the previous transform parent object
            if (!modifyParent) state.parent = transform.parent;

            //Remove the current parent reference
            transform.SetParent(null, true);

            //Set the scale value
            transform.localScale = state.scale;

            //Reset the transform parent
            transform.SetParent(state.parent);

            //Reset the sibling index
            if (modifyParent) transform.SetSiblingIndex(state.siblingIndex);

            //Set the remaining values
            transform.position = state.position;
            transform.rotation = state.rotation;
        }

        /// <summary>
        /// Retrieve all directional axis within a Transform 
        /// </summary>
        /// <param name="transform">The transform that is being evaluated</param>
        /// <returns>
        /// Returns a Vector3 array with 3 values in the form:
        ///     0 == X,
        ///     1 == Y,
        ///     2 == Z
        /// </returns>
        public static Vector3[] GetAxis(this Transform transform) {
            return new Vector3[] {
                transform.right,
                transform.up,
                transform.forward
            };
        }

        /// <summary>
        /// Provide the functionality to iterate over the directional axis of a transform
        /// </summary>
        /// <param name="transform">The transform that is being evaluated</param>
        /// <param name="index">The index of the axis to retrieve. 0 == X, 1 == Y, 2 == Z</param>
        /// <returns>Returns the axis corresponding with the index</returns>
        public static Vector3 GetAxis(this Transform transform, int index) {
            switch (index) {
                //3D Space
                case 0: return transform.right;
                case 1: return transform.up;
                case 2: return transform.forward;

                //Unknown
                default: throw new ArgumentException("Can't get Transform Axis for dimension index " + index + ". Use a value from 0-2");
            }
        }

        /// <summary>
        /// Provide the functionality to retrieve directional axis, according to standard directional enumeration
        /// </summary>
        /// <param name="transform">The transform that is being evaluated</param>
        /// <param name="direction">An <see cref="EAxisDirections"/> value defining the axis direction to retrieve</param>
        /// <returns>Returns the axis direction of the specified enumeration</returns>
        public static Vector3 GetAxis(this Transform transform, EAxisDirection direction) {
            switch (direction) {
                //Defined
                case EAxisDirection.Left:    return -transform.right;
                case EAxisDirection.Right:   return transform.right;
                case EAxisDirection.Down:    return -transform.up;
                case EAxisDirection.Up:      return transform.up;
                case EAxisDirection.Back:    return -transform.forward;
                case EAxisDirection.Forward: return transform.forward;

                //Undefined
                default: throw new ArgumentException("Can't get Transform Axis for EAxisDirection Value '" + (int)direction + "'. Use a valid direction");
            }
        }

        /// <summary>
        /// Retrieve the depth level of the current transform within it's hierarchy
        /// </summary>
        /// <param name="transform">The transform that is being evaluated</param>
        /// <returns>Returns a numerical value representing the depth of the transform in a hierarchy tree</returns>
        /// <remarks>
        /// The value returns follows the pattern of:
        ///     Root Scene Object           => 0
        ///         |-> Child               => 1
        ///             |-> Sub-Child       => 2
        ///                 etc.
        /// </remarks>
        public static int GetDepthLevel(this Transform transform) {
            //Store the depth level to processed
            int depth = 0;

            //Process until reached scene root
            while (transform.parent) {
                ++depth;
                transform = transform.parent;
            }

            //Return the final count
            return depth;
        }

        /// <summary>
        /// Construct a string that lists the hierarchy chain from the current object up
        /// </summary>
        /// <param name="transform">The transform that is being evaluated</param>
        /// <returns>Returns the evaluated chain as a string object</returns>
        public static string GetHierarchyChainString(this Transform transform) {
            //Store the chain as a string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //Loop up the chain
            Transform current = transform;
            do {
                //Get the name of the current transform
                char[] name = current.name.ToCharArray();

                //Reverse the characters (To adjust for the final flip)
                Array.Reverse(name);

                //Log the current object to the builder
                sb.Append(name);

                //Get the next transform up the chain
                current = current.parent;

                //If there is something else, add an arrow
                if (current) sb.Append(" >- ");
            } while (current);

            //Get the final characters
            char[] chain = sb.ToString().ToCharArray();

            //Flip the characters a final time
            Array.Reverse(chain);

            //Return the completed chain
            return new string(chain);
        }
    }

    /// <summary>
    /// Store state information about a Transform object
    /// </summary>
    [Serializable]
    public struct TransformState {
        /*----------Variables----------*/
        //PUBLIC

        [Tooltip("The position value of the Transform State")]
        public Vector3 position;

        [Tooltip("The rotation value of the Transform State")]
        public Quaternion rotation;

        [Tooltip("The scale value of the Transform State")]
        public Vector3 scale;

        [Tooltip("The Transform parent of the Transform State")]
        public Transform parent;

        [Tooltip("The Child index of the Transform in it's parent's list")]
        public int siblingIndex;

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise the object with default starting state values
        /// </summary>
        public TransformState(Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, int siblingIndex) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.parent = parent;
            this.siblingIndex = siblingIndex;
        }
    }
}