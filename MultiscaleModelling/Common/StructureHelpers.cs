using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MultiscaleModelling.Common
{
    public static class StructureHelpers
    {
        public static void UpdateBitmap(Scope scope)
        {
            if (scope != null)
            {
                scope.StructureBitmap = new Bitmap(scope.Width, scope.Height);

                if (scope.StructureArray != null)
                {
                    for (int i = 0; i < scope.Width; i++)
                    {
                        for (int j = 0; j < scope.Height; j++)
                        {
                            scope.StructureBitmap.SetPixel(i, j, scope.StructureArray[i, j].Color);
                        }
                    }
                }
            }
        }

        public static void UpdateArrayStructure(Scope scope)
        {
            if (scope != null)
            {
                scope.StructureArray = new Grain[scope.Width, scope.Height];

                Dictionary<Color, int> grainIds = new Dictionary<Color, int>
                {
                    { Color.FromArgb(0, 0, 0), -1 },
                    { Color.FromArgb(1, 1, 1), 0 }
                };

                if (scope.StructureBitmap != null)
                {
                    for (int i = 0; i < scope.Width; i++)
                    {
                        for (int j = 0; j < scope.Height; j++)
                        {
                            var color = scope.StructureBitmap.GetPixel(i, j);
                            scope.StructureArray[i, j] = new Grain()
                            {
                                Color = color,
                                Id = chooseGrainId(grainIds, color),
                                // set phase if needed
                                Phase = 1
                            };
                        }
                    }
                }
            }
        }

        public static Scope InitStructure(SimulationProperties properties, Random random)
        {
            var scope = new Scope(properties.ScopeWidth, properties.ScopeHeight)
            {
                IsFull = false
            };

            AddBlackBorder(scope);

            for (int i = 1; i < scope.Width - 1; i++)
            {
                for (int j = 1; j < scope.Height - 1; j++)
                {
                    scope.StructureArray[i, j] = new Grain()
                    {
                        Id = 0,
                        Phase = 0,
                        Color = Color.White
                    };
                }
            }

            if (properties.Inclusions.AreEnable && (properties.Inclusions.CreationTime == InclusionsCreationTime.Beginning))
            {
                var inclusions = new Inclusions(properties.Inclusions, random);
                scope = inclusions.AddInclusionsAtTheBegining(scope);
            }

            for (int grainNumber = 0; grainNumber < properties.NumberOfGrains; grainNumber++)
            {
                Point coordinates;
                do
                {
                    coordinates = RandomCoordinates(scope.Width, scope.Height, random);
                }
                while (scope.StructureArray[coordinates.X, coordinates.Y].Id != 0);

                scope.StructureArray[coordinates.X, coordinates.Y].Color = RandomColor(random);
                scope.StructureArray[coordinates.X, coordinates.Y].Id = grainNumber + 1;

                scope.StructureArray[coordinates.X, coordinates.Y].Phase = 1;
            }

            return scope;
        }

        public static void AddBlackBorder(Scope scope)
        {
            for (int k = 0; k < scope.Width; k++)
            {
                scope.StructureArray[k, 0] = new Grain()
                {
                    Id = Convert.ToInt32(SpecialIds.Border),
                    Phase = -1,
                    Color = Color.Black
                };
                scope.StructureArray[k, scope.Height - 1] = new Grain()
                {
                    Id = Convert.ToInt32(SpecialIds.Border),
                    Phase = -1,
                    Color = Color.Black
                };
            }
            for (int l = 0; l < scope.Height; l++)
            {
                scope.StructureArray[0, l] = new Grain()
                {
                    Id = Convert.ToInt32(SpecialIds.Border),
                    Phase = -1,
                    Color = Color.Black
                };
                scope.StructureArray[scope.Width - 1, l] = new Grain()
                {
                    Id = Convert.ToInt32(SpecialIds.Border),
                    Phase = -1,
                    Color = Color.Black
                };
            }
        }

        public static Point RandomCoordinates(int width, int height, Random random)
        {
            return new Point(random.Next(1, width - 1), random.Next(1, height - 1));
        }

        public static Color RandomColor(Random random)
        {
            return Color.FromArgb(random.Next(2, 254), random.Next(2, 254), random.Next(2, 254));
        }

        public static bool IsIdSpecial(int id)
        {
            return Enum.IsDefined(typeof(SpecialIds), id);
        }

        private static int chooseGrainId(Dictionary<Color, int> grainIds, Color color)
        {
            int nextId = grainIds.Values.Max() + 1;

            if (grainIds.ContainsKey(color))
            {
                if (!grainIds.TryGetValue(color, out int id))
                {
                    grainIds[color] = nextId;
                    return nextId;
                }
                return id;
            }
            else
            {
                grainIds.Add(color, nextId);
                return nextId;
            }
        }
    }
}
