using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using System;
using System.Drawing;
using System.IO;

namespace MultiscaleModelling.File
{
    public static class FileReader
    {
        public static Scope ReadTxtFile(string pathString)
        {
            String input;
            try
            {
                input = System.IO.File.ReadAllText(pathString);

                string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                int width = Converters.StringToInt(lines[0]);
                int height = Converters.StringToInt(lines[1]);
                var scope = new Scope(width, height);

                for (int i = 0; i < width; i++)
                {
                    string[] grains = lines[i + 2].Split(' ');
                    for (int j = 0; j < height; j++)
                    {
                        string[] details = grains[j].Split('=');
                        scope.StructureArray[i, j] = new Grain()
                        {
                            Id = Converters.StringToInt(details[0]),
                            Phase = Converters.StringToInt(details[1]),
                            Color = Color.FromArgb(Converters.StringToInt(details[2]), Converters.StringToInt(details[3]), Converters.StringToInt(details[4]))
                        };
                    }
                }

                scope.IsFull = true;
                StructureHelpers.UpdateBitmap(scope);
                return scope;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static Scope ReadBitmapFile(string pathString)
        {
            Bitmap img = new Bitmap(pathString);
            int width = img.Width;
            int height = img.Height;

            var scope = new Scope(width, height);

            try
            {
                scope.StructureBitmap = new Bitmap(pathString);
            }
            catch (Exception)
            {
                return null;
            }

            scope.IsFull = true;
            StructureHelpers.UpdateArrayStructure(scope);
            return scope;
        }
    }
}
