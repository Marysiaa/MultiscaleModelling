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
    public class SRX
    {
        private Random random;

        private SRXProperties properties { get; set; }
        private Scope scope { get; set; }

        private Color energy0 { get; set; }
        private Color energy1 { get; set; }
        private Color energy2 { get; set; }
        private Color energyOther { get; set; }

        private List<Grain> states { get; set; }
        public int ItertionsPerformed { get; set; }

        private List<int> nucleonsPreIteration;

        public SRX(Random random, SRXProperties SRXProperties, Scope scope, bool growth)
        {
            this.random = random;
            this.properties = SRXProperties;
            this.scope = scope;

            SetUpEnergyColors();

            if (growth)
            {
                ItertionsPerformed = 0;
                states = PrepareAvaliableStates();
                nucleonsPreIteration = CalculateNucleonsPerIteration();
            }
        }

        public Scope[] VisualizeEnergy()
        {
            return EnergyDistribution();
        }

        public Scope[] GrowthNucleons(Scope currentScope, Scope energyScope)
        {
            // NUCLEATION
            KeyValuePair<Point, Grain> actualState;
            Grain newState;
            var numberOfNucleons = nucleonsPreIteration[ItertionsPerformed];
            if (numberOfNucleons > 0)
            {
                var avaliablePlaces = new List<KeyValuePair<Point, Grain>>();
                switch (properties.EnergyDistributionType)
                {
                    case EnergyDistributionType.Homogenous:
                        // get higher energy from whole space
                        avaliablePlaces = GetHigherEnergyCells(currentScope);
                        break;
                    case EnergyDistributionType.Heterogenous:
                        // get remaining boundaries
                        avaliablePlaces = GetHighestEnergyCells(currentScope);
                        break;
                    default:
                        return null;
                }

                // select randomly form the list of remaining cells
                int selectedPoints = 0;
                // add new nucleons
                while (avaliablePlaces.Count() > 0 && selectedPoints < numberOfNucleons)
                {
                    // randomly select cells form the list
                    actualState = avaliablePlaces.ElementAt(random.Next(0, avaliablePlaces.Count()));
                    // random new state
                    newState = states.ElementAt(random.Next(0, states.Count()));
                    // change cell state to recrystalized
                    currentScope.StructureArray[actualState.Key.X, actualState.Key.Y] = newState;
                    energyScope.StructureArray[actualState.Key.X, actualState.Key.Y].Energy = newState.Energy;
                    energyScope.StructureArray[actualState.Key.X, actualState.Key.Y].Color = ChooseColor(newState.Energy);
                    // remove recrystalized cell form avaliable cells list
                    avaliablePlaces.Remove(actualState);
                    selectedPoints++;
                }
            }

            // GROWTH
            // create a list of grains which can recrystalize
            var remainingCellsForIteration = new List<KeyValuePair<Point, Grain>>();
            for (int i = 1; i < currentScope.Width - 1; i++)
            {
                for (int j = 1; j < currentScope.Height - 1; j++)
                {
                    //check if any neighbour is recrystalized
                    if (!currentScope.StructureArray[i, j].IsRecrystalized && HasAnyRecrystalizedNeighbour(i, j, currentScope.StructureArray))
                    {
                        remainingCellsForIteration.Add(new KeyValuePair<Point, Grain>(new Point(i, j), currentScope.StructureArray[i, j]));
                    }
                }
            }

            // go through all remaining cells randomly
            var neighbourhood = new List<Grain>();
            int beforeEnergy = 0, afterEnergy = 0;
            while (remainingCellsForIteration.Count() > 0)
            {
                // randomly select cell form the list
                actualState = remainingCellsForIteration.ElementAt(random.Next(0, remainingCellsForIteration.Count()));
                // get neighoburs (Moore)
                neighbourhood = StructureHelpers.TakeMooreNeighbourhood(actualState.Key.X, actualState.Key.Y, currentScope.StructureArray);
                // choose new recrystalized state (from neighoburs)
                newState = neighbourhood.Where(n => n.IsRecrystalized == true).First();
                // calculate before and after SRX energy (like in MC but including internal cell energy) 
                beforeEnergy = (neighbourhood.Where(g => (g.Id != 0 && g.Id != actualState.Value.Id)).Count())
                    + actualState.Value.Energy;
                afterEnergy = neighbourhood.Where(g => (g.Id != 0 && g.Id != newState.Id)).Count();
                // change cell state to recrystalized if it is acceptable (if after energy is lower)
                if (afterEnergy < beforeEnergy)
                {
                    currentScope.StructureArray[actualState.Key.X, actualState.Key.Y] = newState;
                    energyScope.StructureArray[actualState.Key.X, actualState.Key.Y].Energy = newState.Energy;
                    energyScope.StructureArray[actualState.Key.X, actualState.Key.Y].Color = ChooseColor(newState.Energy);
                }
                // remove handled cell form remaining cells list
                remainingCellsForIteration.Remove(actualState);
            }

            // check if all cells are recrystalized
            if (AreAllCellsRecrystalized(currentScope))
            {
                currentScope.IsFull = true;
            }

            StructureHelpers.UpdateBitmap(currentScope);
            StructureHelpers.UpdateBitmap(energyScope);

            ItertionsPerformed++;
            Scope[] scopes = new Scope[2] { currentScope, energyScope };
            return scopes;
        }

        private Scope[] EnergyDistribution()
        {
            var baseScope = scope;
            var energyScope = StructureHelpers.GenerateEmptyStructure(scope.Width, scope.Height);

            for (int i = 1; i < scope.Width - 1; i++)
            {
                for (int j = 1; j < scope.Height - 1; j++)
                {
                    if (scope.StructureArray[i, j].Id != Convert.ToInt32(SpecialIds.Border))
                    {
                        baseScope.StructureArray[i, j].Energy = properties.GrainEnergy;

                        energyScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Energy);
                        energyScope.StructureArray[i, j].Energy = properties.GrainEnergy;
                        energyScope.StructureArray[i, j].Color = ChooseColor(properties.GrainEnergy);
                    }
                    else
                    {
                        baseScope.StructureArray[i, j] = scope.StructureArray[i, j];
                        energyScope.StructureArray[i, j] = scope.StructureArray[i, j];
                    }
                }
            }
            if (properties.EnergyDistributionType == EnergyDistributionType.Heterogenous)
            {
                UpdateBoundariesEnergy(scope, energyScope);
                for (int i = 1; i < energyScope.Height - 1; i++)
                {
                    for (int j = 1; j < energyScope.Width - 1; j++)
                    {
                        baseScope.StructureArray[i, j].Energy = energyScope.StructureArray[i, j].Energy;
                    }
                }
            }

            energyScope.IsFull = true;
            StructureHelpers.UpdateBitmap(energyScope);

            Scope[] scopes = new Scope[2] { energyScope, baseScope };
            return scopes;
        }

        private Color ChooseColor(int energy)
        {
            if (energy == 0)
                return energy0;
            else if (energy <= 5)
                return energy1;
            else if (energy > 5)
                return energy2;
            return energyOther;
        }

        private void SetUpEnergyColors()
        {
            this.energy0 = Color.Blue;
            this.energy1 = Color.YellowGreen;
            this.energy2 = Color.Red;
            this.energyOther = Color.FloralWhite;
        }

        private void UpdateBoundariesEnergy(Scope baseScope, Scope energyScope)
        {
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
                        energyScope.StructureArray[i, j].Color = ChooseColor(properties.BoundaryEnergy.Value);
                        energyScope.StructureArray[i, j].Energy = properties.BoundaryEnergy.Value;
                    }
                    if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j + 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j + 1].Id))
                        || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j - 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j - 1].Id)))
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                    {
                        energyScope.StructureArray[i, j].Color = ChooseColor(properties.BoundaryEnergy.Value);
                        energyScope.StructureArray[i, j].Energy = properties.BoundaryEnergy.Value;
                    }
                }
            }
        }

        private List<Grain> PrepareAvaliableStates()
        {
            var rValues = new List<int>();
            int r;
            for (int i = 1; i <= properties.States; i++)
            {
                do
                {
                    r = random.Next(2, 256);
                }
                while (rValues.Contains(r));
                rValues.Add(r);
            }

            var states = new List<Grain>();

            for (int i = 1; i <= properties.States; i++)
            {
                states.Add(new Grain()
                {
                    Id = i + 100,
                    Color = Color.FromArgb(rValues[i - 1], 0, 0),
                    Energy = 0,
                    IsRecrystalized = true
                });
            }

            return states;
        }

        private List<int> CalculateNucleonsPerIteration()
        {
            var nucleonsPerIteration = new List<int>();
            switch (properties.NucleationDistribution)
            {
                case NucleationDistribution.Beginning:
                    nucleonsPerIteration.Add(properties.Nucleons);
                    for (int i = 1; i < properties.Steps; i++)
                    {
                        nucleonsPerIteration.Add(0);
                    }
                    return nucleonsPerIteration;
                case NucleationDistribution.Constant:
                    var number = properties.Nucleons / properties.Steps;
                    for (int i = 0; i < properties.Steps; i++)
                    {
                        nucleonsPerIteration.Add(number);
                    }
                    return nucleonsPerIteration;
                case NucleationDistribution.Increasing:
                    // to do
                    var temp = properties.Nucleons / properties.Steps;
                    for (int i = 0; i < properties.Steps; i++)
                    {
                        nucleonsPerIteration.Add(temp);
                    }
                    return nucleonsPerIteration;
                default:
                    return null;
            }
        }

        private List<KeyValuePair<Point, Grain>> GetHighestEnergyCells(Scope currentScope)
        {
            var cells = new List<KeyValuePair<Point, Grain>>();
            for (int i = 1; i < currentScope.Width - 1; i++)
            {
                for (int j = 1; j < currentScope.Height - 1; j++)
                {
                    if (currentScope.StructureArray[i, j].Energy >= properties.BoundaryEnergy)
                    {
                        cells.Add(new KeyValuePair<Point, Grain>(new Point(i, j), currentScope.StructureArray[i, j]));
                    }
                }
            }
            return cells;
        }

        private List<KeyValuePair<Point, Grain>> GetHigherEnergyCells(Scope currentScope)
        {
            var cells = new List<KeyValuePair<Point, Grain>>();
            for (int i = 1; i < currentScope.Width - 1; i++)
            {
                for (int j = 1; j < currentScope.Height - 1; j++)
                {
                    if (currentScope.StructureArray[i, j].Energy >= properties.GrainEnergy)
                    {
                        cells.Add(new KeyValuePair<Point, Grain>(new Point(i, j), currentScope.StructureArray[i, j]));
                    }
                }
            }
            return cells;
        }

        private bool AreAllCellsRecrystalized(Scope currentScope)
        {
            bool allRecrystalized = true;
            for (int i = 1; i < currentScope.Width - 1; i++)
            {
                for (int j = 1; j < currentScope.Height - 1; j++)
                {
                    if (!currentScope.StructureArray[i, j].IsRecrystalized)
                    {
                        allRecrystalized = false;
                    }
                }
            }
            return allRecrystalized;
        }

        private bool HasAnyRecrystalizedNeighbour(int i, int j, Grain[,] structureArray)
        {
            var neighbourhood = StructureHelpers.TakeMooreNeighbourhood(i, j, structureArray);
            var recrystalizedNeighbour = neighbourhood.Where(g => (g.IsRecrystalized == true)).FirstOrDefault();
            if (recrystalizedNeighbour != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
