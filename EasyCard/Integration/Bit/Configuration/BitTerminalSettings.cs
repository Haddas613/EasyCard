﻿using Shared.Integration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bit
{
    public class BitTerminalSettings : IExternalSystemSettings
    {
        public Task<bool> Valid()
        {
            return Task.FromResult(true);
        }
    }
}