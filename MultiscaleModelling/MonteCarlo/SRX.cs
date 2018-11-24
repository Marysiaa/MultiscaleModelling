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
        private SRXProperties SRXProperties { get; set; }
        private Scope scope { get; set; }

        private Color energy0 { get; set; }
        private Color energy1 { get; set; }
        private Color energy2 { get; set; }
        private Color energy3 { get; set; }
        private Color energy4 { get; set; }
        private Color energy5 { get; set; }
        private Color energy6 { get; set; }
        private Color energy7 { get; set; }
        private Color energy8 { get; set; }
        private Color energyOther { get; set; }

        // to do
        public SRX(SRXProperties SRXProperties, Scope scope)
        {
            this.SRXProperties = SRXProperties;
            this.scope = scope;

            SetUpColors();
        }

        public Scope VisualizeEnergy()
        {
            switch (SRXProperties.EnergyDistributionType)
            {
                case EnergyDistributionType.Heterogenous:
                    return HeterogenousDistrbution();
                case EnergyDistributionType.Homogenous:
                    return HomogenousDistribution();
                default:
                    return null;
            }
        }

        // to do
        public Scope GrowthNucleons()
        {
            throw new NotImplementedException();
        }

        // to do
        private Scope HeterogenousDistrbution()
        {
            return EnergyDistribution();
        }

        // to do
        private Scope HomogenousDistribution()
        {
            return EnergyDistribution();
        }

        private Scope EnergyDistribution()
        {
            var energyScope = StructureHelpers.GenerateEmptyStructure(scope.Width, scope.Height);

            for (int i = 1; i < scope.Width - 1; i++)
            {
                for (int j = 1; j < scope.Height - 1; j++)
                {
                    if (scope.StructureArray[i, j].Id != Convert.ToInt32(SpecialIds.Boundaries))
                    {
                        energyScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Energy);
                        // calculate energy
                        var neighbourhood = StructureHelpers.TakeMooreNeighbourhood(i, j, scope.StructureArray);
                        int energy = neighbourhood.Where(g => (!StructureHelpers.IsIdSpecial(g.Id) && g.Id != scope.StructureArray[i, j].Id)).Count();
                        int specials = neighbourhood.Where(g => StructureHelpers.IsIdSpecial(g.Id)).Count();
                        if (specials > 0 && energy > 0)
                        {
                            //energy += specials / 2;
                        }
                        energyScope.StructureArray[i, j].Color = ChooseColor(energy);
                    }
                    else
                    {
                        energyScope.StructureArray[i, j].Id = scope.StructureArray[i, j].Id;
                    }
                }
            }

            energyScope.IsFull = true;
            StructureHelpers.UpdateBitmap(energyScope);

            return energyScope;
        }

        private Color ChooseColor(int energy)
        {
            switch (energy)
            {
                case 0: return energy0;
                case 1: return energy1;
                case 2: return energy2;
                case 3: return energy3;
                case 4: return energy4;
                case 5: return energy5;
                case 6: return energy6;
                case 7: return energy7;
                case 8: return energy8;
                default: return energyOther;
            }
        }

        private void SetUpColors()
        {
            this.energy0 = Color.RoyalBlue;
            this.energy1 = Color.DodgerBlue;
            this.energy2 = Color.Aquamarine;
            this.energy3 = Color.CornflowerBlue;
            this.energy4 = Color.RoyalBlue;
            this.energy5 = Color.PaleVioletRed;
            this.energy6 = Color.IndianRed;
            this.energy7 = Color.Crimson;
            this.energy8 = Color.DarkRed;
            this.energyOther = Color.FloralWhite;
        }
    }
}
