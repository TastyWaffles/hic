using System;
using System.IO;

namespace ImageHasher
{
  public interface IHashHandler : IDisposable
  {
    void HandleFile(FileInfo file, string hash);
  }
}