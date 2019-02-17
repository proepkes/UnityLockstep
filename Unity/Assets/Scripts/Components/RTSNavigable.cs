using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSNavigable : MonoBehaviour, IEntityConfigurator
{      
    public void Configure(GameEntity entity)
    {
        entity.isNavigable = true;
    }
}
