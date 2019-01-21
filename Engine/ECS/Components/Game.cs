using BEPUutilities;
using Entitas;
using Entitas.CodeGeneration.Attributes;   
 
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

[Game, Event(EventTarget.Self), Event(EventTarget.Self, EventType.Removed)]
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
public sealed class HealthComponent : IComponent
{
    public int value;
}

[Game]
public sealed class NavigableComponent : IComponent
{                      
}

[Game]
public sealed class HashableComponent : IComponent
{                     
}

