using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using static Main.Utils;
namespace Main
{
  public class Shell : IBaseParser<ShellOptions>
  {
    private Dictionary<String, Type> Commands = new Dictionary<string, Type>
    {
      {"cat", typeof(ConcatenateFiles)},
      {"ls", typeof(ListFiles)},
      {"cls", typeof(ClearScreen)},
      {"pwd", typeof(PrintWorkingDirectory)},
      {"touch", typeof(Touch)},
      {"mkdir", typeof(MakeDirectory)},
      {"cd", typeof(ChangeDirectory)}
      //{"alias", typeof(SetAlias)}
      //{"set", typeof(SetAlias)}
      //{"env", typeof(SetAlias)}
    };
    private Dictionary<String, String> Aliases = new Dictionary<String, String>
    {
      {"clear", "cls"}
    };
    private Dictionary<String, String> Env = new Dictionary<String, String>
    {
      {"PS1", ">"}
    };

    private string Prompt { get; set; }
    
    public Shell(string[] args)
    {
      ((IBaseParser<ShellOptions>)(this)).Parse(args);
    }

    public void ParseOpts(ShellOptions opts)
    {
      //do nothing yet no options to parse.
      InitShell();
      WelcomeMessage();
      Run();
      Terminate();
    }

    public void HandleParseError(IEnumerable<Error> errs)
    {
      //handle errors.
    }

    private void InitShell()
    {
      //Do run commands or something like that
      //Get environment variables, home directory, current working directory, etc.
      //Set prompt.
      //Also get command history
      Prompt = Env["PS1"];
      var enVars = Environment.GetEnvironmentVariables();
      foreach (DictionaryEntry de in enVars)
      {
        System.Console.WriteLine($"{de.Key}={de.Value}");
      }
    }

    private void WelcomeMessage()
    {
      Console.WriteLine("Welcome to bash#, a mini bash-like shell made with C#.");
    }

    private void PrintPrompt()
    {
      Console.Write(Prompt);
    }
    private void Run()
    {
      string input = null;
      for ( ; ; )
      {
        PrintPrompt();
        input = Console.ReadLine();
        if (input == "exit" || input == null) break;
        Execute(input);
      }

    }

    private void Terminate()
    {
      //Shut down code in here. Save stuff, close resources, etc.
      Console.WriteLine("logout");
    }

    private void InvalidCommand(string cmd)
    {
      Console.WriteLine($"sh#: {cmd}: Command not found.");
    }
    

    private (string[], string) ParseCommand(string input)
    {
      //Remove comments and split into arguments
      var ret = RemoveComment(input).Split(' ');
      return (ret, ret[0]);
    }

    private void Execute(string input, bool aliased = false)
    {
      (string[] args, string cmdName) = ParseCommand(input);
      //cmd name is the first word.
      if (string.IsNullOrWhiteSpace(cmdName))
      {
        return;
      }
      else if (!aliased && Aliases.TryGetValue(cmdName, out string alias))
      {
        Execute(alias, true);
      }
      else if (Commands.TryGetValue(cmdName, out Type cmdType))
      {
        RunCommand(cmdType, args);
      }
      else
      {
        InvalidCommand(cmdName);
      }
    }
    

    private void RunCommand(Type cmdType, string[] args)
    {
      var cmdInstance = (IRunner)Activator.CreateInstance(cmdType);
      //Use Skip to ignore the first word which is the command, so it doesn't get parsed.
      cmdInstance.Run(args.Skip(1).ToArray());
    }
  }
}