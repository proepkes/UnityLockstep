using System.Collections.Generic;
using Entitas;

namespace ECS.Systems.Input
{
    public class EmitInput : ReactiveSystem<InputEntity>
    {                                              
        private readonly InputContext _inputContext;    

        public EmitInput(Contexts contexts) : base(contexts.input)
        {                                  
            _inputContext = contexts.input;    
        }

        protected override ICollector<InputEntity> GetTrigger(IContext<InputEntity> context)
        {
            return context.CreateCollector(InputMatcher.Frame.Added());
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasFrame && entity.frame.value.Commands != null;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var command in _inputContext.frame.value.Commands)
            {
                command.Execute(_inputContext);
            }
        }    
    }
}     
