using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Models
{
    public class GrainBoundariesProperties
    {
        public GrainsBoundariesOption GrainsBoundariesOption { get; set; }

        public int? NumberOfGrainsToSelectBoundaries { get; set; }

        public int GrainBoundarySize { get; set; }
    }

    public enum GrainsBoundariesOption { Disabled = -1, AllGrains = 0, NGrains = 1 }
}
