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
            _factors = new List<GrayCode>();
        }

        private List<GrayCode> _factors;
        public List<bool> Alleles { get; private set; }
        public double Likelihood { get; private set; }
        public double Fitness { get; set; }
        public void GenerateLikelihood (double fitnessSum)
        {
            try
            {
                if (GenesCount == 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Likelihood = Fitness / fitnessSum;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate likelihood",e);
            }
            
        }
        public void AddGene (bool gene)
        {
            Alleles.Add(gene);
        }
        public bool GetGene (int index)
        {
            return Alleles[index];
        }

        private void AddGenes (IEnumerable<bool> genes)
        {
            Alleles.AddRange(genes);
        }

        public void AddFactorAndCode (GrayCode factor)
        {
            AddFactor(factor);
            AddGenes(factor.BooleanCode);
        }

        private void AddFactor (GrayCode factor)
        {
            _factors.Add(factor);
        }

        public int GenesCount { get { return Alleles.Count; } }

        public List<GrayCode> Factors
        {
            get { return _factors; }
            set { _factors = value; }
        }

        public List<GrayCode> GenerateFactors()
        {
            try
            {
                if (_factors.Count == 0) throw new ArgumentOutOfRangeException();

                var factors = new List<GrayCode>();
                var code = Alleles;
                foreach (var factor in _factors)
                {
                    var bits = factor.BitAccuracy;
                    var newFactor = factor;
                    newFactor.BooleanCode = code.GetRange(0, bits);
                    factors.Add(newFactor);
                    code.RemoveRange(0, bits);
                }

                if (code.Count == 0)
                    return factors;

                throw new ArgumentOutOfRangeException();
            }
            catch (Exception e) 
            {
                throw new Exception("Cant generate factors",e);
            }
            
        }
    }
}
