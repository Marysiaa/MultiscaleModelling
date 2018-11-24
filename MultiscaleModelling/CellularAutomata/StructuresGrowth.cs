using MultiscaleModelling.Common;
using MultiscaleModelling.Models;
using MultiscaleModelling.MonteCarlo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiscaleModelling.CellularAutomata
{
    public class StructuresGrowth
    {
        private Random random { get; set; }
        private Scope baseScope { get; set; }
        private List<int> remainingIds { get; set; }
        private CA CA { get; set; }
        private MC MC { get; set; }
        private MCProperties MCProperties { get; set; }

        public StructuresGrowth(Random random, Scope baseScope, MCProperties MCProperties)
        {
            this.random = random;
            this.baseScope = baseScope;
            this.CA = new CA(random);
            this.MC = new MC(random, MCProperties);
            this.MCProperties = MCProperties;
        }

        public Scope ChangeStructure(SimulationProperties properties)
        {
            var grainsPoints = new List<Point>();
            Point actRandCoord = new Point(0, 0);
            List<int> prevRandIds = new List<int>();
            for (int i = 0; i < properties.NumberOfRemainingGrains.Value; i++)
            {
                prevRandIds.Add(baseScope.StructureArray[actRandCoord.X, actRandCoord.Y].Id);
                do
                {
                    actRandCoord = StructureHelpers.RandomCoordinates(baseScope.Width, baseScope.Height, random);
                }
                while (prevRandIds.IndexOf(baseScope.StructureArray[actRandCoord.X, actRandCoord.Y].Id) != -1);
                grainsPoints.Add(actRandCoord);
            }
            var remainingGrains = GetRemainingGrains(grainsPoints);

            // class 5: CA -> CA
            // class 9: CA -> MC, MC -> MC, MC -> CA 
            switch (properties.StructureType)
            {
                case StructureType.Dualphase:
                    // dual phase - remaining grains should have id and color changed to the same one
                    ChangeGrainsIntoSecondPhase(remainingGrains);
                    return GeterateDualphaseStructure(remainingGrains, properties);
                case StructureType.Substructure:
                    // substructure - remaining grains shoudld have the same id and color as in base structure
                    return GenerateSubstructure(remainingGrains, properties);
                default:
                    return null;
            }
        }

        private Dictionary<Point, Grain> GetWholeGrain(Point grainPoint)
        {
            var grainPoints = new Dictionary<Point, Grain>();
            var grain = baseScope.StructureArray[grainPoint.X, grainPoint.Y];

            for (int i = 1; i < baseScope.Width - 1; i++)
            {
                for (int j = 1; j < baseScope.Height - 1; j++)
                {
                    if (baseScope.StructureArray[i, j].Id == grain.Id)
                    {
                        grainPoints.Add(new Point(i, j), grain);
                    }
                }
            }

            return grainPoints;
        }

        private List<Dictionary<Point, Grain>> GetRemainingGrains(List<Point> grainPoints)
        {
            var remainingGrains = new List<Dictionary<Point, Grain>>();
            foreach (var point in grainPoints)
            {
                remainingGrains.Add(GetWholeGrain(point));
            }
            return remainingGrains;
        }

        private void ChangeGrainsIntoSecondPhase(List<Dictionary<Point, Grain>> grains)
        {
            foreach (var grain in grains)
            {
                foreach (var g in grain)
                {
                    g.Value.Id = Convert.ToInt32(SpecialIds.SecondPhase);
                    g.Value.Color = Color.HotPink;
                }
            }
        }

        private Scope GenerateSubstructure(List<Dictionary<Point, Grain>> remainingGrains, SimulationProperties properties)
        {
            // prepare base scope with remaining grains
            remainingIds = new List<int>();
            var scope = new Scope(baseScope.Width, baseScope.Height);
            foreach (var grain in remainingGrains)
            {
                foreach (var g in grain)
                {
                    scope.StructureArray[g.Key.X, g.Key.Y] = g.Value;
                    if (remainingIds.IndexOf(g.Value.Id) == -1)
                    {
                        remainingIds.Add(g.Value.Id);
                    }
                }
            }

            Scope prevScope = StructureHelpers.InitCAStructure(properties, random, scope, remainingIds);
            var newScope = new Scope(prevScope.Width, prevScope.Height);

            //if (properties.AdvancedMethodType == AdvancedMethodType.AdvancedMC)
            //{
            //    // MC
            //    while (newScope == null || MC.ItertionsPerformed < MCProperties.NumberOfSteps)
            //    {
            //        newScope = MC.Grow(prevScope, remainingIds);
            //        prevScope = newScope;
            //    }
            //}
            //else
            //{
            // CA 
            while (newScope == null || !newScope.IsFull)
            {
                newScope = CA.Grow(prevScope, properties.NeighbourhoodType, properties.GrowthProbability, remainingIds);
                prevScope = newScope;
            }
            //}

            newScope.IsFull = true;
            StructureHelpers.UpdateBitmap(newScope);
            return newScope;
        }

        private Scope GeterateDualphaseStructure(List<Dictionary<Point, Grain>> remainingGrains, SimulationProperties properties)
        {
            Scope prevScope;
            if (properties.AdvancedMethodType == AdvancedMethodType.AdvancedMC)
            {
                prevScope = StructureHelpers.GenerateEmptyStructure(properties.ScopeWidth, properties.ScopeHeight);
            }
            else
            {
                prevScope = StructureHelpers.InitCAStructure(properties, random);
            }
            var newScope = new Scope(prevScope.Width, prevScope.Height);

            if (properties.AdvancedMethodType == AdvancedMethodType.AdvancedMC)
            {
                // MC
                while (newScope == null || MC.ItertionsPerformed < MCProperties.NumberOfSteps)
                {
                    newScope = MC.Grow(prevScope);
                    prevScope = newScope;
                }
            }
            else
            {
                // CA 
                while (newScope == null || !newScope.IsFull)
                {
                    newScope = CA.Grow(prevScope, properties.NeighbourhoodType, properties.GrowthProbability);
                    prevScope = newScope;
                }
            }

            // add second phase
            foreach (var grain in remainingGrains)
            {
                foreach (var g in grain)
                {
                    newScope.StructureArray[g.Key.X, g.Key.Y] = g.Value;
                }
            }

            newScope.IsFull = true;
            StructureHelpers.UpdateBitmap(newScope);
            return newScope;
        }
    }
}
