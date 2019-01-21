using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSNavigable : MonoBehaviour, IComponentSetter
{      
    public void SetComponent(GameEntity entity)
    {
        entity.isNavigable = true;
    }
}
