using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Common
{
    public class GrainBoundaries
    {
        private Random random { get; set; }
        private Scope baseScope { get; set; }
        private GrainBoundariesProperties grainBoundariesProperties { get; set; }
        private Color boundaryColor { get; set; }

        public GrainBoundaries(Random random, Scope baseScope, GrainBoundariesProperties grainBoundariesProperties)
        {
            this.random = random;
            this.baseScope = baseScope;
            this.grainBoundariesProperties = grainBoundariesProperties;
            this.boundaryColor = Color.Black;
        }

        public Scope SelectGrainBoundaries()
        {
            switch (grainBoundariesProperties.GrainsBoundariesOption)
            {
                case GrainsBoundariesOption.AllGrains:
                    return SelectAllGrainsBoundaries();
                case GrainsBoundariesOption.NGrains:
                    return SelectNGrainsBoundaries();
                default:
                    return null;
            }
        }

        public Scope ClearBackground()
        {
            for (int i = 1; i < baseScope.Width - 1; i++)
            {
                for (int j = 1; j < baseScope.Height - 1; j++)
                {
                    if (baseScope.StructureArray[i, j].Id != Convert.ToInt32(SpecialIds.Boundaries))
                    {
                        baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Empty);
                        baseScope.StructureArray[i, j].Color = Color.White;
                    }
                }
            }

            baseScope.IsFull = true;
            StructureHelpers.UpdateBitmap(baseScope);

            return baseScope;
        }

        private Scope SelectAllGrainsBoundaries()
        {
            var grainBoungarySize = grainBoundariesProperties.GrainBoundarySize;
            for (int i = 2; i < baseScope.Width - 2; i++)
            {
                for (int j = 2; j < baseScope.Height - 2; j++)
                {
                    if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i + 1, j].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i + 1, j].Id))
                        || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i - 1, j].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i - 1, j].Id)))
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                    {
                        if (grainBoungarySize == 1)
                        {
                            baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                            baseScope.StructureArray[i, j].Color = boundaryColor;
                        }
                        else
                        {
                            int halfSize = (grainBoungarySize - 1) / 2;
                            if (halfSize == 0)
                            {
                                baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                baseScope.StructureArray[i, j].Color = boundaryColor;
                            }
                            else
                            {
                                for (int w = i - halfSize; w < i + halfSize; w++)
                                {
                                    if (w > 0 && w < baseScope.Width - 1)
                                    {
                                        baseScope.StructureArray[w, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                        baseScope.StructureArray[w, j].Color = boundaryColor;
                                    }
                                }
                            }
                        }
                    }
                    if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j + 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j + 1].Id))
                        || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j - 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j - 1].Id)))
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                    {
                        if (grainBoungarySize == 1)
                        {
                            baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                            baseScope.StructureArray[i, j].Color = boundaryColor;
                        }
                        else
                        {
                            int halfSize = (grainBoungarySize - 1) / 2;
                            if (halfSize == 0)
                            {
                                baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                baseScope.StructureArray[i, j].Color = boundaryColor;
                            }
                            else
                            {
                                for (int h = j - halfSize; h < j + halfSize; h++)
                                {
                                    if (h > 0 && h < baseScope.Height - 1)
                                    {
                                        baseScope.StructureArray[i, h].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                        baseScope.StructureArray[i, h].Color = boundaryColor;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            baseScope.IsFull = true;
            StructureHelpers.UpdateBitmap(baseScope);

            return baseScope;
        }

        private IList<int> GetNRandomGrainIds()
        {
            var grainsIds = new List<int>();
            Point actRandCoord = new Point(0, 0);
            List<int> prevRandIds = new List<int>();
            for (int i = 0; i < grainBoundariesProperties.NumberOfGrainsToSelectBoundaries; i++)
            {
                prevRandIds.Add(baseScope.StructureArray[actRandCoord.X, actRandCoord.Y].Id);
                do
                {
                    actRandCoord = StructureHelpers.RandomCoordinates(baseScope.Width, baseScope.Height, random);
                }
                while (prevRandIds.IndexOf(baseScope.StructureArray[actRandCoord.X, actRandCoord.Y].Id) != -1);
                grainsIds.Add(baseScope.StructureArray[actRandCoord.X, actRandCoord.Y].Id);
            }
            return grainsIds;
        }

        private Scope SelectNGrainsBoundaries()
        {
            var nGrainsIds = GetNRandomGrainIds();

            var grainBoungarySize = grainBoundariesProperties.GrainBoundarySize;
            for (int i = 2; i < baseScope.Width - 2; i++)
            {
                for (int j = 2; j < baseScope.Height - 2; j++)
                {
                    if (nGrainsIds.IndexOf(baseScope.StructureArray[i, j].Id) != -1)
                    {
                        if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i + 1, j].Id
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i + 1, j].Id))
                            || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i - 1, j].Id
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i - 1, j].Id)))
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                        {
                            if (grainBoungarySize == 1)
                            {
                                baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                baseScope.StructureArray[i, j].Color = boundaryColor;
                            }
                            else
                            {
                                int halfSize = (grainBoungarySize - 1) / 2;
                                if (halfSize == 0)
                                {
                                    baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                    baseScope.StructureArray[i, j].Color = boundaryColor;
                                }
                                else
                                {
                                    for (int w = i - halfSize; w < i + halfSize; w++)
                                    {
                                        if (w > 0 && w < baseScope.Width - 1)
                                        {
                                            baseScope.StructureArray[w, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                            baseScope.StructureArray[w, j].Color = boundaryColor;
                                        }
                                    }
                                }
                            }
                        }
                        if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j + 1].Id
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j + 1].Id))
                            || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j - 1].Id
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j - 1].Id)))
                            && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                        {
                            if (grainBoungarySize == 1)
                            {
                                baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                baseScope.StructureArray[i, j].Color = boundaryColor;
                            }
                            else
                            {
                                int halfSize = (grainBoungarySize - 1) / 2;
                                if (halfSize == 0)
                                {
                                    baseScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                    baseScope.StructureArray[i, j].Color = boundaryColor;
                                }
                                else
                                {
                                    for (int h = j - halfSize; h < j + halfSize; h++)
                                    {
                                        if (h > 0 && h < baseScope.Height - 1)
                                        {
                                            baseScope.StructureArray[i, h].Id = Convert.ToInt32(SpecialIds.Boundaries);
                                            baseScope.StructureArray[i, h].Color = boundaryColor;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            baseScope.IsFull = true;
            StructureHelpers.UpdateBitmap(baseScope);

            return baseScope;
        }
    }
}
