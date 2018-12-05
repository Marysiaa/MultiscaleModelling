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
        
        public NucleationDistribution NucleationDistribution { get; set; }

        public int Steps { get; set; }

        public int States { get; set; }

        public int Nucleons { get; set; }
    }

    public enum EnergyDistributionType { Disabled = -1, Heterogenous = 0, Homogenous = 1 }
   
    public enum NucleationDistribution { Disabled = -1, Beginning = 0, Constant = 1, Increasing = 2 }

}
