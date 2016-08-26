using System;
using System.IO;
using System.Linq;

namespace ImageHasher
{
  public class RenameHandler : IHashHandler
  {
    private readonly RenameFilesOptions _options;
    private readonly string _rootParentDir;
    private readonly string _outputDir;

    public RenameHandler(RenameFilesOptions options)
    {
      _options = options;

      string source = HasherUtils.IsDirectory(_options.Source) ? _options.Source : Directory.GetCurrentDirectory();
      _rootParentDir = new DirectoryInfo(source).Name;

      _outputDir = HasherUtils.GetOutputDirectory(_options);
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

      if (HasherUtils.IsDirectory(_outputDir))
      {
        if (_options.PreserveSubDir)
        {
          //make path relevant to source dir

          //split filepath on path separator
          string[] fileSplit = file.FullName.Split(Path.DirectorySeparatorChar);

          //find source parent dir name in filepath
          int index = Array.IndexOf(fileSplit, _rootParentDir);

          string finalOutputDirPath;
          if (index < 0)
          {
            finalOutputDirPath = _outputDir;
          }
          else
          {
            //want to skip the current index,
            //but leave room at start for the outputDir in order to make building the path easier
            int len = fileSplit.Length - index;
            string[] tmp = new string[len];
            tmp[0] = _outputDir;
            Array.Copy(fileSplit, index + 1, tmp, 1, len);

            finalOutputDirPath = Path.Combine(tmp);

            //todo handle exceptions from CreateDirectory
            Directory.CreateDirectory(finalOutputDirPath);
          }

          destination = Path.Combine(finalOutputDirPath, hash + file.Extension);
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

      if (_options.Increment)
      {
        if (File.Exists(destination))
        {
          destination = IncrementFilePath(destination);
        }
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

    private string IncrementFilePath(string destination)
    {
      //todo omg this function has no error handling

      FileInfo fileInfo = new FileInfo(destination);

      string[] splitName = fileInfo.Name.Split('_');

      int val = 0;
      if (splitName.Length > 1)
      {
        val = int.Parse(splitName[1]) + 1;
      }
      return Path.Combine(fileInfo.Directory.FullName, splitName[0] + '_' + val);
    }
  }
}