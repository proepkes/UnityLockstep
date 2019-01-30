using UnityEngine;
using UnityEditor;

using DebugTools;

public class DebugToolsSettingsEditor : EditorWindow {

	DebugSettings settings;
	string settingsMenu = "";
	GUIStyle buttonStyle, leftMenu;
	
	[MenuItem("Tools/DebugTools Settings")]
	public static void OpenSettings() {
		DebugToolsSettingsEditor window = EditorWindow.GetWindow(typeof(DebugToolsSettingsEditor), true, "DebugTools Settings") as DebugToolsSettingsEditor;
		window.Show();
	}

	private void OnEnable() {
		string xml;

		if (Setup.LoadSettingsFile (out xml) && xml != "") {
			settings = DebugSettings.FromXML (xml);
		} else {
			settings = new DebugSettings ();     
		}

		buttonStyle = new GUIStyle("Button");
		//buttonStyle.fixedWidth = 200;

        leftMenu = new GUIStyle {fixedWidth = 200};
    }

	private void OnGUI() {

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(leftMenu); 				//Left side menu
		
		if (GUILayout.Button("General", buttonStyle)) { settingsMenu = "general"; }
		if (GUILayout.Button("Console Output", buttonStyle)) { settingsMenu = "consoleoutput"; }
		if (GUILayout.Button("Console Input", buttonStyle)) { settingsMenu = "consoleinput"; }
		if (GUILayout.Button("Tracker", buttonStyle)) { settingsMenu = "tracker"; }
		if (GUILayout.Button("Graphing", buttonStyle)) { settingsMenu = "grapher"; }
		if (GUILayout.Button("Logging", buttonStyle)) { settingsMenu = "logging"; }
		if (GUILayout.Button("Key Bindings", buttonStyle)) { settingsMenu = "keybindings"; }

		GUILayout.FlexibleSpace();       
                                                                       
		if (GUILayout.Button("Information", buttonStyle)) { settingsMenu = "information"; }
		if (GUILayout.Button("Documentation", buttonStyle)) { ShowDocumentation(); }
		GUILayout.Label("");

		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical(); 				//Right Side menu


		switch(settingsMenu) {
			case "general":
			default:
				GUILayout.Label ("General Settings", EditorStyles.boldLabel);
				break;
		}
		
		EditorGUILayout.BeginScrollView(Vector2.zero, new GUIStyle("HelpBox"));

		switch(settingsMenu) {
			case "consoleoutput":
				ShowConsoleOutputSettings();	
				break;
			case "consoleinput":
				ShowConsoleInputSettings();	
				break;
			case "tracker":
				ShowTrackerSettings();	
				break;
			case "grapher":
				ShowGrapherSettings();	
				break;
			case "logging":
				ShowLoggerSettings();	
				break;
			case "keybindings":
				ShowKeyBindingSettings();	
				break;
			case "information":
				ShowInformation();	
				break;
			case "general":
			default:
				ShowGeneralSettings();
				break;
		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndScrollView();

		GUILayout.Label("");
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();   
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		GUILayout.Label("");

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}

	//====================================
	// Show General Settings
	//====================================
	void ShowGeneralSettings() {
		settings.profileName = EditorGUILayout.TextField("Profile name", settings.profileName);
		settings.generalMargin = (uint)EditorGUILayout.IntField("Graph Margin", (int)settings.generalMargin);
		settings.stripFromRelease = EditorGUILayout.Toggle("Strip from release", settings.stripFromRelease);
	}

	//====================================
	// Show Console Output Settings
	//====================================
	void ShowConsoleOutputSettings() {
		settings.consoleColour = EditorGUILayout.ColorField("Console colour", settings.consoleColour);
		settings.consoleMaxLength = (uint)EditorGUILayout.IntField("Console max length", (int)settings.consoleMaxLength);
	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowConsoleInputSettings() {
		EditorGUILayout.HelpBox("There are currently no setting options avaliable for the Console Input", MessageType.Warning);
	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowTrackerSettings() {
		settings.trackerColour = EditorGUILayout.ColorField("Tracker colour", settings.trackerColour);
	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowGrapherSettings() {
		settings.graphMaxColour = EditorGUILayout.ColorField("Default min colour", settings.graphMaxColour);
		settings.graphMinColour = EditorGUILayout.ColorField("Default max colour", settings.graphMinColour);
		settings.graphLineSize = (uint)EditorGUILayout.IntField("Graph line size", (int)settings.graphLineSize);
		settings.graphUpdateFrequency = (uint)EditorGUILayout.IntField("Graph update frequency:", (int)settings.graphUpdateFrequency);
		settings.graphMargin = (uint)EditorGUILayout.IntField("Graph Margin", (int)settings.graphMargin);
	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowLoggerSettings() {
		settings.useUniqueLogNames = EditorGUILayout.Toggle("Use unique log files", settings.useUniqueLogNames);
//		settings.postLogFiles = EditorGUILayout.Toggle("POST log files", settings.postLogFiles);

	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowKeyBindingSettings() {
		settings.debugMenuKey = (KeyCode)EditorGUILayout.EnumPopup("Debug menu key", settings.debugMenuKey);        
		settings.trackerKey = (KeyCode)EditorGUILayout.EnumPopup("Tracker key", settings.trackerKey);
		settings.grapherKey = (KeyCode)EditorGUILayout.EnumPopup("Grapher key", settings.grapherKey);
	}

	//====================================
	// Show Console Input Settings
	//====================================
	void ShowInformation() {                                                            
		GUILayout.Label("Debug Tools");
		GUILayout.Label("Version 0.3.0 (alpha)");
		GUILayout.Label("");
		GUILayout.Label("Developed by Aaron Walwyn.");
		GUILayout.Label("Copyright 2017. All rights reserved.");
		GUILayout.Label("");
		GUILayout.Label("Asset Store Page: ");
		GUILayout.Label("Website: ");
		GUILayout.Label("Forum Page: ");
		GUILayout.Label("Support Contact: ");
		GUILayout.Label("");
		GUILayout.Label("This version of DebugTools is free in return for feedback.");
		GUILayout.Label("Please consider leaving a review and/or comment on the store page.");
	}

	//====================================
	// Show Documentation
	//====================================
	void ShowDocumentation() {

		string[] paths = AssetDatabase.FindAssets ("readme l:DebugTools");

		if (paths.Length > 0) {
			Application.OpenURL ("file://" + System.Environment.CurrentDirectory + "/" + AssetDatabase.GUIDToAssetPath(paths[0]));
		} else {	
			Application.OpenURL ("http://aaronwalwyn.com/DebugTools/readme.html");
		}
	}

	void UpdateSettingsPanel(string str) {
		this.settingsMenu = str;
	}

	private void ResetSettingsFile() {
		settings = new DebugSettings();
	}   
}
