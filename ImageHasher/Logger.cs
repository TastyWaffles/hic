using System;

namespace ImageHasher
{
  public class Logger
  {
    private static Logger instance = new Logger();

    //defaults to false
    private bool verbose;

    internal static void InitialiseLogger(BaseOptions options)
    {
      instance.verbose = options.Verbose;
    }

    internal static void Info(string s)
    {
      if (instance.verbose)
      {
        Console.WriteLine(s);
      }
    }
  }
}