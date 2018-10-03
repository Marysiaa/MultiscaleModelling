using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Models
{
    public class SimulationProperties
    {
        public int ScopeWidth { get; set; }

        public int ScopeHeight { get; set; }

        public int NumberOfGrains { get; set; }

        public NeighbourhoodType NeighbourhoodType { get; set; }
    }

    public enum NeighbourhoodType { Neumann = 0, Moore = 1 }
}
