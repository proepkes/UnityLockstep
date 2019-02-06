namespace Lockstep.Core.Commands
{
    public interface ICommand
    {
        void Execute(InputEntity inputEntity);
    }
}