using Entitas;                           

namespace Lockstep.Core.Components.Game
{   
    /// <summary>
    /// Flags an entity as newly created. Useful for rollback: if a shadow marked as new, the referenced entity just gets destroyed.
    /// </summary>
    public class NewComponent : IComponent
    {
    }
}
