using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ImageHasher
{
  public class FileOutputHandler : IHashHandler
  {
    private readonly FileOutputOptions _options;
    private readonly FileStream _fileStream;
    private readonly StreamWriter _streamWriter;

    public FileOutputHandler(FileOutputOptions options)
    {
      _options = options;
      string output = HashUtils.GetOutputDirectory(options);
      Directory.CreateDirectory(output);
      output = Path.Combine(output, options.FileName);

      if (!options.Quiet)
      {
        Logger.Info("Output file path: " + output);
      }

      _fileStream = File.Create(output);
      _streamWriter = new StreamWriter(_fileStream);
    }

    public void Dispose()
    {
      _streamWriter.Dispose();
      _fileStream.Dispose();
    }

    public void RunHandler()
    {
      using (HashAlgorithm algorithm = HashAlgorithm.Create(_options.Algorithm))
      {
        if (HashUtils.IsDirectory(_options.Source))
        {
          DirectoryInfo info = new DirectoryInfo(_options.Source);
          //Run on the base directory
          RunOnDirectory(info, algorithm);

          //Run on all subdirectories
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
          //Run on single file
          FileInfo fileInfo = new FileInfo(_options.Source);
          if (fileInfo.Exists)
          {
            RunOnFile(fileInfo, algorithm);
          }
        }
      }
    }

    private void RunOnDirectory(DirectoryInfo directoryInfo, HashAlgorithm algorithm)
    {
      foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles()
        .Where(s => HashUtils.SupportedExtensions.Contains(s.Extension, StringComparer.OrdinalIgnoreCase)))
      {
        RunOnFile(fileInfo, algorithm);
      }
    }

    private void RunOnFile(FileInfo fileInfo, HashAlgorithm algorithm)
    {
      string hash = HashUtils.GetHashFromFile(fileInfo, algorithm);
      _streamWriter.WriteLine(fileInfo.FullName + _options.Separator + (_options.Lowercase ? hash.ToLower() : hash));
    }
  }
}