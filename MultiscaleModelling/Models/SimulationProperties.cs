namespace MultiscaleModelling.Models
{
    public class SimulationProperties
    {
        public int ScopeWidth { get; set; }

        public int ScopeHeight { get; set; }

        public int NumberOfGrains { get; set; }

        public NeighbourhoodType NeighbourhoodType { get; set; }

        public InclusionsProperties Inclusions { get; set; }
    }

    public enum NeighbourhoodType { Neumann = 0, Moore = 1 }

    public enum SpecialIds { Empty = 0, Border = -1, Inclusion = -2 }
}
