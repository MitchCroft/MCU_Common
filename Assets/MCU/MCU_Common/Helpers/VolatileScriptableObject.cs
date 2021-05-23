using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MCU.Helpers
{
    /// <summary>
    /// A Scriptable Object base class whose serial value changes won't persist between playback states
    /// </summary>
    /// <remarks>
    /// Intended for objects containing data that will be modified at play time but is not desired to
    /// be saved with the asset
    /// </remarks>
    public abstract class VolatileScriptableObject : ScriptableObject
    {
        /*----------Variables----------*/
        //PRIVATE

#if UNITY_EDITOR
        /// <summary>
        /// Store a JSON representation of this object that can be re-loaded when exiting play mode
        /// </summary>
        [SerializeField, HideInInspector]
        private string _volatileState = string.Empty;
#endif

        /*----------Functions----------*/
        //PRIVATE

#if UNITY_EDITOR
        /// <summary>
        /// Handle the processing of JSON data to control the information stored in this object
        /// </summary>
        /// <param name="state">The current playmode state that the editor is entering</param>
        private void PlayModeStateChangedCallback(PlayModeStateChange state) {
            switch (state) {
                // Save the current state of the data for recall
                case PlayModeStateChange.EnteredPlayMode:
                    _volatileState = JsonUtility.ToJson(this, false);
                    break;

                // Load the saved data back onto the object
                case PlayModeStateChange.ExitingPlayMode:
                    if (!string.IsNullOrWhiteSpace(_volatileState)) {
                        JsonUtility.FromJsonOverwrite(_volatileState, this);
                        _volatileState = null;
                    } else Debug.LogError($"Unable to load volatile memory of '{this}', no state data was stored", this);

                    break;
            }
        }
#endif

        //PROTECTED

        /// <summary>
        /// Subscribe to the state changed callback that is used to manage contained data
        /// </summary>
        protected virtual void OnEnable() {
#if UNITY_EDITOR
            // Subscribe to the state change callback
            EditorApplication.playModeStateChanged += PlayModeStateChangedCallback;

            // If there is persisting volatile memory, consume it
            if (!Application.isPlaying && !string.IsNullOrWhiteSpace(_volatileState)) {
                JsonUtility.FromJsonOverwrite(_volatileState, this);
                _volatileState = null;
            }
#endif
        }

        /// <summary>
        /// Unsubscribe from the state changed callback that is used to manage contained data
        /// </summary>
        protected virtual void OnDisable() {
#if UNITY_EDITOR
            // Remove the state change callback
            EditorApplication.playModeStateChanged -= PlayModeStateChangedCallback;
#endif
        }
    }
}