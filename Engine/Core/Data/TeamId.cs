
namespace Lockstep.Core.Data
{                   
    public struct TeamId
    {                                              
        private readonly byte _value;
                                                                                    
        private TeamId(byte value)
        {
            this._value = value;
        }                                            
        public static implicit operator TeamId(byte value)
        {
            return new TeamId(value);
        }

        public static implicit operator byte(TeamId record)
        {
            return record._value;
        }
    }
}
