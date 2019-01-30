using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugTools {

	public enum GraphCanvasType {
		LINE_PLOT,
		SCATTER_PLOT,
		//BAR_PLOT
	}

	public class Grapher : MonoBehaviour {

		public static Grapher instance;
		private List<GraphCanvas> allGraphs;

		public Grapher Initialise() {

			RectTransform rt = gameObject.AddComponent<RectTransform>();
			rt.SetParent(Setup.instance.debugCanvas.transform);
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.zero;
			rt.pivot = new Vector2(0.5f, 0.5f);

			rt.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
			rt.sizeDelta = new Vector2(Screen.width, Screen.height);

			Setup.instance.debugGrapherGroup = gameObject.AddComponent<CanvasGroup> ();
			Setup.HideGroup (Setup.instance.debugGrapherGroup);


			allGraphs = new List<GraphCanvas>();

			return (instance = this);
		}

		// Use this for initialization
		void Start () {
			InvokeRepeating("UpdateAllGraphs", 0, 1f/Setup.settings.graphUpdateFrequency);
		}

		private void UpdateAllGraphs() {
			for(int i=0; i<allGraphs.Count; i++) {
				allGraphs[i].Update();
			}
		}

		public static GraphCanvas CreateGraph(
			string key, GraphCanvasType gcType,
			bool realtime, bool multiPlot,
			float val1, float val2
			) {
			GraphCanvas gc = new GraphCanvas();

			gc.realtime = realtime;
			gc.multiPlot = multiPlot;
			gc.name = key;
			gc.gcType = gcType;

			gc.minValue = val1;
			gc.maxValue = val2;

			gc.gameobject = new GameObject("Graph: **Name**");
			RectTransform rt = gc.gameobject.AddComponent<RectTransform>();
			rt.SetParent(Setup.instance.debugGrapherGroup.gameObject.transform);
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.zero;
			rt.pivot = Vector2.zero;

			rt.anchoredPosition = new Vector2(Setup.settings.graphMargin, Setup.settings.graphMargin);
			rt.sizeDelta = new Vector2(Screen.width - (2 * Setup.settings.graphMargin) - 110, Screen.height - (2 * Setup.settings.graphMargin));

			gc.image = gc.gameobject.AddComponent<RawImage>();
			gc.texture = new Texture2D(Mathf.RoundToInt(rt.rect.width / Setup.settings.graphLineSize), Mathf.RoundToInt(rt.rect.height / Setup.settings.graphLineSize));
			gc.texture.filterMode = FilterMode.Point;
			gc.texture.wrapMode = TextureWrapMode.Repeat;

			gc.image.texture = gc.texture;
			gc.ClearTexture();

			//InvokeRepeating ("UpdateGraphTexture", 0f, 1f / (float)settings.graphUpdateFrequency);

			gc.canvas = gc.gameobject.AddComponent<CanvasGroup> ();

			GameObject g = new GameObject("GrapherButton");
			rt = g.AddComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.zero;
			rt.pivot = Vector2.one;

			rt.anchoredPosition = new Vector2(Screen.width, Screen.height) 
				- new Vector2(Setup.settings.generalMargin, Setup.settings.generalMargin) 
				- new Vector2(0, instance.allGraphs.Count * 60);

			rt.sizeDelta = new Vector2(100,50);
			rt.SetParent(gc.gameobject.transform.parent);

			Button b = g.AddComponent<Button>();
			b.targetGraphic = g.AddComponent<Image>();
			g = new GameObject("Label");
			g.transform.SetParent(b.transform);
			Text t = g.AddComponent<Text>();
			t.text = key;
			t.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
			t.color = Color.black;
			t.alignment = TextAnchor.MiddleCenter;
			g.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			//b.transform.Find("Text").GetComponent<Text>().text = "Show/Hide";
			
			b.onClick.AddListener(() => {
				instance.HideAllGraphs();
				gc.Activate();
				//Debug.Log("test");
			});

			instance.allGraphs.Add(gc);

			instance.HideAllGraphs();
			gc.Activate();

			return gc;
		}

		//==========================================
		// HELPER FUNCTIONS
		//==========================================
		public void HideAllGraphs() {
			for(int i = 0; i < allGraphs.Count; i++) {
				Setup.HideGroup(allGraphs[i].canvas);
			}
		}
	}

	//==========================================
	// CLASS : GraphCanvas
	//==========================================
	[System.Serializable]
	public class GraphCanvas : System.Object {

		//private Dictionary<string, Graph> graphDictionary = new Dictionary<string, Graph>();
		private List<Vector2> values = new List<Vector2>();

		public string name;
		public GraphCanvasType gcType;

		public float maxValue;
		public float minValue;
		public Color maxColor;
		public Color minColour;

		//public float latestValue;
		public float previous;

		public GameObject gameobject;
		public RawImage image;
		public Texture2D texture;

		public bool multiPlot = true;
		public bool realtime = true;

		private bool active = true;
		public bool setDirty = false;

		public CanvasGroup canvas;

		int graphIndex = 0;

		public void Activate() {
			Setup.ShowGroup(this.canvas);
		}

		public void Update() {
			if (realtime || setDirty) {
				UpdateTexture();
				UpdateGraphPosition();

				if (active) {
					ApplyTexture();
				}
			}
		}

		private void UpdateTexture() {
			if (texture != null) {
				if (gcType == GraphCanvasType.LINE_PLOT) {
					if (realtime) {
						for (int y = 0; y < texture.height; y++) {
							texture.SetPixel(graphIndex % texture.width, y, Color.clear);
						}

						int margin = 10;

                        if (values.Count < 1) return;

						float latestValue = Mathf.Clamp(values[values.Count-1].y, minValue, maxValue);

						float per = (latestValue - minValue) / (maxValue - minValue);
						int p =  Mathf.FloorToInt((texture.height - (2 * margin)) * per);
						texture.SetPixel(graphIndex % texture.width, margin + p, Color.Lerp(Color.red, Color.green, per));

						if (Mathf.Abs(previous - latestValue) > 0) {
							int dir = (latestValue > previous) ? +1 : -1;

							int j = Mathf.FloorToInt((texture.height - (2 * margin)) * ((previous - minValue) / (maxValue - minValue)));
							int k = Mathf.FloorToInt((texture.height - (2 * margin)) * ((latestValue - minValue) / (maxValue - minValue)));

							for (int n = j; n != (k + dir); n += dir) {
								texture.SetPixel(graphIndex % texture.width, n + margin, Color.Lerp(Color.red, Color.green, (float)n / (texture.height - (2 * margin))));
							}

							previous = latestValue;
						}

						texture.SetPixel(graphIndex % texture.width, margin-1, Color.black);
						if (graphIndex % 10 == 0) {
							texture.SetPixel(graphIndex % texture.width, margin-2, Color.black);
							texture.SetPixel(graphIndex % texture.width, margin-3, Color.black);
						}

						graphIndex++;
						
						//Multiplot code
						/*Graph[] graphs = new Graph[graphDictionary.Count];
						graphDictionary.Values.CopyTo (graphs, 0);

						for (int i = 0; i < graphs.Length; i++) {

							graphs[i].top = ((texture.height - graphs.Length) / graphs.Length) * (i+1) + i - 1;
							graphs[i].bottom = ((texture.height - graphs.Length) / graphs.Length) * (i) + i;
							texture.SetPixel(graphIndex % texture.width, graphs[i].top+1, Color.black);
							//graphTexture.SetPixel(graphIndex % graphTexture.width, graphs[i].top , graphs[i].minColour);
							//graphTexture.SetPixel(graphIndex % graphTexture.width, graphs[i].bottom, graphs[i].minColour);

							graphs[i].latestValue = Mathf.Clamp(graphs[i].latestValue, graphs[i].minValue, graphs[i].maxValue);

							float per = ((graphs[i].latestValue - graphs[i].minValue) / (graphs[i].maxValue - graphs[i].minValue));
							int p =  Mathf.FloorToInt((graphs[i].top - graphs[i].bottom) * per);;
							texture.SetPixel(graphIndex % texture.width, graphs[i].bottom + p, Color.Lerp(graphs[i].minColour, graphs[i].maxColor, per));

							if (graphs[i].previous != graphs[i].latestValue) {
								int dir = (graphs[i].latestValue > graphs[i].previous) ? +1 : -1;

								int j = Mathf.FloorToInt((graphs[i].top - graphs[i].bottom) * ((graphs[i].previous - graphs[i].minValue) / (graphs[i].maxValue - graphs[i].minValue)));
								int k = Mathf.FloorToInt((graphs[i].top - graphs[i].bottom) * ((graphs[i].latestValue - graphs[i].minValue) / (graphs[i].maxValue - graphs[i].minValue)));

								for (int n = j; n != (k + dir); n += dir) {
									texture.SetPixel(graphIndex % texture.width, n + graphs[i].bottom, Color.Lerp(graphs[i].minColour, graphs[i].maxColor, (float)n / (float)(graphs[i].top - graphs[i].bottom)));
								}

								graphs[i].previous = graphs[i].latestValue;
							}
						}*/
					} else {
						//order values then draw lines

						values.Sort((a, b) => a.x.CompareTo(b.x));

                        ClearTexture();

                        float scaleY = (texture.height - (2 * Setup.settings.graphMargin)) / minValue;
                        float scaleX = (texture.width - (2 * Setup.settings.graphMargin)) / maxValue;
                        int scale = (scaleY < scaleX) ? Mathf.FloorToInt(scaleY) : Mathf.FloorToInt(scaleX);

                        //Draw Y axis
                        for (int i = 0; i < maxValue * scaleY; i++) {
                            for (int j = 0; j <= 1; j++) {
                                texture.SetPixel((int)Setup.settings.graphMargin - j, (int)Setup.settings.graphMargin + i, Color.black);
                            }
                        }

                        //Draw X axis
                        for (int i = 0; i < minValue * scaleX; i++) {
                            for (int j = 0; j < 1; j++) {
                                texture.SetPixel((int)Setup.settings.graphMargin + i, (int)Setup.settings.graphMargin - j, Color.black);
                            }
                        }

                        foreach (Vector2 v in values) {
                            for (int sx = 0; sx < scale; sx++) {
                                for (int sy = 0; sy < scale; sy++) {
                                    texture.SetPixel((int)Setup.settings.graphMargin + Mathf.FloorToInt(v.x * scaleX) + sx, (int)Setup.settings.graphMargin + Mathf.FloorToInt(v.y * scaleY) + sy, Color.red);
                                }
                            }
                        }


                    }
				} else if (gcType == GraphCanvasType.SCATTER_PLOT) {
					//Graph[] graphs = new Graph[graphDictionary.Count];
					//graphDictionary.Values.CopyTo (graphs, 0);

					float scaleY = (texture.height - (2* Setup.settings.graphMargin)) / minValue;
					float scaleX = (texture.width - (2* Setup.settings.graphMargin)) / maxValue;
					int scale = (scaleY < scaleX) ? Mathf.FloorToInt(scaleY) : Mathf.FloorToInt(scaleX);

					//Draw Y axis
					for (int i=0; i < maxValue * scaleY; i++) {
						for(int j=0; j<=1; j++) {
							texture.SetPixel((int)Setup.settings.graphMargin - j, (int)Setup.settings.graphMargin + i, Color.black);
						}
					}

					//Draw X axis
					for (int i=0; i < minValue * scaleX; i++) {
						for(int j=0; j<1; j++) {
							texture.SetPixel((int)Setup.settings.graphMargin + i, (int)Setup.settings.graphMargin - j, Color.black);
						}
					}

					float sumx = 0, sumy = 0;
					foreach (Vector2 v in values) {
						sumx += v.x;
						sumy += v.y;
						for(int sx = 0; sx < scale; sx++) {
							for (int sy = 0; sy < scale; sy++) {
								texture.SetPixel((int)Setup.settings.graphMargin + Mathf.FloorToInt(v.x * scaleX) + sx, (int)Setup.settings.graphMargin + Mathf.FloorToInt(v.y * scaleY) + sy, Color.red);
							}
						}
					}

					float deltaY = sumy / sumx;
					for (int i=0; i < (minValue * scaleX); i++) {
						if ((int)Setup.settings.graphMargin + Mathf.RoundToInt(i * deltaY) > maxValue * scaleY) break;
						texture.SetPixel((int)Setup.settings.graphMargin + i, (int)Setup.settings.graphMargin + Mathf.RoundToInt(i * deltaY), Color.blue);
					}

					setDirty = false;
				}
			}
		}

		private void UpdateGraphPosition() {
			image.uvRect = new Rect((float)graphIndex / (float)texture.width, 0, 1, 1);
		}

		public void SetPlotPoint(Vector2 value) {
			
			if(gcType == GraphCanvasType.LINE_PLOT && realtime) values.Clear();

			values.Add(value);
			setDirty = true;
		}

		private void ApplyTexture() {
			texture.Apply(false);
		}

		public void ClearTexture() {
			Color c = new Color(1f, 1f, 1f, 0f);

			for (int x = 0; x < texture.width; x++) {
				for (int y = 0; y < texture.height; y++) {
					texture.SetPixel(x, y, c);//new Color(1f, 1f, 1f, 0f));
				}
			}
			ApplyTexture ();
		}
	}
}
