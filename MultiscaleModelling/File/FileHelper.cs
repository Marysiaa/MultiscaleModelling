using System;
using System.IO;

namespace MultiscaleModelling.File
{
    public static class FileHelper
    {
        public static string DecideExtension(FileType type)
        {
            return type == FileType.Bitmap ? ".bmp" : ".txt";
        }

        public static string DecideFileName(FileType type)
        {
            return string.Concat("structure_", DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss"), DecideExtension(type));
        }

    }

    public enum FileType { Bitmap = 0, Txt = 1 }
}
