using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Models
{
    public class SRXProperties
    {
        public EnergyDistributionType EnergyDistributionType { get; set; }

        public int GrainEnergy { get; set; }

        public int? BoundaryEnergy { get; set; }

        public NucleationPosition NucleationPosition { get; set; }

        public NucleationAmount NucleationAmount { get; set; }
    }

    public enum EnergyDistributionType { Disabled = -1, Heterogenous = 0, Homogenous = 1 }

    public enum NucleationPosition { Disabled = -1, Anywhere = 0, BC = 1 }

    public enum NucleationAmount { Disabled = -1, Beginning = 0, Increasing = 1 }

}
