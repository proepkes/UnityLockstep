namespace Lockstep.Core.Interfaces
{
    public interface ISystems
    {
        Contexts Contexts { get; }

        ICommandBuffer CommandBuffer { get; set; }          

        void Initialize();

        void Tick();                              
    }
}