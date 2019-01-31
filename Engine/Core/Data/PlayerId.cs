
namespace Lockstep.Core.Data
{
    public struct PlayerId
    {
        private readonly byte _value;

        private PlayerId(byte value)
        {
            this._value = value;
        }
        public static implicit operator PlayerId(byte value)
        {
            return new PlayerId(value);
        }

        public static implicit operator byte(PlayerId record)
        {
            return record._value;
        }
    }
}
