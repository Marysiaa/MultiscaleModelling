using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.MonteCarlo
{
    public class MC
    {
        private Random random { get; set; }
        private MCProperties properties { get; set; }
        private List<Grain> states { get; set; }
        public int ItertionsPerformed { get; set; }

        public MC(Random random, MCProperties properties)
        {
            this.random = random;
            this.properties = properties;

            states = PrepareAvaliableStates();
            ItertionsPerformed = 0;
        }

        public Scope Grow(Scope scope)
        {
            var remainingCellsForIteration = new List<KeyValuePair<Point, Grain>>();

            // create a list of grains which can change its state
            for (int i = 1; i < scope.Width - 1; i++)
            {
                for (int j = 1; j < scope.Height - 1; j++)
                {
                    if (CanGrainChangeState(i, j, scope.StructureArray))
                    {
                        remainingCellsForIteration.Add(new KeyValuePair<Point, Grain>(new Point(i, j), scope.StructureArray[i, j]));
                    }
                }
            }

            int cell;
            KeyValuePair<Point, Grain> actualState;
            Grain newState;
            while (remainingCellsForIteration.Count() > 0)
            {
                // randomly select cells form the list
                cell = GetRandomCell(0, remainingCellsForIteration.Count());
                actualState = remainingCellsForIteration.ElementAt(cell);

                // randomly selected new state
                newState = GetRandomState();

                // change cell state if it is acceptable
                if (IsStateChangeAcceptable(actualState.Key, scope.StructureArray, newState))
                {
                    scope.StructureArray[actualState.Key.X, actualState.Key.Y] = newState;
                }

                // remove handled cell form remaining cells list
                remainingCellsForIteration.Remove(actualState);
            }

            ItertionsPerformed++;
            StructureHelpers.UpdateBitmap(scope);
            return scope;
        }


        private int GetRandomCell(int min, int max)
        {
            return random.Next(min, max);
        }

        private Grain GetRandomState()
        {
            return states.ElementAt(random.Next(0, states.Count()));
        }

        private bool CanGrainChangeState(int i, int j, Grain[,] structureArray)
        {
            var neighbourhood = TakeMooreNeighbourhood(i, j, structureArray);
            var groups = neighbourhood.Where(g => (!StructureHelpers.IsIdSpecial(g.Id) && g.Id == structureArray[i, j].Id)).GroupBy(g => g.Id);
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

                if (orderedNeighbourhood.First().Value < 5)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
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

        private bool IsStateChangeAcceptable(Point point, Grain[,] structureArray, Grain newState)
        {
            var neighbourhood = TakeMooreNeighbourhood(point.X, point.Y, structureArray);
            int previousEnergy = neighbourhood.Where(g => (g.Id != 0 && g.Id != structureArray[point.X, point.Y].Id)).Count();
            int newEnergy = neighbourhood.Where(g => (g.Id != 0 && g.Id != newState.Id)).Count();
            if (newEnergy - previousEnergy <= 0)
            {
                return true;
            }
            return false;
        }

        private List<Grain> PrepareAvaliableStates()
        {
            var states = new List<Grain>();

            for (int i = 1; i <= properties.NumberOfInititalStates; i++)
            {
                states.Add(new Grain()
                {
                    Id = i,
                    Color = StructureHelpers.RandomColor(random)
                });
            }

            return states;
        }
    }
}
