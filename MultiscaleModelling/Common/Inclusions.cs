using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MultiscaleModelling.Common
{
    public class Inclusions
    {
        private InclusionsProperties inclusionsProperties;

        private Random random;

        public Inclusions(InclusionsProperties inclusionsProperties, Random random)
        {
            this.inclusionsProperties = inclusionsProperties;
            this.random = random;
        }

        public Scope AddInclusionsAtTheBegining(Scope scope)
        {
            for (int inclusionNumber = 0; inclusionNumber < inclusionsProperties.Amount; inclusionNumber++)
            {
                Point coordinates;
                do
                {
                    coordinates = StructureHelpers.RandomCoordinates(scope.Width, scope.Height, random);
                }
                while (scope.StructureArray[coordinates.X, coordinates.Y].Id != 0);

                switch (inclusionsProperties.InclusionsType)
                {
                    case InclusionsType.Square:
                        AddSquareInclusion(scope, coordinates);
                        break;
                    case InclusionsType.Circular:
                        AddCirularInclusion(scope, coordinates);
                        break;
                }
            }

            return scope;
        }

        public Scope AddInclusionsAfterGrainGrowth(Scope scope)
        {
            for (int inclusionNumber = 0; inclusionNumber < inclusionsProperties.Amount; inclusionNumber++)
            {
                Point coordinates;
                do
                {
                    coordinates = StructureHelpers.RandomCoordinates(scope.Width, scope.Height, random);
                }
                while (!IsCoordinateOnGrainBoundaries(scope, coordinates));

                switch (inclusionsProperties.InclusionsType)
                {
                    case InclusionsType.Square:
                        AddSquareInclusion(scope, coordinates);
                        break;
                    case InclusionsType.Circular:
                        AddCirularInclusion(scope, coordinates);
                        break;
                }
            }

            return scope;
        }

        private void AddSquareInclusion(Scope scope, Point coordinates)
        {
            int a = (int)(inclusionsProperties.Size / Math.Sqrt(2));
            int halfA = (a / 2);
            for (int x = coordinates.X - halfA; (x <= coordinates.X + halfA && x < scope.Width && x > 0); x++)
            {
                for (int y = coordinates.Y - halfA; (y <= coordinates.Y + halfA && y < scope.Height && y > 0); y++)
                {
                    if (!StructureHelpers.IsIdSpecial(scope.StructureArray[x, y].Id) || scope.StructureArray[x, y].Id == 0)
                    {
                        scope.StructureArray[x, y].Color = Color.Black;
                        scope.StructureArray[x, y].Id = Convert.ToInt32(SpecialIds.Inclusion);
                    }
                }
            }
        }

        private void AddCirularInclusion(Scope scope, Point coordinates)
        {
            var pointsInside = GetPointsInsideCircle(inclusionsProperties.Size, coordinates);
            foreach(var point in pointsInside)
            {
                if(point.X < scope.Width && point.X > 0 && point.Y < scope.Height && point.Y > 0)
                {
                    if (!StructureHelpers.IsIdSpecial(scope.StructureArray[point.X, point.Y].Id) || scope.StructureArray[point.X, point.Y].Id == 0)
                    {
                        scope.StructureArray[point.X, point.Y].Color = Color.Black;
                        scope.StructureArray[point.X, point.Y].Id = Convert.ToInt32(SpecialIds.Inclusion);
                    }
                }
            }
        }

        private IList<Point> GetPointsInsideCircle(int radius, Point center)
        {
            List<Point> pointsInside = new List<Point>();

            for (int x = center.X - radius; x < center.X + radius; x++)
            {
                for (int y = center.Y - radius; y < center.Y + radius; y++)
                {
                    if ((x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y) <= radius * radius)
                    {
                        pointsInside.Add(new Point(x, y));
                    }
                }
            }
            return pointsInside;
        }

        private bool IsCoordinateOnGrainBoundaries(Scope scope, Point coordinates)
        {
            var centerId = scope.StructureArray[coordinates.X, coordinates.Y].Id;
            var neighboursIds = new List<int>
            {
                scope.StructureArray[coordinates.X - 1, coordinates.Y].Id,
                scope.StructureArray[coordinates.X + 1, coordinates.Y].Id,
                scope.StructureArray[coordinates.X, coordinates.Y - 1].Id,
                scope.StructureArray[coordinates.X, coordinates.Y + 1].Id,
                scope.StructureArray[coordinates.X - 1, coordinates.Y - 1].Id,
                scope.StructureArray[coordinates.X - 1, coordinates.Y + 1].Id,
                scope.StructureArray[coordinates.X + 1, coordinates.Y - 1].Id,
                scope.StructureArray[coordinates.X + 1, coordinates.Y + 1].Id
            };

            foreach(var neighbourId in neighboursIds)
            {
                if(centerId != neighbourId && !StructureHelpers.IsIdSpecial(neighbourId))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
