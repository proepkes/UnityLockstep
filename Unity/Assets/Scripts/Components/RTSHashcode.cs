using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSHashcode : MonoBehaviour, IEntityConfigurator
{
    public void Configure(GameEntity entity)
    {
        entity.isHashable = true;
    }
}
