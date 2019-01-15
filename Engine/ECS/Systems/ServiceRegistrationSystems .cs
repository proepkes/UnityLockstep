using ECS.Services;
using Entitas;

public class ServiceRegistrationSystems : Systems
{
    public ServiceRegistrationSystems(Contexts contexts, Services services)
    {
        Add(new RegisterCommandServiceSystem(contexts, services.Command));         
    }
}

public class RegisterCommandServiceSystem : IInitializeSystem
{
    private readonly ServiceContext _serviceContext;
    private readonly ICommandService _commandService;

    public RegisterCommandServiceSystem(Contexts contexts, ICommandService timeService)
    {
        _serviceContext = contexts.service;
        _commandService = timeService;
    }

    public void Initialize()
    {
        _serviceContext.ReplaceTimeService(_timeService);
    }
}