namespace Lockstep.Core.Interfaces
{
    public interface ICommand
    {             
        void Execute(InputEntity inputEntity);
    }
}