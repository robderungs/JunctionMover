using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Command.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UsageAttribute : Attribute
    {
        public string Usage { get; private set; }

        public UsageAttribute(string usage)
        {
            Usage = usage;
        }
    }
}
