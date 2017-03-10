using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Services.FileSystem.Directory
{
    public interface IDirectoryMoverService
    {
        bool Move(string source, string destination);
        bool Move(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory);
    }
}
