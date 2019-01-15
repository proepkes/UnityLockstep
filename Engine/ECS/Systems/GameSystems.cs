public class Feature : Entitas.Systems
{      
    public Feature(string name)
    {
    }

    public Feature()
    {
    }
}

public sealed class GameSystems : Entitas.Systems
{
    public GameSystems(Contexts contexts)
    {
        //// Input
        Add(new TickSystem(contexts));
        //Add(new ProcessInputSystem(contexts));

        //// Update
        //Add(new BoardSystem(contexts));
        //Add(new FallSystem(contexts));
        //Add(new FillSystem(contexts));
        //Add(new ScoreSystem(contexts));

        //// View
        //Add(new AddViewSystem(contexts));

        //// Events (Generated)
        //Add(new InputEventSystems(contexts));
        //Add(new GameEventSystems(contexts));
        //Add(new GameStateEventSystems(contexts));

        //// Cleanup (Generated, only with Entitas Asset Store version)
        //Add(new InputCleanupSystems(contexts));
        //Add(new GameCleanupSystems(contexts));
    }
}
