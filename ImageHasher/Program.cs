using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using CommandLine;

namespace ImageHasher
{
  // ReSharper disable once ClassNeverInstantiated.Global
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