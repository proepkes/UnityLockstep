using System;

namespace Lockstep.Game.Networking
{
    public interface IClient
    {                           
        event Action<byte[]> DataReceived;  

        /// <summary>
        /// Send data reliable ordered
        /// </summary>                    
        void Send(byte[] data);   
    }
}