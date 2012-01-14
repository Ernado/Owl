using System;
using System.Collections.Generic;
using Owl.Algorythms;

namespace Owl.GeneticAlgorithm.Domain
{
    class Organism
    {
        public Organism ()
        {
            Alleles = new List<bool>();
        }

        private List<GrayCode> _factors;
        public List<bool> Alleles { get; private set; }
        public double Likelihood { get; private set; }
        public double Fitness { get; set; }
        public void GenerateLikelihood (double fitnessSum)
        {
            Likelihood = Fitness/fitnessSum;
        }
        public void AddGene (bool gene)
        {
            Alleles.Add(gene);
        }
        public bool GetGene (int index)
        {
            return Alleles[index];
        }

        public int GenesCount { get { return Alleles.Count; } }

        public List<GrayCode> Factors
        {
            get
            {
                var factors = new List<GrayCode>();
                var code = Alleles;
                foreach (var factor in _factors)
                {
                    var bits = factor.BitAccuracy;
                    var newFactor = factor;
                    newFactor.BooleanCode = code.GetRange(0, bits);
                    factors.Add(newFactor);
                    code.RemoveRange(0,bits);
                }

                if (code.Count == 0)
                    return factors;

                throw new Exception("Cant decode alleles");
            }
            set { _factors = value; }
        } 
    }
}
