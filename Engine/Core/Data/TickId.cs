
namespace Lockstep.Core.Data
{                   
    public struct TickId
    {                                              
        private readonly uint _value;
                                                                                    
        private TickId(uint value)
        {
            this._value = value;
        }                                            
        public static implicit operator TickId(uint value)
        {
            return new TickId(value);
        }

        public static implicit operator uint(TickId record)
        {
            return record._value;
        }

        public static bool operator ==(TickId first, TickId second)
        {
            return first._value == second._value;
        }

        public static bool operator !=(TickId first, TickId second)
        {
            return first._value != second._value;
        }
    }
}
