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
        private Color energyOther { get; set; }

        public SRX(SRXProperties SRXProperties, Scope scope)
        {
            this.SRXProperties = SRXProperties;
            this.scope = scope;

            SetUpColors();
        }

        public Scope VisualizeEnergy()
        {
            return EnergyDistribution();
        }

        // to do
        public Scope GrowthNucleons(Scope currentScope, Scope previousScope)
        {
            return currentScope;
        }

        private Scope EnergyDistribution()
        {
            var energyScope = StructureHelpers.GenerateEmptyStructure(scope.Width, scope.Height);

            for (int i = 1; i < scope.Width - 1; i++)
            {
                for (int j = 1; j < scope.Height - 1; j++)
                {
                    if (scope.StructureArray[i, j].Id != Convert.ToInt32(SpecialIds.Border))
                    {
                        energyScope.StructureArray[i, j].Id = Convert.ToInt32(SpecialIds.Energy);
                        energyScope.StructureArray[i, j].Energy = SRXProperties.GrainEnergy;
                        energyScope.StructureArray[i, j].Color = ChooseColor(SRXProperties.GrainEnergy);
                    }
                    else
                    {
                        energyScope.StructureArray[i, j] = scope.StructureArray[i, j];
                    }
                }
            }
            if (SRXProperties.EnergyDistributionType == EnergyDistributionType.Heterogenous)
            {
                UpdateBoundariesEnergy(scope, energyScope);
            }

            energyScope.IsFull = true;
            StructureHelpers.UpdateBitmap(energyScope);

            return energyScope;
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

        private void SetUpColors()
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
                        energyScope.StructureArray[i, j].Color = ChooseColor(SRXProperties.BoundaryEnergy.Value);
                        energyScope.StructureArray[i, j].Energy = SRXProperties.BoundaryEnergy.Value;
                    }
                    if (((baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j + 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j + 1].Id))
                        || (baseScope.StructureArray[i, j].Id != baseScope.StructureArray[i, j - 1].Id
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j - 1].Id)))
                        && !StructureHelpers.IsIdSpecial(baseScope.StructureArray[i, j].Id))
                    {
                        energyScope.StructureArray[i, j].Color = ChooseColor(SRXProperties.BoundaryEnergy.Value);
                        energyScope.StructureArray[i, j].Energy = SRXProperties.BoundaryEnergy.Value;
                    }
                }
            }
        }
    }
}
