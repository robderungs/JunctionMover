using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Command.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandIdAttribute : Attribute
    {
        public int CommandId { get; private set; }

        public CommandIdAttribute(int id)
        {
            CommandId = id;
        }
    }
}
