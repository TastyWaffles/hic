namespace ImageHasher
{
  public static class ImageHasher
  {
    public static void RunRename(RenameFilesOptions options)
    {
      using (IHashHandler handler = new RenameHandler(options))
      {
        handler.RunHandler();
      }
    }

    public static void RunFileOutput(FileOutputOptions options)
    {
      using (IHashHandler handler = new FileOutputHandler(options))
      {
        handler.RunHandler();
      }
    }
  }
}