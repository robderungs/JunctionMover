using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using StructureMap;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Devices;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using JunctionMoverShell.Model.Command;
using JunctionMoverShell.Model.Command.Attributes;
using JunctionMoverShell.Model.Arguments;
using JunctionMoverShell.Services.FileSystem.Directory;

namespace JunctionMoverShell
{

    public static class ArgumentExtentions
    {
        public static IArgument GetArgumentByKey(this IEnumerable<IArgument> arguments, string key)
        {
            return arguments.Single(p => string.Equals(p.Key, key, StringComparison.InvariantCultureIgnoreCase));
        }

        public static T GetArgument<T>(this IEnumerable<IArgument> arguments) where T : IArgument
        {
            return arguments.OfType<T>().Single();
        }
    }


    public static class CommandExtensions
    {
        public static string GetDescription(this ICommand command)
        {
            return command.GetType().GetCustomAttribute<CommandDescriptionAttribute>()?.Description;
        }

        public static int? GetCommandId(this ICommand command)
        {
            return command.GetType().GetCustomAttribute<CommandIdAttribute>()?.CommandId;
        }

        public static string GetUsage(this ICommand command)
        {
            return command.GetType().GetCustomAttribute<UsageAttribute>()?.Usage;
        }

    }

    public interface IFolderIconService
    {
        void SetIcon(string folder, string iconPath);

    }

