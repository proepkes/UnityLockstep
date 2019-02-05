namespace Lockstep.Game.Commands
{
    public interface ICommand
    {
        void Execute(InputEntity inputEntity);
    }
}