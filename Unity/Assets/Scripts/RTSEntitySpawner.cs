using Lockstep.Framework.Commands;
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

public class RTSEntitySpawner : MonoBehaviour
{
    public int Count;             
    public RTSEntity Prefab; 
    private RTSEntityDatabase entityDatabase;    

    private void Start()
    {
        entityDatabase = RTSSimulator.Instance.EntityDatabase;    
        if (entityDatabase.Entities.IndexOf(Prefab) < 0)
        {
            Debug.LogError("Prefabs have to be added to the database in order to be spawnable");
        }
    }

    public void Spawn(Vector2 position)
    {                        
        for (int j = 0; j < Count; j++)
        {                                       
            LockstepNetwork.Instance.SendInput(new SpawnCommand
            {
                EntityConfigId = entityDatabase.Entities.IndexOf(Prefab),
                Position = position
            });
        }

    }
}
