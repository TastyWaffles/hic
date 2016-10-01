using System;
using System.Collections.Generic;
using CommandLine;

namespace ImageHasher
{
  ///
  /// Base Options
  public class BaseOptions
  {
    private string _algorithm;

    [Option('a', "algorithm", Default = "SHA1", HelpText = "Hashing algorithm, specify SHA1 or MD5")]
    public string Algorithm
    {
      get { return _algorithm; }
      set { _algorithm = "SHA1".Equals(value) || "MD5".Equals(value) ? value : "SHA1"; }
    }

    [Option('o', "output", HelpText = "Output directory")]
    public string OutputDir { get; set; }

    [Option('v', "verbose", HelpText = "Execute with verbose output")]
    public bool Verbose { get; set; }

    [Option('r', "recursive", HelpText = "Recursively look for files")]
    public bool Recursive { get; set; }

    [Option('s', "source", HelpText = "Source/Input directory/file")]
    public string Source { get; set; }

    [Option('l', "lowercase", HelpText = "Output hashes in lowercase")]
    public bool Lowercase { get; set; }

    [Option('x', "exclude", Separator = ',', HelpText =
         "Specify which extensions should be excluded. " +
         "By default the following are supported: jpg, png, bmp, exif, gif, tif. " +
         "You can exclude multiple extensions by separating values with ','. " +
         "For example '-x tif,exif,gif' will result in only jpg, png and bmp images being processed."
     )]
    public IEnumerable<string> ExcludedFileExtensions { get; set; }
  }

  ///
  /// RenameFilesOptions Options
  [Verb("rename", HelpText = "Rename files with hash")]
  public class RenameFilesOptions : BaseOptions
  {
    [Option('c', "copy", HelpText = "Copy the source file(s) and rename the duplicate(s)")]
    public bool Copy { get; set; }

    [Option('i', "increment",
       HelpText = "Append an increment (<hash>_i) to duplicate files, otherwise files will be overwritten")]
    public bool Increment { get; set; }

    [Option('p', "preserve",
       HelpText = "Preserve-sub directory structure when running recursively with output directory")]
    public bool PreserveSubDir { get; set; }

    [Option('d', "deleteEmpty", HelpText = "Delete any empty directories in source directory (if recursing)")]
    public bool DeleteEmptyDirs { get; set; }
  }


  ///
  /// FileOutputHandler Options
  [Verb("file", HelpText = "Output the hashes to file, each on a new line preceeded by the original file path and " +
                           "separated by a ',' character")]
  public class FileOutputOptions : BaseOptions
  {
    private string _filename;

    [Option('f', "file", HelpText = "Specify the name for the file")]
    public string FileName
    {
      get { return _filename ?? "Img_Hashes_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv"; }
      set { _filename = value; }
    }

    [Option('c', "separator", Default = ',', HelpText = "Character used to separate the original path from the hash")]
    public char Separator { get; set; }
  }
}