    public class FolderIconService : IFolderIconService
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        static extern UInt32 SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize;
            public UInt32 dwMask;
            public IntPtr pvid;
            public string pszWebViewTemplate;
            public UInt32 cchWebViewTemplate;
            public string pszWebViewTemplateVersion;
            public string pszInfoTip;
            public UInt32 cchInfoTip;
            public IntPtr pclsid;
            public UInt32 dwFlags;
            public string pszIconFile;
            public UInt32 cchIconFile;
            public int iIconIndex;
            public string pszLogo;
            public UInt32 cchLogo;
        }

        public void SetIcon(string folder, string iconPath)
        {
            Console.WriteLine($"Writing icon {iconPath} to folder {folder}");

            LPSHFOLDERCUSTOMSETTINGS FolderSettings = new LPSHFOLDERCUSTOMSETTINGS();
            FolderSettings.dwMask = 0x10;
            FolderSettings.pszIconFile = iconPath;
            FolderSettings.iIconIndex = 0;

            UInt32 FCS_READ = 0x00000001;
            UInt32 FCS_FORCEWRITE = 0x00000002;
            UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

            UInt32 HRESULT = SHGetSetFolderCustomSettings(ref FolderSettings, folder, FCS_WRITE);
        }
    }



    public interface ISymbolicLinkService
    {
        void CreateLink(string source, string destination);
    }

    public class SymbolicLinkService : ISymbolicLinkService
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public void CreateLink(string source, string destination)
        {
            var sourceFileExists = File.Exists(source);
            var sourceDirectoryExists = Directory.Exists(source);
            var destinationFileExists = File.Exists(destination);
            var destinationDirectoryExists = Directory.Exists(destination);

            if (!(destinationFileExists || destinationDirectoryExists))
            {
                throw new ArgumentException($"Source '{source}' does not exist!");
            }

            if (sourceFileExists || sourceDirectoryExists)
            {
                throw new ArgumentException($"Destination '{destination} already exists!");
            }

            CreateSymbolicLink(source, destination, destinationFileExists ? SymbolicLink.File : SymbolicLink.Directory);
        }
    }



  

    [CommandId(1000)]
    [CommandDescription("Moves a folder to the ProgramFiles folder")]
    [RequiresArguments(typeof(CommandIdArgument), typeof(SourcePathArgument))]
    public class MoveToProgramsCommand : ICommand
    {
        private readonly IDirectoryMoverService _directoryMoverService;

        public MoveToProgramsCommand(IDirectoryMoverService directoryMoverService)
        {
            _directoryMoverService = directoryMoverService;
        }

        public void Execute(IEnumerable<IArgument> arguments)
        {
            var sourcePath = arguments.GetArgument<SourcePathArgument>();
        }
    }

    [CommandId(1001)]
    [CommandDescription("Moves a folder to the same location on a different drive and creates a symbolic link at the source path")]
    [Usage("commandId=1001 sourcePath=C:\\example")]
    [RequiresArguments(typeof(SourcePathArgument))]
    public class MoveToDAndCreateSymLinkCommand : ICommand
    {
        private readonly IDirectoryMoverService _directoryMoverService;
        private readonly ISymbolicLinkService _symbolicLinkService;
        private readonly IFolderIconService _folderIconService;

        public MoveToDAndCreateSymLinkCommand(IDirectoryMoverService directoryMoverService, ISymbolicLinkService symbolicLinkService, IFolderIconService folderIconService)
        {
            _directoryMoverService = directoryMoverService;
            _symbolicLinkService = symbolicLinkService;
            _folderIconService = folderIconService;
        }

        public void Execute(IEnumerable<IArgument> arguments)
        {
            var sourcePath = arguments.GetArgument<SourcePathArgument>().Value;
            var destinationPath = sourcePath.Replace("C:", "D:");

            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);

            var iconFile = new FileInfo(directory + "\\FolderWithWarningIcon.ico");

            _directoryMoverService.Move(sourcePath, destinationPath);
            _symbolicLinkService.CreateLink(sourcePath, destinationPath);

            SharpShell.SharpIconOverlayHandler.SharpIconOverlayHandler h;

            _folderIconService.SetIcon(sourcePath, iconFile.FullName);
            _folderIconService.SetIcon(destinationPath, iconFile.FullName);
        }
    }


    public class CommandFactory
    {
        private readonly IContainer _container;

        public CommandFactory(IContainer container)
        {
            _container = container;
        }

        public ICommand Create(CommandIdArgument commandIdArgument)
        {
            int id;
            if (commandIdArgument == null || !int.TryParse(commandIdArgument.Value, out id))
            {
                return null;
            }

            return _container.GetAllInstances<ICommand>().SingleOrDefault(p => p.GetType().GetCustomAttribute<CommandIdAttribute>().CommandId == id);
        }
    }

    public class SourcePathArgument : IArgument
    {
        public string Key => "SourcePath";

        public string Value { get; set; }
    }

    public class CommandIdArgument : IArgument
    {
        public string Key => "CommandId";
        public string Value { get; set; }
    }

    public class ArgumentFactory
    {
        private readonly IContainer _container;

        public ArgumentFactory(IContainer container)
        {
            _container = container;
        }

        public IArgument Create(string arg)
        {
            if (string.IsNullOrEmpty(arg) || !arg.Contains("="))
            {
                return null;
            }

            var fragments = arg.Split('=');
            var key = fragments[0];
            var value = fragments[1];
            var argumentTypes = _container.GetAllInstances<IArgument>();
            var argument = argumentTypes.SingleOrDefault(p => string.Equals(p.Key, key, StringComparison.InvariantCultureIgnoreCase));

            if (argument == null || string.IsNullOrEmpty(value))
            {
                return null;
            }

            argument.Value = value;

            return argument;
        }
    }


    class Program
    {
        public static void ShowAvailableCommands(IContainer container)
        {
            Console.WriteLine("No command specified.");
            var availableCommands = container.GetAllInstances<ICommand>();

            Console.WriteLine("Available commands:");

            foreach (var cmd in availableCommands)
            {
                Console.WriteLine($"Id: {cmd.GetCommandId()}");
                Console.WriteLine($"Description: {cmd.GetDescription() ?? "No description specified"}");
                Console.WriteLine($"Usage: {cmd.GetUsage() ?? "No usage specified"}");
                Console.WriteLine();
            }

            Console.ReadKey();
            return;
        }



        static void Main(string[] args)
        {
            // var directoryJunctionAttribute = System.IO.FileAttributes.ReparsePoint;

            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.AssembliesAndExecutablesFromApplicationBaseDirectory();
                    x.AddAllTypesOf<ICommand>();
                    x.AddAllTypesOf<IArgument>();
                    x.AddAllTypesOf<IDirectoryMoverService>();
                    x.AddAllTypesOf<ISymbolicLinkService>();
                    x.AddAllTypesOf<IFolderIconService>();
                    x.WithDefaultConventions();
                });
            });

            var argumentFactory = container.GetInstance<ArgumentFactory>();
            var commandFactory = container.GetInstance<CommandFactory>();

            var arguments = args.Select(p => argumentFactory.Create(p)).Where(p => p != null);

            if (arguments.Count() <= 0)
            {
                ShowAvailableCommands(container);
                return;
            }

            var commandIdArgument = arguments.GetArgument<CommandIdArgument>();
            var command = commandFactory.Create(commandIdArgument);

            command.Execute(arguments);

            Console.ReadKey();
        }
    }
}
