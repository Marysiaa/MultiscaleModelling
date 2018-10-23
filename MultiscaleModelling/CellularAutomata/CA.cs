using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MultiscaleModelling.CellularAutomata
{
    public class CA
    {
        private Random random { get; set; }

        public CA(Random random)
        {
            this.random = random;
        }

        public Scope Grow(Scope previousStructure, NeighbourhoodType neighbourhoodType, int? growthProbability)
        {
            var currentStructure = new Scope(previousStructure.Width, previousStructure.Height);
            StructureHelpers.AddBlackBorder(currentStructure);

            var isFull = true;
            var isGrowthExtended = neighbourhoodType == NeighbourhoodType.ExtendedMoore ? true : false;
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
                        if (!isGrowthExtended)
                        {
                            switch (neighbourhoodType)
                            {
                                case NeighbourhoodType.Moore:
                                    neighbourhood = takeMooreNeighbourhood(i, j, previousStructure.StructureArray);
                                    break;
                                case NeighbourhoodType.Neumann:
                                    neighbourhood = takeNeumannNeighbourhood(i, j, previousStructure.StructureArray);
                                    break;
                            }

                            var groups = neighbourhood.Where(g => !StructureHelpers.IsIdSpecial(g.Id)).GroupBy(g => g.Id);
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
                        else
                        {
                            var dictionary = new Dictionary<Grain, int>();
                            var grainGrowth = false;
                            // rule 1
                            neighbourhood = takeMooreNeighbourhood(i, j, previousStructure.StructureArray);
                            var groups = neighbourhood.Where(g => !StructureHelpers.IsIdSpecial(g.Id)).GroupBy(g => g.Id);
                            if (groups.Any())
                            {
                                foreach (var group in groups)
                                {
                                    var number = group.Key;
                                    var total = group.Count();
                                    dictionary.Add(group.FirstOrDefault(), total);
                                }
                                var orderedNeighbourhood = dictionary.OrderByDescending(x => x.Value);
                                if (orderedNeighbourhood.First().Value >= 5)
                                {
                                    currentStructure.StructureArray[i, j] = orderedNeighbourhood.First().Key;
                                    grainGrowth = true;
                                }
                                else
                                {
                                    // rule 2
                                    neighbourhood = takeNearestMooreNeighbourhood(i, j, previousStructure.StructureArray);
                                    groups = neighbourhood.Where(g => !StructureHelpers.IsIdSpecial(g.Id)).GroupBy(g => g.Id);
                                    if (groups.Any())
                                    {
                                        orderedNeighbourhood = dictionary.OrderByDescending(x => x.Value);
                                        if (orderedNeighbourhood.First().Value >= 3)
                                        {
                                            currentStructure.StructureArray[i, j] = orderedNeighbourhood.First().Key;
                                            grainGrowth = true;
                                        }
                                    }
                                    if (!grainGrowth)
                                    {
                                        // rule 3
                                        neighbourhood = takeFurtherMooreNeighbourhood(i, j, previousStructure.StructureArray);
                                        groups = neighbourhood.Where(g => !StructureHelpers.IsIdSpecial(g.Id)).GroupBy(g => g.Id);
                                        if (groups.Any())
                                        {
                                            orderedNeighbourhood = dictionary.OrderByDescending(x => x.Value);
                                            if (orderedNeighbourhood.First().Value >= 3)
                                            {
                                                currentStructure.StructureArray[i, j] = orderedNeighbourhood.First().Key;
                                                grainGrowth = true;
                                            }
                                        }
                                    }
                                    if (!grainGrowth)
                                    {
                                        // rule 4 - Moore with probability
                                        neighbourhood = takeMooreNeighbourhood(i, j, previousStructure.StructureArray);
                                        groups = neighbourhood.Where(g => !StructureHelpers.IsIdSpecial(g.Id)).GroupBy(g => g.Id);
                                        var randomProbability = random.Next(0, 100);
                                        if (groups.Any() && (randomProbability <= growthProbability))
                                        {
                                            orderedNeighbourhood = dictionary.OrderByDescending(x => x.Value);
                                            currentStructure.StructureArray[i, j] = orderedNeighbourhood.First().Key;
                                            grainGrowth = true;
                                        }
                                    }
                                }
                            }
                            if (!grainGrowth)
                            {
                                // no grain yet
                                currentStructure.StructureArray[i, j] = new Grain()
                                {
                                    Id = 0,
                                    Phase = 0,
                                    Color = Color.White
                                };
                            }
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

        private List<Grain> takeMooreNeighbourhood(int i, int j, Grain[,] structureArray)
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

        private List<Grain> takeNeumannNeighbourhood(int i, int j, Grain[,] structureArray)
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

        private List<Grain> takeNearestMooreNeighbourhood(int i, int j, Grain[,] structureArray)
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

        private List<Grain> takeFurtherMooreNeighbourhood(int i, int j, Grain[,] structureArray)
        {
            var neighbourhood = new List<Grain>
            {
                structureArray[i - 1, j - 1],
                structureArray[i - 1, j + 1],
                structureArray[i + 1, j - 1],
                structureArray[i + 1, j + 1]
            };
            return neighbourhood;
        }

    }
}
