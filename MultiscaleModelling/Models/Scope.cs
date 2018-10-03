using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace MultiscaleModelling.Models
{
    public class Scope
    {
        public Scope(int width, int height)
        {
            StructureArray = new Grain[width, height];
            StructureBitmap = new Bitmap(width, height);
            Width = width;
            Height = height;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public Grain[,] StructureArray { get; set; }

        public Bitmap StructureBitmap { get; set; } 

        public bool IsFull { get; set; }
    }
}
