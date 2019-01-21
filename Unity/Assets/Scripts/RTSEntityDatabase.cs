
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EntityDatabase.asset", menuName = "UnityLockstep/Entity Database")]
public class RTSEntityDatabase : ScriptableObject
{
    public List<GameObject> Entities;  
}
