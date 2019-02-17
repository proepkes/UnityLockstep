using System.Collections.Generic;
using FixMath.NET;
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

public class RTSAgent : MonoBehaviour, IEntityConfigurator
{
    [FixedNumber]
    public Fix64 Radius;

    [FixedNumber]
    public Fix64 MaxSpeed = 1;

    public void Configure(GameEntity e)
    {
        e.isNavigable = true;
        e.AddRadius(Radius);
        e.AddMaxSpeed(MaxSpeed);
        e.AddRvoAgentSettings(Vector2.Zero, 15, 10, new List<KeyValuePair<Fix64, GameEntity>>());
    }
}
