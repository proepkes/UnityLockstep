using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugTools {
	public class Tracker : MonoBehaviour {

		public static Tracker instance;
		private Text trackerOutput;
		private Dictionary<string, object> trackedVariables;

		public Tracker Initialise() {


			RectTransform rt = gameObject.AddComponent<RectTransform>();
			rt.SetParent(Setup.instance.debugCanvas.transform);
			rt.anchorMin = Vector2.up;
			rt.anchorMax = Vector2.up;
			rt.pivot = new Vector2(0.5f, 1);

			rt.anchoredPosition = new Vector2(Screen.width / 2f, -10f);
			rt.sizeDelta = new Vector2(Screen.width - (2 * Setup.settings.generalMargin), Screen.height / 2);

			trackerOutput = gameObject.AddComponent<Text>();
			trackerOutput.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
			trackerOutput.color = Setup.settings.trackerColour;
			trackerOutput.alignment = TextAnchor.UpperLeft;

			trackerOutput.verticalOverflow = VerticalWrapMode.Overflow;
			trackerOutput.horizontalOverflow = HorizontalWrapMode.Overflow;

			trackerOutput.text = "Test String Please Ignore";

			trackedVariables = new Dictionary<string, object>();

			Setup.instance.debugTrackerGroup = gameObject.AddComponent<CanvasGroup> ();
			Setup.HideGroup (Setup.instance.debugTrackerGroup);

			
			return (instance = this);
		}

		private void Update() {
			UpdateTracker();
		}

		private void UpdateTracker() {
			string s = "";

			try {
				foreach (string k in trackedVariables.Keys) {
					s += k + ": " + trackedVariables[k].ToString() + "\n";
				}
			} catch (InvalidOperationException e) {
				Debug.Log(e.Message);
				return;
			}

			if (trackerOutput != null) {
				trackerOutput.text = s;
			}
		}

		public static void Track(string key, object value) {
			lock (instance.trackedVariables) {
				if (instance.trackedVariables.ContainsKey(key)) {
					instance.trackedVariables[key] = value;
				} else {
					instance.trackedVariables.Add(key, value);        
				}
			}
		}
	}
}
