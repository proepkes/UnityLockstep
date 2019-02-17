using System;
using System.Collections.Generic;
using FixMath.NET;
using Lockstep.Core.State.Game;
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

[Serializable]
public class RTSAgent : MonoBehaviour, IEntityConfigurator
{                  
    public int MaxSpeed = 2;

    public int Radius = 2;

    public void Configure(GameEntity e)
    {
        e.isNavigable = true;
        e.AddRadius(Radius);
        e.AddMaxSpeed(MaxSpeed);
        e.AddAgent(Vector2.Zero, Vector2.Zero, MaxSpeed, 15, 10, 5, new List<Line>());
    }
}
