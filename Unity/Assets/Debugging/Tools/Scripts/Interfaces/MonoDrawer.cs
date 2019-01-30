using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DebugTools {
    public abstract class MonoDrawer : MonoBehaviour, IVariableDrawer
    {
        [SerializeField] protected string drawerName;

        public string GetDrawerName {
            get {return drawerName; }
        }

        public virtual void RegisterDrawer() {
            DrawManager.RegsiterDrawer(GetDrawerName, (IVariableDrawer)this);
        }

        public abstract void DrawVariable(string key, object message);

        public abstract void DrawVariable(string key, string format, params object[] args);
    }
}