using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace ImageHasher
{
  public class ImageUtils
  {
    internal static readonly string[] SupportedExtensions = {".jpg", ".jpeg", ".png", ".bmp", ".exif", ".gif", ".tif"};

    internal static ImageFormat GetImageFormat(string extension)
    {
      switch (extension)
      {
        case ".jpg":
        case ".jpeg":
          return ImageFormat.Jpeg;
        case ".png":
          return ImageFormat.Png;
        case ".bmp":
          return ImageFormat.Bmp;
        case ".exif":
          return ImageFormat.Exif;
        case ".gif":
          return ImageFormat.Gif;
        case ".tif":
          return ImageFormat.Tiff;
        default:
          Console.WriteLine("Unable to find image format for extension: " + extension);
          return null;
      }
    }

    internal static string GetHashFromFile(FileInfo file, HashAlgorithm algorithm)
    {
      ImageFormat imageFormat = ImageUtils.GetImageFormat(file.Extension);
      if (imageFormat == null)
      {
        return null;
      }

      using (MemoryStream ms = new MemoryStream())
      {
        using (Bitmap img = new Bitmap(file.FullName))
        {
          img.Save(ms, imageFormat);
          return BitConverter.ToString(algorithm.ComputeHash(ms)).Replace("-", "");
        }
      }
    }
  }
}