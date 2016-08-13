using System;
using System.IO;
using System.Linq;

namespace ImageHasher
{
  public class RenameHandler : IHashHandler
  {
    private readonly RenameFilesOptions _options;

    public RenameHandler(RenameFilesOptions options)
    {
      _options = options;
    }

    public void Dispose()
    {
      //todo
    }

    public void HandleFile(FileInfo file, string hash)
    {
      if (file.Directory == null)
      {
        return;
      }

      string destination;

      if (ImageUtils.IsDirectory(_options.OutputDir))
      {
        if (_options.LeaveInPlace)
        {
          //todo make path relevant to source dir
          string relevantDirPath = ""; //should be full path of current file from sourceDir down
          string source = ImageUtils.IsDirectory(_options.Source) ? _options.Source : Directory.GetCurrentDirectory();



          destination = Path.Combine(_options.OutputDir, relevantDirPath, hash + file.Extension);
        }
        else
        {
          destination = Path.Combine(_options.OutputDir, hash + file.Extension);
        }
      }
      else
      {
        destination = Path.Combine(file.Directory.FullName, hash + file.Extension);
      }


      if (_options.Copy)
      {
        file.CopyTo(destination);
      }
      else
      {
        file.MoveTo(destination);
      }
    }
  }
}

/*
class Program
{
    static void Main(string[] args)
    {
        CloneDirectory(@"C:\SomeRoot", @"C:\SomeOtherRoot");
    }

    private static void CloneDirectory(string root, string dest)
    {
        foreach (var directory in Directory.GetDirectories(root))
        {
            string dirName = Path.GetFileName(directory);
            if (!Directory.Exists(Path.Combine(dest, dirName)))
            {
                Directory.CreateDirectory(Path.Combine(dest, dirName));
            }
            CloneDirectory(directory, Path.Combine(dest, dirName));
        }

        foreach (var file in Directory.GetFiles(root))
        {
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
        }
    }
}
*/