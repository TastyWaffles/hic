using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageHasher
{
  public class RenameHandler : IHashHandler
  {
    private readonly RenameFilesOptions _options;
    private readonly DirectoryInfo _rootParentDir;
    private readonly DirectoryInfo _outputDir;
    private readonly IEnumerable<string> _supportedExtensions;

    public RenameHandler(RenameFilesOptions options)
    {
      _options = options;

      string source = HashUtils.IsDirectory(_options.Source) ? _options.Source : Directory.GetCurrentDirectory();
      _rootParentDir = new DirectoryInfo(source);

      _outputDir = new DirectoryInfo(HashUtils.GetOutputDirectory(_options));
      if (!_outputDir.Exists)
      {
        _outputDir.Create();
      }

      _supportedExtensions = HashUtils.SupportedExtensions; //.Intersect(_options.ExcludedFileExtensions); todo
    }


    public void Dispose()
    {
      //no-op
    }

    public void RunHandler()
    {
      if (HashUtils.IsDirectory(_options.Source))
      {
        DirectoryInfo info = new DirectoryInfo(_options.Source);

        using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
        {
          RunOnDirectory(info, algorithm);

          if (_options.Recursive)
          {
            foreach (DirectoryInfo directoryInfo in info.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
              RunOnDirectory(directoryInfo, algorithm);
            }
          }
        }

//        if (_options.DeleteEmptyDirs)
//        {
//          HashUtils.DeleteAllEmptyDirectories(info);
//        }
      }
      else
      {
        FileInfo fileInfo = new FileInfo(_options.Source);
        if (fileInfo.Exists)
        {
          using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
          {
            RunOnFile(fileInfo, algorithm, CreateFinalDirectoryPath(fileInfo.Directory));
          }
        }
      }
    }

    private
      void RunOnDirectory(DirectoryInfo directoryInfo, HashAlgorithm algorithm)
    {
      string finalOutputDirPath = CreateFinalDirectoryPath(directoryInfo);

      foreach (
        FileInfo fileInfo
        in
        directoryInfo.EnumerateFiles
          ()
          .
          Where(s =>
            _supportedExtensions.Contains
            (
              s.Extension
              ,
              StringComparer.OrdinalIgnoreCase
            )))
      {
        RunOnFile(fileInfo, algorithm, finalOutputDirPath);
      }

      if (_options.DeleteEmptyDirs && !directoryInfo.EnumerateFileSystemInfos().Any())
      {
        directoryInfo.Delete();
      }
    }

    private
      void RunOnFile(FileInfo fileInfo, HashAlgorithm algorithm, string finalOutputDirPath)
    {
      string hash = HashUtils.GetHashFromFile(fileInfo, algorithm);

      string destination = Path.Combine(finalOutputDirPath,
        (_options.Lowercase ? hash.ToLower() : hash) + fileInfo.Extension);

      ActionFile(fileInfo, destination);
    }

    private
      string CreateFinalDirectoryPath(DirectoryInfo dirInfo)
    {
      string finalOutputDirPath;

      if (
        _options.PreserveSubDir
      )
      {
        //make path relevant to source dir

        //split filepath on path separator
        string[] dirSplit = dirInfo.FullName.Split(Path.DirectorySeparatorChar);

        //find source parent dir name in filepath
        int index = Array.IndexOf(dirSplit, _rootParentDir.Name);

        if (index < 0 || index == dirSplit.Length - 1)
        {
          finalOutputDirPath = _outputDir.FullName;
        }
        else
        {
          //want to skip the current index,
          //but leave room at start for the outputDir in order to make building the path easier
          int len = dirSplit.Length - index - 1;
          string[] tmp = new string[len + 1];
          tmp[0] = _outputDir.FullName;
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
        finalOutputDirPath = HashUtils.GetOutputDirectory(_options);
      }
      return finalOutputDirPath;
    }

    private void ActionFile(FileInfo fileInfo, string destination)
    {
      if (!fileInfo.FullName.Equals(destination))
      {
        if (_options.Increment)
        {
          if (File.Exists(destination))
          {
            destination = IncrementFilePath(destination);
          }
        }
        else
        {
          if (File.Exists(destination))
          {
            File.Delete(destination);
          }
        }

        if (_options.Copy)
        {
          try
          {
            fileInfo.CopyTo(destination);
          }
          catch (IOException)
          {
          }
        }
        else
        {
          try
          {
            fileInfo.MoveTo(destination);
          }
          catch (IOException)
          {
          }
        }
      }
    }

    private string IncrementFilePath(string destination)
    {
//todo omg this function has no error handling

      FileInfo fileInfo = new FileInfo(destination);

      string[] splitName = fileInfo.Name.Split('_', '.');

      int val = 0;
      if (splitName.Length > 2)
      {
        val = int.Parse(splitName[1]) + 1;
      }
      return Path.Combine(fileInfo.Directory.FullName, splitName[0] + '_' + val + fileInfo.Extension);
    }
  }
}