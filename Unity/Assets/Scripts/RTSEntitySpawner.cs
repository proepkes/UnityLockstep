using Lockstep.Commands;             
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

public class RTSEntitySpawner : MonoBehaviour
{
    public int Count;             
    public GameObject Prefab;   

    private void Start()
    {  
        if (RTSSimulator.Instance.EntityDatabase.Entities.IndexOf(Prefab) < 0)
        {
            Debug.LogError("Prefabs have to be added to the database in order to be spawnable");
        }
    }

    public void Spawn(Vector2 position)
    {                        
        for (int j = 0; j < Count; j++)
        {
            RTSSimulator.Instance.Simulation.Execute(new SpawnCommand
            {
                EntityConfigId = RTSSimulator.Instance.EntityDatabase.Entities.IndexOf(Prefab),
                Position = position
            });
        }

    }
}
