using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHelper : MonoBehaviour
{
    public TMP_Text AgentCountText;
    public TMP_Text ConnectedText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AgentCountText.text = "Agents: " + Contexts.sharedInstance.game.GetEntities().Length;
        ConnectedText.text = "Connected: " + LockstepNetwork.Instance.Connected;
    }
}
