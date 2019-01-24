namespace ECS.Data
{
    public interface ICommand
    {             
        void Execute(InputContext context);
    }
}