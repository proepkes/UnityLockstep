using System;
using System.IO;                    
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;                           

namespace DebugTools {
	public class Setup : MonoBehaviour
    {             

		public static Setup instance;
		public static DebugSettings settings;
		public Canvas debugCanvas;              

		public CanvasGroup debugTrackerGroup;
		public CanvasGroup debugGrapherGroup;

		public bool initialiseOnStart = false;          
                         
		private void Awake() {
			if (initialiseOnStart) {
				Initialise ();
			}
		}
                             
		public void Initialise() {

			if (instance == null) {
				instance = this;
				DontDestroyOnLoad (this.gameObject);

				LoadConfig ();

				this.gameObject.AddComponent<DrawManager>().Initialise();

				CreateCanvas ();       
				CreateTracker ();
				CreateGrapher ();

			} else {
				Destroy (this.gameObject);
			}
		}           
                         
		private void LoadConfig() {
			if (settings == null) {

				string xml = "";

				if (LoadSettingsFile (out xml) && xml != "") {
					settings = DebugSettings.FromXML (xml);
				} else {
					settings = new DebugSettings ();
				}
			}

			Assert.IsNotNull (settings);
		}
                        
		private void CreateCanvas() {
			this.transform.position = Vector3.zero;

			debugCanvas = new GameObject("Canvas").AddComponent<Canvas>();
			debugCanvas.transform.SetParent (this.transform);
			debugCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
			debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			debugCanvas.sortingOrder = 32767;

			GraphicRaycaster gr = debugCanvas.gameObject.AddComponent<GraphicRaycaster>();

			if (UnityEngine.EventSystems.EventSystem.current == null) {
				GameObject g = new GameObject("Event");
				g.transform.SetParent(this.transform);
				UnityEngine.EventSystems.EventSystem.current = g.AddComponent<UnityEngine.EventSystems.EventSystem>();
				g.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
			}
		}
                       
		private void CreateTracker() {
			GameObject g = new GameObject("Tracker");
			g.AddComponent<Tracker>().Initialise();
		}                      
                            
		private void CreateGrapher() {
			GameObject g = new GameObject("Grapher");
			g.AddComponent<Grapher>().Initialise();
		}          

		private void Update() {       
			if (Input.GetKeyUp (settings.trackerKey) && debugTrackerGroup != null) {
				SwitchGroup (debugTrackerGroup);
			}

			if (Input.GetKeyUp (settings.grapherKey) && debugGrapherGroup != null) {
				SwitchGroup (debugGrapherGroup);
			}
		}                                               

		public static bool LoadSettingsFile (out string xml) {
			xml = "";

			try {

				xml = File.ReadAllText(Application.streamingAssetsPath + "/DebugTools/settings.xml");

			} catch (Exception) {         
				return false;
			}

			return (xml != "");
		}

		public static void SwitchGroup(CanvasGroup _cg) {
			if (_cg.alpha > 0f) {
				HideGroup(_cg);
			} else {
				ShowGroup(_cg);
			}
		}

		public static void ShowGroup(CanvasGroup _cg) {
			_cg.interactable = true;
			_cg.alpha = 1f;
			_cg.blocksRaycasts = true;
		}

		public static void HideGroup(CanvasGroup _cg) {
			_cg.interactable = false;
			_cg.alpha = 0f;
			_cg.blocksRaycasts = false;
		}                                           
	}
}
