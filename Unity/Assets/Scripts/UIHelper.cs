using UnityEngine;
using UnityEngine.UI;

public class UIHelper : MonoBehaviour
{
    public Text HashCodeText;
    public Text AgentCountText;

    public Text ConnectedText;
    public Text CurrentTickText;

    // Update is called once per frame
    void Update()
    {
        if (RTSNetworkedSimulation.Instance.Simulation.Running)
        {
            HashCodeText.text = "HashCode: " + Contexts.sharedInstance.gameState.hashCode.value;
            CurrentTickText.text = "CurrentTick: " + RTSNetworkedSimulation.Instance.Systems.CurrentTick;
        }

        AgentCountText.text = "Agents: " + RTSNetworkedSimulation.Instance.Systems.EntitiesInCurrentTick;

        ConnectedText.text = "Connected: " + RTSNetworkedSimulation.Instance.Connected;
    }
}
