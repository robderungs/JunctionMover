using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunctionMoverShell.Services.FileSystem.Directory
{
    public class BasicDirectoryMoverService : IDirectoryMoverService
    {
        public bool Move(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            new Computer().FileSystem.MoveDirectory(sourceDirectory.FullName, destinationDirectory.FullName, UIOption.AllDialogs);
            return true;
        }

        public bool Move(string source, string destination)
        {
            var sourceDirectory = new DirectoryInfo(source);
            var destinationDirectory = new DirectoryInfo(destination);
            return Move(sourceDirectory, destinationDirectory);
        }
    }
}
