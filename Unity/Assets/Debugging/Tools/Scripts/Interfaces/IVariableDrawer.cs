using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DebugTools {
	public interface IVariableDrawer {

		string GetDrawerName { get; }

		void DrawVariable(string key, object message);

		void DrawVariable(string key, string format, params object[] args);

		void RegisterDrawer();
	}
}
