using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageHasher
{
  public static class ImageHasher
  {
    public static void RunRename(RenameFilesOptions options)
    {
      using (IHashHandler handler = new RenameHandler(options))
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

    private static void Run(IHashHandler handler, BaseOptions options)
    {
      using (HashAlgorithm algorithm = HashAlgorithm.Create(options.Algorithm))
      {
        if (ImageUtils.IsDirectory(options.Source))
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
                handler.HandleFile(file, options.Lowercase ? hash.ToLower() : hash);
              }
            }
          }
        }
        else
        {
          FileInfo file = new FileInfo(options.Source);
          string hash = ImageUtils.GetHashFromFile(file, algorithm);
          handler.HandleFile(file, options.Lowercase ? hash.ToLower() : hash);
        }
      }
    }
  }
}