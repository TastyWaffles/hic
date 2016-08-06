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

          //relevantDirPath = file.Directory.FullName.Remove(source);

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