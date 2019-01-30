using System.IO;          
using System.Xml.Serialization;   
using UnityEngine;     

namespace DebugTools {
                     
	public class DebugSettings  {

		public static readonly char[] allReserveSymbols = {'@', '^'};
		public static readonly char[] allCommandSymbols = {'*', '#'};

		//General Settings
		public string profileName = "Default";
		public uint generalMargin = 10;
		public bool stripFromRelease = false;

		//Console Output Settings
		public uint consoleMaxLength = 20;
		public Color consoleColour = Color.green;

		//Console Input Settings
		public char commandChar = '*';
		public char reservedChar = '@';

		//Tracker Settings
		public Color trackerColour = Color.green;

		//Grapher Settings
		public Color graphMinColour = Color.red;
		public Color graphMaxColour = Color.green;
		public uint graphMargin = 10;
		public uint graphLineSize = 2;
		public uint graphUpdateFrequency = 25;

		//Logger Settings
		public bool useUniqueLogNames = false;
		public bool postLogFiles = false;

		//Key Bind Settings
		public KeyCode debugMenuKey = KeyCode.Minus; 
		public KeyCode trackerKey = KeyCode.Period;
		public KeyCode grapherKey = KeyCode.Slash; 

		public static DebugSettings FromXML(string _xml) {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DebugSettings));
			return (DebugSettings)xmlSerializer.Deserialize(new StringReader(_xml));
		}
			
		public string ToXML() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DebugSettings));

			using (StringWriter textWriter = new StringWriter()) {
				xmlSerializer.Serialize(textWriter, this);
				return textWriter.ToString();
			}
		}        
	}
}
