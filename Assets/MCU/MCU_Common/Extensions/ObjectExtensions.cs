using UnityEngine;

namespace MCU.Extensions {
    /// <summary>
    /// Provide additional functionality to <see cref="UnityEngine.Object"/> objects
    /// </summary>
    public static class ObjectExtensions {
        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Retrieves a formatted debug log message, prefixed with identifying information about the calling object
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="format">A string defining the format that the message will be created with</param>
        /// <param name="values">The values that will be substituated into the format string. See <see cref="string.Format(string, object[])"/> for specifics</param>
        /// <returns>Returns a combined string message with the identifying information prefixing the supplied data</returns>
        public static string GetFormattedLog(this UnityEngine.Object obj, string format, params object[] values) {
#if UNITY_EDITOR
            return string.Format("<b>'{0}' ({1})</b>: {2}", obj.name, obj.GetType(), string.Format(format, values));
#else
            return string.Format("'{0}' ({1}): {2}", obj.name, obj.GetType(), string.Format(format, values));
#endif
        }

        /// <summary>
        /// Retrieves a formatted debug log message, prefixed with identifying information about the calling object
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="msg">The message that is to be appended to the debug information</param>
        /// <returns>Returns a combined string message with the identifying information prefixing the supplied data</returns>
        public static string GetFormattedLog(this UnityEngine.Object obj, string msg) {
#if UNITY_EDITOR
            return string.Format("<b>'{0}' ({1})</b>: {2}", obj.name, obj.GetType(), msg);
#else
            return string.Format("'{0}' ({1}): {2}", obj.name, obj.GetType(), msg);
#endif
        }

        /// <summary>
        /// Log an object relevent formatted message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="format">A string defining the format that the message will be created with</param>
        /// <param name="values">The values that will be substituated into the format string. See <see cref="string.Format(string, object[])"/> for specifics</param>
        public static void Log(this UnityEngine.Object obj, string format, params object[] values) {
            Debug.Log(obj.GetFormattedLog(format, values), obj);
        }

        /// <summary>
        /// Log an object relevent formatted message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="msg">The message that is to be appended to the debug information</param>
        public static void Log(this UnityEngine.Object obj, string msg) {
            Debug.Log(obj.GetFormattedLog(msg), obj);
        }

        /// <summary>
        /// Log an object relevent formatted warning message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="format">A string defining the format that the message will be created with</param>
        /// <param name="values">The values that will be substituated into the format string. See <see cref="string.Format(string, object[])"/> for specifics</param>
        public static void LogWarning(this UnityEngine.Object obj, string format, params object[] values) {
            Debug.LogWarning(obj.GetFormattedLog(format, values), obj);
        }

        /// <summary>
        /// Log an object relevent formatted warning message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="msg">The message that is to be appended to the debug information</param>
        public static void LogWarning(this UnityEngine.Object obj, string msg) {
            Debug.LogWarning(obj.GetFormattedLog(msg), obj);
        }

        /// <summary>
        /// Log an object relevent formatted error message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="format">A string defining the format that the message will be created with</param>
        /// <param name="values">The values that will be substituated into the format string. See <see cref="string.Format(string, object[])"/> for specifics</param>
        public static void LogError(this UnityEngine.Object obj, string format, params object[] values) {
            Debug.LogError(obj.GetFormattedLog(format, values), obj);
        }

        /// <summary>
        /// Log an object relevent formatted error message to the unity console
        /// </summary>
        /// <param name="obj">The object that will be used to generate the message</param>
        /// <param name="msg">The message that is to be appended to the debug information</param>
        public static void LogError(this UnityEngine.Object obj, string msg) {
            Debug.LogError(obj.GetFormattedLog(msg), obj);
        }
    }
}