using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSHashcode : MonoBehaviour, IComponentSetter
{
    public void SetComponent(GameEntity entity)
    {
        entity.isHashable = true;
    }
}
