using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Command.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresArgumentsAttribute : Attribute
    {
        public Type[] ArgumentTypes { get; private set; }

        public RequiresArgumentsAttribute(params Type[] argumentTypes)
        {
            ArgumentTypes = argumentTypes;
        }
    }

}
