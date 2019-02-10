using Entitas;
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
            CurrentTickText.text = "CurrentTick: " + Contexts.sharedInstance.gameState.tick.value;
            AgentCountText.text = "Agents: " + Contexts.sharedInstance.game.GetEntities(GameMatcher.LocalId).Length;
        }


        ConnectedText.text = "Connected: " + RTSNetworkedSimulation.Instance.Connected;
    }
}
