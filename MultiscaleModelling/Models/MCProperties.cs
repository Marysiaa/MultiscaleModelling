using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Models
{
    public class MCProperties
    {
        public NeighbourhoodType Neighbourhood { get; set; }

        public int NumberOfInititalStates { get; set; }

        public  int NumberOfSteps { get; set; }
    }
}
