using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Command.Attributes
{
    public class CommandDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public CommandDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
