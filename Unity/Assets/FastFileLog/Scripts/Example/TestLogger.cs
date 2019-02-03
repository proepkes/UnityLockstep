using UnityEngine;
using System.Collections;

namespace FastFileLog {
    public class TestLogger : MonoBehaviour {
        void Start() {
            // register first before log
            // format: logger key, log filename, enable screen log, enable file log
            // note #1: you can simply use gameObject as the logger key; 
            // note #2: logger key must be unique for every instance and script
            LogManager.Register("testLogger", "aTestFile.txt", true, true);

            // output log
            // format: logger key, log msg        
            LogManager.Log("testLogger", "a test msg");
        }
    }
}