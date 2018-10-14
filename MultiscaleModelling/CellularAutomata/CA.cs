using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MultiscaleModelling.CellularAutomata
{
    public class CA
    {
        public CA() { }
        
        public Scope Grow(Scope previousStructure, NeighbourhoodType neighbourhoodType)
        {
            var currentStructure = new Scope(previousStructure.Width, previousStructure.Height);
            StructureHelpers.AddBlackBorder(currentStructure);

            var isFull = true;
            List<Grain> neighbourhood = new List<Grain>();

            for (int i = 1; i < previousStructure.Width - 1; i++)
            {
                for (int j = 1; j < previousStructure.Height - 1; j++)
                {
                    if (previousStructure.StructureArray[i, j].Id != 0)
                    {
                        currentStructure.StructureArray[i, j] = previousStructure.StructureArray[i, j];
                    }

                    else if (previousStructure.StructureArray[i, j].Id == 0)
                    {
                        switch (neighbourhoodType)
                        {
                            case NeighbourhoodType.Moore:
                                neighbourhood = TakeMooreNeighbourhood(i, j, previousStructure.StructureArray);
                                break;
                            case NeighbourhoodType.Neumann:
                                neighbourhood = TakeNeumannNeighbourhood(i, j, previousStructure.StructureArray);
                                break;
                        }

                        var groups = neighbourhood.Where(g => g.Id != -1 && g.Id != 0).GroupBy(g => g.Id);
                        if (groups.Any())
                        {
                            var dictionary = new Dictionary<Grain, int>();
                            foreach (var group in groups)
                            {
                                var number = group.Key;
                                var total = group.Count();
                                dictionary.Add(group.FirstOrDefault(), total);
                            }
                            var orderedNeighbourhood = dictionary.OrderByDescending(x => x.Value);
                            currentStructure.StructureArray[i, j] = orderedNeighbourhood.First().Key;
                        }
                        else
                        {
                            currentStructure.StructureArray[i, j] = new Grain()
                            {
                                Id = 0,
                                Phase = 0,
                                Color = Color.White
                            };
                        }
                    }

                    if (currentStructure.StructureArray[i, j].Id == 0)
                        isFull = false;
                }
            }

            currentStructure.IsFull = isFull;

            StructureHelpers.UpdateBitmap(currentStructure);

            return currentStructure;
        }
        
        private List<Grain> TakeMooreNeighbourhood(int i, int j, Grain[,] structureArray)
        {
            var neighbourhood = new List<Grain>
            {
                structureArray[i - 1, j],
                structureArray[i + 1, j],
                structureArray[i, j - 1],
                structureArray[i, j + 1],
                structureArray[i - 1, j - 1],
                structureArray[i - 1, j + 1],
                structureArray[i + 1, j - 1],
                structureArray[i + 1, j + 1]
            };
            return neighbourhood;
        }

        private List<Grain> TakeNeumannNeighbourhood(int i, int j, Grain[,] structureArray)
        {
            var neighbourhood = new List<Grain>
            {
                structureArray[i - 1, j],
                structureArray[i + 1, j],
                structureArray[i, j - 1],
                structureArray[i, j + 1]
            };
            return neighbourhood;
        }
    }
}
