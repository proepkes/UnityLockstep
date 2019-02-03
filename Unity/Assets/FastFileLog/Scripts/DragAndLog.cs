using UnityEngine;
using System.Collections.Generic;

namespace FastFileLog {
    public class DragAndLog : MonoBehaviour {
        [System.Serializable]
        public class GameObjectLogConfig {
            public GameObject gameObject;
            public bool enabled = true;
            public bool printToScreen;
            public bool printToFile;
        }

        //[Tooltip("要配置或添加某个object的Log，将其拖动到此列表中")]
        public GameObjectLogConfig[] loggerConfigs;

        /// <summary>
        /// Configure loggers according to the settings in the list. Unlisted loggers won't be affected.
        /// </summary>
        private void DoLoggerConfig() {
            if (!LogManager.debug)
                return;

            for (int i = 0; i < loggerConfigs.Length; i++) {
                GameObjectLogConfig config = loggerConfigs[i];
                if (config.gameObject == null || !config.enabled)
                    continue;

                LogManager.Register(config.gameObject, config.gameObject.name, config.printToScreen, config.printToFile);
            }
        }

        void Start() {
            DoLoggerConfig();
        }
    }
}