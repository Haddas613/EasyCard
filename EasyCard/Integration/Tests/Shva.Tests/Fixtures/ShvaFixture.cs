using Shva.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Tests.Fixtures
{
    public class ShvaFixture : IDisposable
    {
        public ShvaSettings ShvaSettings { get; private set; }
        public ShvaFixture()
        {
            //TODO: assign values
            ShvaSettings = new ShvaSettings { };
        }

        public void Dispose()
        {
            //Dispose context, clients etc..
        }
    }
}
