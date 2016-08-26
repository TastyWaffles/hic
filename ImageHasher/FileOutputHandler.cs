using System.IO;

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
      string output = HasherUtils.GetOutputDirectory(options);
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

    public void HandleFile(FileInfo file, string hash)
    {
      _streamWriter.WriteLine(file.FullName + _options.Separator + hash);
    }
  }
}