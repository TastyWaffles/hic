using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageHasher
{
  public class ImageHasher
  {
    /*private static void HashDirectory(RenameFilesOptions options)
    {
      DirectoryInfo directory = new DirectoryInfo(options.Source);
    }

    private static void HashSingleFile(RenameFilesOptions options)
    {
      FileInfo file = new FileInfo(options.Source);
      string hash = ImageUtils.GetHashFromFile(file, HashAlgorithm.Create("MD5"));

      if (!options.Quiet)
      {
        Console.WriteLine(file.Name + " >> " + hash + file.Extension);
      }

      if (file.Directory == null)
      {
        return;
      }

      string destination = file.Directory.FullName + "\\" + hash + file.Extension;

      //if !copy
//      if (options. != null)
//      {
//        file.CopyTo(options.CopyFilesOptions);
//      }
//      else
//      {
//        file.MoveTo(options.MoveFiles ?? destination);
//      }
    }*/

/*    public static void RunRename(RenameFilesOptions options)
                {
                  Console.WriteLine(options.Source ?? "src is null");
                  Console.WriteLine(options.Copy);

                  if (options.Source != null &&
                      (File.GetAttributes(options.Source) & FileAttributes.Directory) == FileAttributes.Directory)
                  {
                    HashDirectory(options);
                  }
                  else
                  {
                    HashSingleFile(options);
                  }
                }*/


    public static void RunFileRename(RenameFilesOptions options)
    {
      using (IHashHandler handler = new RenamedHandler(options))
      {
        Run(handler, options);
      }
    }

    public static void RunFileOutput(FileOutputOptions options)
    {
      using (IHashHandler handler = new FileOutputHandler(options))
      {
        Run(handler, options);
      }
    }


    private static void Run(IHashHandler derp, BaseOptions options)
    {
      using (HashAlgorithm algorithm = HashAlgorithm.Create(options.Algorithm))
      {
        if (IsDirectory(options.Source))
        {
          foreach (string extension in ImageUtils.SupportedExtensions)
          {
            if (!options.ExcludedFileExtensions.Contains(extension))
            {
              DirectoryInfo dir = new DirectoryInfo(options.Source);
              IEnumerable<FileInfo> enumerateFiles = dir.EnumerateFiles("*" + extension,
                options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

              foreach (FileInfo file in enumerateFiles)
              {
                string hash = ImageUtils.GetHashFromFile(file, algorithm);
                derp.HandleFile(file, options.Lowercase ? hash.ToLower() : hash);
              }
            }
          }
        }
        else
        {
          FileInfo file = new FileInfo(options.Source);
          string hash = ImageUtils.GetHashFromFile(file, algorithm);

          derp.HandleFile(file, options.Lowercase ? hash.ToLower() : hash);
        }
      }
    }

    internal static string GetOutputDirectory(BaseOptions options)
    {
      if (options.OutputDir != null)
      {
        return options.OutputDir;
      }
      if (options.Source != null && IsDirectory(options.Source))
      {
        return options.Source;
      }
      return Directory.GetCurrentDirectory();
    }

    private static bool IsDirectory(string path)
    {
      return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
    }
  }
}