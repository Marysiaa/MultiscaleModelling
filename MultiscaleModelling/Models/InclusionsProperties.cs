namespace MultiscaleModelling.Models
{
    public class InclusionsProperties
    {
        public bool AreEnable { get; set; }

        public int Amount { get; set; }

        public int Size { get; set; }

        public InclusionsType InclusionsType { get; set; }

        public InclusionsCreationTime CreationTime { get; set; }
    }

    public enum InclusionsCreationTime { Beginning = 0, After = 1 }

    public enum InclusionsType { Square = 0, Circular = 1 }
}
 