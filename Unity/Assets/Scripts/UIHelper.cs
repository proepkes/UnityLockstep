using UnityEngine;
using UnityEngine.UI;

public class UIHelper : MonoBehaviour
{
    public Text ConnectedText;
    public Text AgentCountText;    

    // Update is called once per frame
    void Update()
    {
        ConnectedText.text = "Connected: " + RTSNetworkedSimulation.Instance.Connected;
        AgentCountText.text = "Agents: " + Contexts.sharedInstance.game.GetEntities().Length;
    }
}
