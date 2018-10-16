using MultiscaleModelling.Models;
using System;

namespace MultiscaleModelling.Common
{
    public class Inclusions
    {
        private InclusionsProperties InclusionsProperties;

        public Inclusions(InclusionsProperties inclusionsProperties)
        {
            this.InclusionsProperties = inclusionsProperties;
        }

        public void AddInclusionsAtTheBigining()
        {
            throw new NotImplementedException();
        }

        public void AddInclusionsAfterGrainGrowth()
        {
            throw new NotImplementedException();
        }
    }
}
