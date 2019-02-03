using UnityEngine;               
using System.Collections.Generic;
using System.IO;

namespace FastFileLog {           

    [System.Serializable]
    public class Logger{
        public string name;
        public object key;

        public bool enabled;
        public bool printToScreen = false;
        public bool printToFile = false;
        
        private List<string> data;
        private FileStream fileWriter;
        private bool debug {
            get { return enabled && LogManager.debug; }
        }

        public void Configure(bool printToScreen, bool printToFile, object gameObject) {
            this.printToScreen = printToScreen;
            this.printToFile = printToFile;
            this.key = gameObject;
        }

        public void Log(object logMsg) {
            if (debug) {
                data.Add(logMsg.ToString());                         
                
                if (printToScreen)
                {
                    string msgString = logMsg.ToString();
                    Flush(msgString);
                }
            }
        }

        
        public void Flush(string msg) {
            if (printToScreen && debug) {
                Debug.Log(msg);
            }
        }

        public void Save() {
            if (debug && printToFile) {
                string filePath = GetFileFullPath(name);  
                File.WriteAllLines(filePath, data);  
            }            
        }


       

        #region virtual functions         

        protected virtual string GetFileFullPath(string name) {
            return LogManager.Instance.savePath + "/" + name + ".txt";
        }
        #endregion

        #region Null Instance
        private static Logger _nullLogger;
        public static Logger NullLogger {
            get {
                if (_nullLogger == null) {
                    _nullLogger = new Logger("NullLogger", false) {enabled = false};
                }
                return _nullLogger;
            }
        }
        public static bool IsNull(Logger l) {
            return l == NullLogger;
        }
        #endregion


        public Logger(string name, bool enabled) {
            this.name = name;
            this.enabled = enabled;

            data = new List<string>();                                                            
        }

        #region Helper
        private System.Text.UnicodeEncoding uniEncoding = new System.Text.UnicodeEncoding();
        private void WriteToStream(Stream stream, string msg) {
            if (stream != null) {
                stream.Write(uniEncoding.GetBytes(msg),
                0, uniEncoding.GetByteCount(msg));
            }
        }
        #endregion
    }
}