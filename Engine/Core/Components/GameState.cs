using Entitas;
using Entitas.CodeGeneration.Attributes;

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


[GameState, Unique]
public sealed class PlayerIdComponent : IComponent
{
    public byte value;
}