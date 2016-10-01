namespace ImageHasher
{
  public static class ImageHasher
  {
    public static void RunRename(RenameFilesOptions options)
    {
      Logger.InitialiseLogger(options);
      using (IHashHandler handler = new RenameHandler(options))
      {
        handler.RunHandler();
      }
    }

    public static void RunFileOutput(FileOutputOptions options)
    {
      Logger.InitialiseLogger(options);
      using (IHashHandler handler = new FileOutputHandler(options))
      {
        handler.RunHandler();
      }
    }
  }
}