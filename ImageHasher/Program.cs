using System;
using CommandLine;

namespace ImageHasher
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      new Parser(config =>
        {
          config.HelpWriter = Console.Out;
          config.CaseSensitive = false;
        }).ParseArguments<RenameFilesOptions, FileOutputOptions>(args).
        WithParsed<RenameFilesOptions>(ImageHasher.RunRename).
        WithParsed<FileOutputOptions>(ImageHasher.RunFileOutput);
    }
  }
}