using LiteNetLib.Utils;
using Lockstep.Framework.Commands;
using Lockstep.Framework.Networking;
using Lockstep.Framework.Services;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public int Count;
    public Transform Position;
    public GameObject Prefab;

    private void Start()
    {           
    }

    public void Spawn()
    {
        var sim = FindObjectOfType<LockstepSimulator>();
        for (int j = 0; j < Count; j++)
        {
            var x = new NetDataWriter();
            var z = new NetDataReader();         

            LockstepNetwork.Instance.SendCommand(CommandTag.Spawn, new SpawnCommand { AssetName = Prefab.name });
        }

    }
}
