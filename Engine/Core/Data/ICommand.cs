namespace Lockstep.Core.Data
{
    public interface ICommand
    {             
        void Execute(InputContext context);
    }
}