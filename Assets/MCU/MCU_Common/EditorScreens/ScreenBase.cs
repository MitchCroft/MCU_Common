#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace MCU.EditorScreens {
    /// <summary>
    /// A base point for screen elements that are to be displayed within the editor
    /// </summary>
    public abstract class ScreenBase {
        /*----------Types----------*/
        //PUBLIC

        /// <summary>
        /// Raise a request to interested parties that the screen change states to the specified
        /// </summary>
        /// <param name="state">The state that is to be changed to</param>
        public delegate void ScreenChangeDel(Enum state);

        /*----------Events----------*/
        //PUBLIC

        /// <summary>
        /// Event that will be raised when a screen state change is being requested
        /// </summary>
        public event ScreenChangeDel OnStateChange;

        /*----------Functions----------*/
        //PROTECTED

        /// <summary>
        /// Request that all listening parties change state to the specified
        /// </summary>
        /// <param name="state">The state that is to be changed to</param>
        protected void ChangeState(Enum state) { OnStateChange?.Invoke(state); }

        //PUBLIC

        /// <summary>
        /// Raised when this screen is opened for display by a <see cref="ScreenStateMachine{TEnum}"/> object
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        public virtual void OnOpened(Dictionary<string, object> data) {}

        /// <summary>
        /// Handle the displaying of UI elements via the Layout display commands (EditorGUILayout, GUILayout)
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        public virtual void DisplayLayout(Dictionary<string, object> data) {}

        /// <summary>
        /// Raised when this screen is being closed
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        public virtual void OnClose(Dictionary<string, object> data) {}

        /// <summary>
        /// Raised when this screen object is being destroyed
        /// </summary>
        /// <param name="data">The current collection of information that has been set for the state machine</param>
        public virtual void OnDestroy(Dictionary<string, object> data) {}
    }
}
#endif