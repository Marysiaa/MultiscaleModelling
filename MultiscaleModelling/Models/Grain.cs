using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Models
{
    public class Grain
    {
        public int Id { get; set; }

        public Color Color { get; set; }

        public int Energy { get; set; } = -1;

        public bool IsRecrystalized { get; set; } = false;
    }
}
