namespace Lockstep.Framework.Commands
{
    public interface ICommand
    {                       
        void Execute(InputContext context);
    }
}