﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Business
{
    public interface IEntityBase<T>
    {
        T GetID();
    }
}
