using Entitas;
using Entitas.CodeGeneration.Attributes;
using FixMath.NET;

[GameState, Unique]
public class GameStatePausedComponent : IComponent
{
}

[GameState, Unique]
public class GameStateInGameComponent : IComponent
{
}

[GameState, Unique]
public sealed class HashCodeComponent : IComponent
{
    public long value;
}