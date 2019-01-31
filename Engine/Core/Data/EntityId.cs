
namespace Lockstep.Core.Data
{                   
    public struct EntityId
    {                                              
        private readonly uint _value;
                                                                                    
        private EntityId(uint value)
        {
            this._value = value;
        }                                            
        public static implicit operator EntityId(uint value)
        {
            return new EntityId(value);
        }

        public static implicit operator uint(EntityId record)
        {
            return record._value;
        }
    }
}
