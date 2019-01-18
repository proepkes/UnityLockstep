using BEPUutilities;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;
 
[Game] 
public class IdComponent : IComponent
{
    [PrimaryEntityIndex]
    public int value;
}
 
[Game]
public class AssetComponent : IComponent
{
    public string name;
}

[Game]
public class TeamComponent : IComponent
{
    public int value;
}

[Game]
public class ControllableComponent : IComponent
{
}
      
[Game]
public class DestinationComponent : IComponent
{
    public Vector2 value;
} 

[Game, Event(EventTarget.Self)]
public class PositionComponent : IComponent
{
    public Vector2 value;
}

[Game]
public class MovingComponent : IComponent
{

}

[Game]
public class VelocityComponent : IComponent
{
    public Fix64 value;
} 

[Game]
public sealed class HealthComponent : IComponent
{
    public int value;
}