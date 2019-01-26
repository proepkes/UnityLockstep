using System.Collections.Generic;
using Entitas;

namespace Lockstep.Core.Systems.Input
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
            return context.CreateCollector(InputMatcher.Commands.Added());
        }

        protected override bool Filter(InputEntity entity)
        {
            return entity.hasCommands;
        }

        protected override void Execute(List<InputEntity> entities)
        {
            foreach (var command in _inputContext.commands.input)
            {
                command?.Execute(_inputContext);
            }
        }    
    }
}     
