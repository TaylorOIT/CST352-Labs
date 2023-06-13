// Assignment 4
// Pete Myers
// OIT, Spring 2018
// Handout

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleFileSystem;

namespace SimpleShell
{
    public class SimpleShell : Shell
    {
        private abstract class Cmd
        {
            private string name;
            private SimpleShell shell;

            public Cmd(string name, SimpleShell shell) { this.name = name; this.shell = shell; }

            public string Name => name;
            public SimpleShell Shell => shell;
            public Session Session => shell.session;
            public Terminal Terminal => shell.session.Terminal;
            public FileSystem FileSystem => shell.session.FileSystem;
            public SecuritySystem SecuritySystem => shell.session.SecuritySystem;

            abstract public void Execute(string[] args);
            virtual public string HelpText { get { return ""; } }
            virtual public void PrintUsage() { Terminal.WriteLine("Help not available for this command"); }

            // helpers for any cmds that work with paths
            protected string MakeFullPath(string path)
            {

                if (path.StartsWith("/"))
                {
                    // it's already a full path, so we just return it
                    return path;
                }

                // modify the cwd using the path
                List<string> cwdParts = new List<string>(Shell.cwd.FullPathName.Split('/'));
                if (cwdParts.Count == 2 && cwdParts[1].Length == 0)
                    cwdParts.RemoveAt(1);

                string[] pathParts = path.Split('/');

                int i = 0;
                foreach (string part in pathParts)
                {
                    if (part == ".")
                    {
                        // do nothing
                    }
                    else if (part == "..")
                    {
                        // go to parent
                        if (cwdParts.Count == 1)
                            throw new Exception("No parent!");
                        cwdParts.RemoveAt(cwdParts.Count - 1);
                    }
                    else
                    {
                        // descend
                        cwdParts.Add(part);
                    }
                }
                if (cwdParts.Count == 1)
                    return "/";
                return string.Join("/", cwdParts);
            }
        }

        private Session session;
        private Directory cwd;
        private Dictionary<string, Cmd> cmds;   // name -> Cmd
        private bool running;

        public SimpleShell(Session session)
        {
            this.session = session;
            cwd = null;
            cmds = new Dictionary<string, Cmd>();
            running = false;

            AddCmd(new ExitCmd(this));
            AddCmd(new PwdCmd(this));
            AddCmd(new CdCmd(this));
            AddCmd(new LsCmd(this));
            AddCmd(new HelpCmd(this));
        }

        private void AddCmd(Cmd c) { cmds[c.Name] = c; }

        public void Run(Terminal terminal)
        {
            // NOTE: takes over the current thread, returns only when shell exits
            // expects terminal to already be connected

            // set the initial current working directory
            cwd = session.HomeDirectory;

            // main loop...
            running = true;
            while (running)
            {
                // print command prompt
                terminal.Write(cwd.FullPathName + ">");

                // get command line
                string cmdline = terminal.ReadLine().Trim();

                // identify and execute command
                string[] args = cmdline.Split(' ');
                if (cmds.ContainsKey(args[0]))
                {
                    cmds[args[0]].Execute(args);
                }
                else
                {
                    // invalid command!
                    terminal.WriteLine("Error: command not found!");
                }
            }

        }

        #region commands

        // example command: exit
        private class ExitCmd : Cmd
        {
            public ExitCmd(SimpleShell shell) : base("exit", shell) { }

            public override void Execute(string[] args)
            {
                Terminal.WriteLine("Bye!");
                Shell.running = false;
            }

            override public string HelpText { get { return "Exits shell"; } }

            override public void PrintUsage()
            {
                Terminal.WriteLine("usage: exit");
            }
        }

        private class PwdCmd : Cmd
        {
            public PwdCmd(SimpleShell shell) : base("pwd", shell) { }

            public override void Execute(string[] args)
            {
                // prints the full path of the current working directory
                Terminal.WriteLine("Current working directory: " + Shell.cwd.FullPathName);
            }

            override public string HelpText { get { return "Prints the full path of the current working directory"; } }

            override public void PrintUsage()
            {
                Terminal.WriteLine("usage: pwd");
            }
        }

        private class CdCmd : Cmd
        {
            public CdCmd(SimpleShell shell) : base("cd", shell) { }


            public override void Execute(string[] args)
            {
                // Changes the current directory to the specified path
                try
                {
                    // verify cmd line args
                    if (args.Length != 2)
                        throw new Exception("Expect only 1 argument!");

                    string path = args[1];

                    path = MakeFullPath(path);

                    FSEntry entry = FileSystem.Find(path);
                    if (entry == null)
                        throw new Exception("Directory not found!");
                    if (entry.IsFile)
                        throw new Exception("Path must be a directory!");
                    Shell.cwd = entry as Directory;
                }
                catch (Exception ex)
                {
                    Terminal.WriteLine("Error: " + ex.Message);
                }
            }
            override public string HelpText { get { return "Changes the current directory to the specified path"; } }

            override public void PrintUsage()
            {
                Terminal.WriteLine("usage: cd <path>");
            }
        }

        private class LsCmd : Cmd
        {
            public LsCmd(SimpleShell shell) : base("ls", shell) { }

            public override void Execute(string[] args)
            {
                // prints the contents of the current working directory

                // validate the cmd line args
                if (args.Length > 1)
                    Terminal.WriteLine("Error: too many args!");

                // print the contents..
                foreach (Directory d in Shell.cwd.GetSubDirectories())
                {
                    Terminal.WriteLine(d.Name + "/");
                }

                foreach (File f in Shell.cwd.GetFiles())
                {
                    Terminal.WriteLine(f.Name);
                }
            }

            override public string HelpText { get { return "Prints the contents of the current working directory"; } }

            override public void PrintUsage()
            {
                Terminal.WriteLine("usage: ls");
            }
        }
        private class HelpCmd : Cmd
        {
            public HelpCmd(SimpleShell shell) : base("help", shell) { }

            public override void Execute(string[] args)
            {
                // prints the list of available shell commands

                // validate the cmd line args
                if (args.Length == 1)
                {
                    // print the available commands...
                    foreach(string cmdName in Shell.cmds.Keys)
                    {
                        Terminal.WriteLine(cmdName + " - " + Shell.cmds[cmdName].HelpText);
                    }
                }
                else if (args.Length == 2)
                {
                    // print help and usage for given cmd name
                    string cmdName = args[1];
                    if(Shell.cmds.ContainsKey(cmdName))
                    {
                        Terminal.WriteLine(cmdName + " - " + Shell.cmds[cmdName].HelpText);
                        Shell.cmds[cmdName].PrintUsage();
                    }
                    else
                    {
                        Terminal.WriteLine("Error: cmd not found!");
                    }
                }
            }

            override public string HelpText { get { return "Prints the list of available shell commands"; } }

            override public void PrintUsage()
            {
                Terminal.WriteLine("usage: help");
            }
        }
    }

    #endregion
}
