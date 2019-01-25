using System;

namespace Lockstep.Client.Interfaces
{
    public interface INetwork
    {                           
        event Action<byte[]> DataReceived;  

        /// <summary>
        /// Send data reliable ordered
        /// </summary>                    
        void Send(byte[] data, int length);   
    }
}