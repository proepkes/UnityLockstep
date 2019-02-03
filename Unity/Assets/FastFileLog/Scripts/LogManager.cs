using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FastFileLog {
    // Fast File Log

    // Usage: 

    // #1 Create an empty gameObject and add LogManager component, which will add a DragAndLog component automatically.

    // #2 Drag the GameObject of your script(which needs to output log) to DragAndLog component of LogManager, to enable file log for that GameObject
    // note: does not support multithreading. For multithreading log, use LogManager.Register (See also TestLogger.cs).        
   
    // #3 In your script, call LogManager.Log method to output log msg, which will be saved to file when game exit
    // Format: LogManager.Log(object gameObject, object logMsg);
    // gameObject is the gameObject of the caller script

    // #4 To disable a single log, disable it in DragAndLog component
    // To disable all logs, simply delete the LogManager GameObject from scene or set it to inactive;

    // #5 To customize log format, extend the Logger class, and override LogFormat function.

    [RequireComponent(typeof(DragAndLog))]
    public class LogManager : MonoBehaviour {

        [Tooltip("The save path of log files, relative to the Asset path or application root path")]
        public string savePath;

        [Header("Readonly")]
        [Tooltip("The list of all enabled loggers")]
        public List<Logger> loggerList = new List<Logger>();

        // whether debug is on
        public static bool debug {
            get { return Instance != null && Instance.enabled; }
        }


        #region public Log Method
        /// <summary>
        /// register to log system, with a specified log file name
        /// </summary>

        // note #1: you can simply use gameObject as the logger key; 
        // note #2: logger key must be unique for every instance and every script

        public static void Register(object loggerKey, string logFilename, bool printToScreen, bool printToFile) {
            Register(loggerKey, logFilename, printToScreen, printToFile, CreateBaseLogger);
        }


        /// <summary>
        /// log some message. Searches for the configure by loggerKey reference.
        /// </summary>
        /// <param name="loggerKey"></param>
        /// <param name="logMsg"></param>
        public static void Log(object loggerKey, object logMsg) {
            if (!debug || loggerKey == null) {
                return;
            }

            Logger logger;
            if (TryGetLogger(loggerKey, out logger)) {
                logger.Log(logMsg);
            }
        }

        #endregion

        


        #region Helpers

        private delegate Logger LoggerFactory(string name, bool enabled);
        private static LoggerFactory CreateBaseLogger = (name, enabled) => { return new Logger(name, enabled); };
        
        private Dictionary<object, Logger> loggerDict = new Dictionary<object, Logger>();
        private static Logger NullLogger { get { return Logger.NullLogger; } }



        private static void Register(object loggerKey, string fileName, bool printToScreen, bool printToFile, LoggerFactory createLogger) {
            if (!debug ||
                Instance.loggerDict.ContainsKey(loggerKey)) {
                return;
            }

            Logger logger = createLogger(fileName, true);
            Instance.loggerDict.Add(loggerKey, logger);
            Instance.loggerList.Add(logger);

            logger.Configure(printToScreen, printToFile, loggerKey);
        }

        public static bool TryGetLogger(object loggerKey, out Logger logger) {
            if (loggerKey == null) {
                logger = null;
                return false;
            } else
                return Instance.loggerDict.TryGetValue(loggerKey, out logger); 
        }
        
        public static void SaveAllLog() {
            if (!debug)
                return;

            foreach(Logger logger in Instance.loggerList) {       
                logger.Save();
            }
        }

        
        public static int GetLineNumber() {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
            return st.GetFrame(0).GetFileLineNumber();
        }

        #endregion

        


        #region Unity Life Cycle
        public static LogManager Instance;
        void Awake() {
            if (Instance == null)
                Instance = this;
        }

        
        void OnDestroy() {
            SaveAllLog();
        }
        #endregion

    }
}