using BEPUutilities;
using Entitas;
using FixMath.NET;


public class MovableComponent : IComponent
{
}

public class MovingComponent : IComponent
{
}    

public class PositionComponent : IComponent
{
    public Vector2 value;
}     

public class DirectionComponent : IComponent
{
    public Fix64 value;
}