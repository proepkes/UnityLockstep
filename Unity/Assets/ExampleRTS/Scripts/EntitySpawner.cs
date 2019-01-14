using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public int Count;
    public Transform Position;
    public GameObject EntityPrefab;

    private void Start()
    {                                
        var sim = FindObjectOfType<LockstepSimulator>();
        for (int j = 0; j < Count; j++)
        {
            var e = GameObject.Instantiate(EntityPrefab.gameObject).GetComponent<UnityLSAgent>();
            e.Init(Position.position);

            sim.RegisterEntity(e.agent);
        }
    }     
}
