using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

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
      //no-op
    }

    public void RunHandler()
    {
      using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
      {
        if (HasherUtils.IsDirectory(_options.Source))
        {
          DirectoryInfo info = new DirectoryInfo(_options.Source);
          RunOnDirectory(info, algorithm);

          if (_options.Recursive)
          {
            foreach (DirectoryInfo directoryInfo in info.EnumerateDirectories())
            {
              RunOnDirectory(directoryInfo, algorithm);
            }
          }
        }
        else
        {
          FileInfo fileInfo = new FileInfo(_options.Source);
          if (fileInfo.Exists)
          {
            RunOnFile(fileInfo, algorithm, CreateFinalDirectoryPath(fileInfo.Directory));
          }
        }
      }
    }

    private void RunOnDirectory(DirectoryInfo directoryInfo, HashAlgorithm algorithm)
    {
      string finalOutputDirPath = CreateFinalDirectoryPath(directoryInfo);

      foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles()
        .Where(s => HasherUtils.SupportedExtensions.Contains(s.Extension, StringComparer.OrdinalIgnoreCase)))
      {
        RunOnFile(fileInfo, algorithm, finalOutputDirPath);
      }
    }

    private void RunOnFile(FileInfo fileInfo, HashAlgorithm algorithm, string finalOutputDirPath)
    {
      string hash = HasherUtils.GetHashFromFile(fileInfo, algorithm);

      string destination = Path.Combine(finalOutputDirPath,
        (_options.Lowercase ? hash.ToLower() : hash) + fileInfo.Extension);

      ActionFile(fileInfo, destination);
    }

    private string CreateFinalDirectoryPath(DirectoryInfo dirInfo)
    {
      string finalOutputDirPath;

      if (_options.PreserveSubDir)
      {
        //make path relevant to source dir

        //split filepath on path separator
        string[] dirSplit = dirInfo.FullName.Split(Path.DirectorySeparatorChar);

        //find source parent dir name in filepath
        int index = Array.IndexOf(dirSplit, _rootParentDir);

        if (index < 0)
        {
          finalOutputDirPath = _outputDir;
        }
        else
        {
          //want to skip the current index,
          //but leave room at start for the outputDir in order to make building the path easier
          int len = 1 + dirSplit.Length - index;
          string[] tmp = new string[len];
          tmp[0] = _outputDir;
          Array.Copy(dirSplit, index + 1, tmp, 1, len);

          finalOutputDirPath = Path.Combine(tmp);

          if (!Directory.Exists(finalOutputDirPath))
          {
            //todo handle exceptions from CreateDirectory
            Directory.CreateDirectory(finalOutputDirPath);
          }
        }
      }
      else
      {
        finalOutputDirPath = HasherUtils.GetOutputDirectory(_options);
      }

      return finalOutputDirPath;
    }


    private void ActionFile(FileInfo fileInfo, string destination)
    {
      if (_options.Increment)
      {
        if (File.Exists(destination))
        {
          destination = IncrementFilePath(destination);
        }
      }

      if (_options.Copy)
      {
        fileInfo.CopyTo(destination);
      }
      else
      {
        fileInfo.MoveTo(destination);
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