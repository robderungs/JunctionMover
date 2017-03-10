using JunctionMoverShell.Model.Arguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Model.Command
{
    public interface ICommand
    {
        void Execute(IEnumerable<IArgument> arguments);
    }
}
