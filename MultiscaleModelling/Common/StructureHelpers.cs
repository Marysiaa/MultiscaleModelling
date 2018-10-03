using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.Common
{
    public static class StructureHelpers
    {
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
            
            for (int grainNumber = 0; grainNumber < properties.NumberOfGrains; grainNumber++)
            {
                var coordinates = RandomCoordinates(properties, random);
                scope.StructureArray[coordinates.X, coordinates.Y].Color = RandomColor(random);
                scope.StructureArray[coordinates.X, coordinates.Y].Id = grainNumber + 1;

                // select phase if needed
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
                    Id = -1,
                    Phase = -1,
                    Color = Color.Black
                };
                scope.StructureArray[k, scope.Height - 1] = new Grain()
                {
                    Id = -1,
                    Phase = -1,
                    Color = Color.Black
                };
            }
            for (int l = 0; l < scope.Height; l++)
            {
                scope.StructureArray[0, l] = new Grain()
                {
                    Id = -1,
                    Phase = -1,
                    Color = Color.Black
                };
                scope.StructureArray[scope.Width - 1, l] = new Grain()
                {
                    Id = -1,
                    Phase = -1,
                    Color = Color.Black
                };
            }
        }

        public static Point RandomCoordinates(SimulationProperties properties, Random random)
        {
            return new Point(random.Next(1, properties.ScopeWidth - 1), random.Next(1, properties.ScopeHeight - 1));
        }

        public static Color RandomColor(Random random)
        {
            return Color.FromArgb(random.Next(2, 254), random.Next(2, 254), random.Next(2, 254));
        }
    }
}
