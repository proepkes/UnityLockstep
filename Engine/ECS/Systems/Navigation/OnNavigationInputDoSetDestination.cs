using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Navigation
{
    public class OnNavigationInputDoSetDestination : ReactiveSystem<InputEntity>
    {                                               
        private readonly INavigationService _navigationService;     

        public OnNavigationInputDoSetDestination(Contexts contexts, INavigationService navigationService) : base(contexts.input)
        {                                 
            _navigationService = navigationService;  
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.NavigationInputData.Added());
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasNavigationInputData;
        }

        protected override void Execute(List<InputEntity> inputs)
        {     
            foreach (var input in inputs)
            {
                var destination = input.navigationInputData.Destination;
                foreach (var entityId in input.navigationInputData.EntityIds)
                {
                    _navigationService.SetAgentDestination(entityId, destination);            
                }  
            }
        }
    }
}
