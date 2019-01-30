using UnityEngine;   
using DebugTools;

public class PlotCommandBufferOffset : MonoBehaviour
{

    GraphCanvas graph;  

    void Start()
    {
        graph = Grapher.CreateGraph("CommandBufferOffset", GraphCanvasType.LINE_PLOT, true, true, 0f, 20f);
    }
                                      
    void Update()
    {      
    }
}
