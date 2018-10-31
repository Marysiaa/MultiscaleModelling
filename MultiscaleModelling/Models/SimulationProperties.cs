namespace MultiscaleModelling.Models
{
    public class SimulationProperties
    {
        public int ScopeWidth { get; set; }

        public int ScopeHeight { get; set; }

        public int NumberOfGrains { get; set; }

        public NeighbourhoodType NeighbourhoodType { get; set; }

        public InclusionsProperties Inclusions { get; set; }

        public int? GrowthProbability { get; set; }

        public StructureType StructureType { get; set; }

        public int? NumberOfRemainingGrains { get; set; }
    }

    public enum NeighbourhoodType { Neumann = 0, Moore = 1, ExtendedMoore = 2 }

    public enum SpecialIds { Empty = 0, Border = -1, Inclusion = -2, SecondPhase = -3 }

    public enum StructureType { Disabled = -1, Substructure = 0, Dualphase = 1 }
}
