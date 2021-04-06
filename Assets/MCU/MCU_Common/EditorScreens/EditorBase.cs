#if UNITY_EDITOR
using UnityEditor;

namespace MCU.EditorScreens {
    /// <summary>
    /// Provide a base point for editors to ingerit from and manage displaying of multiple 'screens' of information
    /// </summary>
    public abstract class EditorBase : Editor {
        /*----------Variables----------*/
        //PROTECTED

        /// <summary>
        /// Store a reference to the state machine that will be used to display the different elements
        /// </summary>
        protected ScreenStateMachine screenStateMachine;

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Setup references that are required when this this object is enabled
        /// </summary>
        protected virtual void OnEnable() { screenStateMachine = CreateWindowStateMachine(); }

        /// <summary>
        /// Save editor preferences for resuming operation
        /// </summary>
        protected virtual void OnDestroy() {
            if (screenStateMachine != null)
                screenStateMachine.Dispose();
        }

        /// <summary>
        /// Get the state machine object that will be displayed for this window
        /// </summary>
        /// <returns>Returns the state machine instance that is to be displayed</returns>
        protected abstract ScreenStateMachine CreateWindowStateMachine();

        //PUBLIC

        /// <summary>
        /// Render the window UI controls to the display area
        /// </summary>
        public override void OnInspectorGUI() {
            if (screenStateMachine != null) {
                screenStateMachine.DoLayoutNavigation();
                screenStateMachine.DoLayoutActiveScreen();
            }
        }    
    }
}
#endif