using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageHasher
{
  public class RenameHandler : IHashHandler
  {
    /// <summary>
    /// The options given to the handler
    /// </summary>
    private readonly RenameFilesOptions _options;
    /// <summary>
    /// The root directory of the input files
    /// </summary>
    private readonly DirectoryInfo _rootParentDir;
    /// <summary>
    /// The root directory for the processed files
    /// </summary>
    private readonly DirectoryInfo _outputDir;
    /// <summary>
    /// The extensions supported by the handler
    /// </summary>
    private readonly IEnumerable<string> _supportedExtensions;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="options">Parsed command line arguments</param>
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

    /// <summary>
    /// Run the Rename Handler
    /// </summary>
    public void RunHandler()
    {
      if (HashUtils.IsDirectory(_options.Source))
      {
        DirectoryInfo info = new DirectoryInfo(_options.Source);

        using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
        {
          RunOnDirectory(info, algorithm);

          if (!_options.Recursive) return;

          foreach (DirectoryInfo directoryInfo in info.EnumerateDirectories("*", SearchOption.AllDirectories))
          {
            RunOnDirectory(directoryInfo, algorithm);
          }
        }
      }
      else
      {
        FileInfo fileInfo = new FileInfo(_options.Source);

        if (!fileInfo.Exists) return;

        using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
        {
          RunOnFile(fileInfo, algorithm, CreateFinalDirectoryPath(fileInfo.Directory));
        }
      }
    }

    /// <summary>
    /// Iterate over each directory (as required) and action each file.
    /// </summary>
    /// <param name="directoryInfo">Directory to action</param>
    /// <param name="algorithm">Algorithm to use</param>
    private void RunOnDirectory(DirectoryInfo directoryInfo, HashAlgorithm algorithm)
    {
      Logger.Info("Processing dir: " + directoryInfo.FullName);

      string finalOutputDirPath = CreateFinalDirectoryPath(directoryInfo);

      foreach (
        FileInfo fileInfo in
        directoryInfo.EnumerateFiles()
          .Where(s => _supportedExtensions.Contains(s.Extension, StringComparer.OrdinalIgnoreCase)))
      {
        RunOnFile(fileInfo, algorithm, finalOutputDirPath);
      }

      if (_options.DeleteEmptyDirs && !directoryInfo.EnumerateFileSystemInfos().Any())
      {
        directoryInfo.Delete();
      }
    }

    /// <summary>
    /// Run the hash algorithm on the file and apply all required actions to said file.
    /// </summary>
    /// <param name="fileInfo">File to action</param>
    /// <param name="algorithm">Algorithm to use</param>
    /// <param name="finalOutputDirPath">The final output directory for the file</param>
    private void RunOnFile(FileInfo fileInfo, HashAlgorithm algorithm, string finalOutputDirPath)
    {
      string hash = HashUtils.GetHashFromFile(fileInfo, algorithm);

      string destination = Path.Combine(finalOutputDirPath,
        (_options.Lowercase ? hash.ToLower() : hash) + fileInfo.Extension);

      ActionFile(fileInfo, destination);
    }

    /// <summary>
    /// Returns the output directory based on the given input directory.
    /// Handles whether to preserve sub-directory structure.
    /// Creates the output directory if it doesn't exist.
    /// </summary>
    /// <param name="dirInfo">The input directory</param>
    /// <returns>The output directory</returns>
    private string CreateFinalDirectoryPath(DirectoryInfo dirInfo)
    {
      string finalOutputDirPath;

      if (_options.PreserveSubDir)
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
            Logger.Info("Created directory: " + finalOutputDirPath);
          }
        }
      }
      else
      {
        finalOutputDirPath = HashUtils.GetOutputDirectory(_options);
      }
      return finalOutputDirPath;
    }

    /// <summary>
    /// Actions a file based on the program args, handles clashes by copying/moving and incrementing/overwriting.
    /// </summary>
    /// <param name="fileInfo">The file to be actioned</param>
    /// <param name="destination">The destination file path</param>
    private void ActionFile(FileInfo fileInfo, string destination)
    {
      //Already there
      if (fileInfo.FullName.Equals(destination)) return;

      if (File.Exists(destination))
      {
        if (_options.Increment)
        {
          destination = IncrementFilePath(destination);
        }
        else
        {
          File.Delete(destination);
        }
      }

      try
      {
        if (_options.Copy)
        {
          fileInfo.CopyTo(destination);
        }
        else
        {
          fileInfo.MoveTo(destination);
        }
      }
      catch (IOException e)
      {
        Logger.Info(e.Message);
      }
    }

    /// <summary>
    /// Increments a suffix on the end of a file.
    /// The suffix is the counter in the format: filename_counter.extension
    /// </summary>
    /// <param name="destination">The intended file path</param>
    /// <returns>The resultant file path</returns>
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