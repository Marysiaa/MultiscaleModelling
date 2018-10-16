using MultiscaleModelling.Models;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace MultiscaleModelling.File
{
    public static class FileSaver
    {
        public static string SaveTxtFile(Scope scopeToSave, string pathString)
        {
            StringBuilder content = new StringBuilder();
            content.Append(string.Concat(scopeToSave.Width, Environment.NewLine, scopeToSave.Height, Environment.NewLine));

            for (int x = 0; x < scopeToSave.Width; x++)
            {
                for (int y = 0; y < scopeToSave.Height; y++)
                {
                    content.Append(scopeToSave.StructureArray[x, y].Id).Append("=");
                    content.Append(scopeToSave.StructureArray[x, y].Phase).Append("=");
                    content.Append(scopeToSave.StructureArray[x, y].Color.R).Append("=");
                    content.Append(scopeToSave.StructureArray[x, y].Color.G).Append("=");
                    content.Append(scopeToSave.StructureArray[x, y].Color.B).Append(" ");
                }
                content.Append(Environment.NewLine);
            }

            try
            {
                System.IO.File.WriteAllText(pathString, content.ToString());
            }
            catch (IOException e)
            {
                return e.ToString();
            }

            return Path.GetFileName(pathString);
        }

        public static string SaveBitmapFile(Scope scopeToSave, string pathString)
        {
            try
            {
                scopeToSave.StructureBitmap.Save(pathString, ImageFormat.Bmp);
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return Path.GetFileName(pathString);
        }
    }
}
