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
public sealed class GameHashCodeComponent : IComponent
{
    public long value;
}