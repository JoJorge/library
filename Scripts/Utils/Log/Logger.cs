namespace Utils
{
    /// <summary>
    /// wrap debug log, for removing UnityEngine relations while testing outside of Unity.
    /// </summary>
    public static class Logger
    {
        private static System.Action<string> _logAction = null;

        private static System.Action<string> _logWarningAction = null;

        private static System.Action<string> _logErrorAction = null;

        /// <summary>
        /// log message, will use Debug.Log in Unity
        /// </summary>
        /// <param name="message"> message to log </param>
        public static void Log(string message)
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.Log(message);
#endif
            _logAction?.Invoke(message);
        }

        /// <summary>
        /// log warning message, will use Debug.LogWarning in Unity
        /// </summary>
        /// <param name="message"> warning message to log </param>
        public static void LogWarning(string message)
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogWarning(message);
#endif
            _logWarningAction?.Invoke(message);
        }

        /// <summary>
        /// log error message, will use Debug.LogError in Unity
        /// </summary>
        /// <param name="message"> error message to log </param>
        public static void LogError(string message)
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogError(message);
#endif
            _logErrorAction?.Invoke(message);
        }

        /// <summary>
        /// set log action, for testing outside of Unity
        /// </summary>
        /// <param name="logAction"> log action to set </param>
        public static void SetLogAction(System.Action<string> logAction)
        {
            _logAction = logAction;
        }

        /// <summary>
        /// set log warning action, for testing outside of Unity
        /// </summary>
        /// <param name="logWarningAction"> log warning action to set </param>
        public static void SetLogWarningAction(System.Action<string> logWarningAction)
        {
            _logWarningAction = logWarningAction;
        }

        /// <summary>
        /// set log error action, for testing outside of Unity
        /// </summary>
        /// <param name="logErrorAction"> log error action to set </param>
        public static void SetLogErrorAction(System.Action<string> logErrorAction)
        {
            _logErrorAction = logErrorAction;
        }
    }
}
