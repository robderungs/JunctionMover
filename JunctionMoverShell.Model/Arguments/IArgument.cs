﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Arguments
{
    public interface IArgument
    {
        string Key { get; }
        string Value { get; set; }
    }
}
