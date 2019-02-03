using UnityEngine;
using System.Collections;

namespace FastFileLog {
    // drag the gameobject to dragAndLog list to enable logging.
    public class TestDragAndLog : MonoBehaviour {
        float time = 0;
        // Update is called once per frame
        void Update() {
            time += Time.deltaTime;
            if (time > 1) {
                time = 0;
                LogManager.Log(gameObject, "This is a drag and log example. Log every 1 second. Log will be saved in Assets/FastFileLog/Log folder");
            }
        }
    }
}