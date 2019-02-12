using System;        
using BEPUutilities;
using Lockstep.Core.Logic;
using Moq;           
using Xunit;
using Xunit.Abstractions;

namespace Test
{                                                                                 
    public class NavigationTests
    {          
        private readonly ITestOutputHelper _output;

        public NavigationTests(ITestOutputHelper output)
        {                                    
            _output = output;
            Console.SetOut(new Converter(output));   
        }    
    }
}
