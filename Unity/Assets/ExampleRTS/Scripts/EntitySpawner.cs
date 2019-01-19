using Lockstep.Framework.Commands;    
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

public class EntitySpawner : MonoBehaviour
{
    public int Count;             
    public GameObject Prefab;
    public bool Movable;   

    public void Spawn(Vector2 position)
    {                                                      
        for (int j = 0; j < Count; j++)
        {                                       
            LockstepNetwork.Instance.SendInput(new SpawnCommand
            {
                AssetName = Prefab.name,
                Movable = Movable  , Position = position
            });
        }

    }
}
