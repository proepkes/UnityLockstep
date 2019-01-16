using Entitas;
                    
public class RegisterServicesSystem : IInitializeSystem
{
    private readonly ServiceContext _serviceContext;
    private readonly ExternalServices _externalServices;         

    public RegisterServicesSystem(Contexts contexts, ExternalServices externalServices)
    {                      
        _serviceContext = contexts.service;
        _externalServices = externalServices;             
    }

    public void Initialize()
    {
        _serviceContext.SetCommandService(_externalServices.Command);
        _serviceContext.SetTimeService(_externalServices.Time);
        _serviceContext.SetGridService(_externalServices.Grid);
    }
}