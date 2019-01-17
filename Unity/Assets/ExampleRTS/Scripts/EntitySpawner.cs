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
            new SpawnCommand { AssetName = Prefab.name }.Serialize(x);
            var z = new NetDataReader(x.Data);
            
            new SpawnCommand ().Deserialize(z);

            LockstepNetwork.Instance.SendInput(CommandTag.Spawn, new SpawnCommand { AssetName = Prefab.name });
        }

    }
}
