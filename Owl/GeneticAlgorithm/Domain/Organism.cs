using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;

namespace Owl.GeneticAlgorithm.Domain
{
    public class Organism
    {
        private List<GrayCode> _factors;

        public Organism()
        {
            Alleles = new List<bool>();
            _factors = new List<GrayCode>();
        }

        public List<bool> Alleles { get; private set; }
        public double Likelihood { get; private set; }
        public double Fitness { get; set; }

        public int GenesCount
        {
            get { return Alleles.Count; }
        }

        public List<GrayCode> Factors
        {
            get { return _factors; }
            set { _factors = value; }
        }

        public void GenerateLikelihood(double fitnessSum)
        {
            try
            {
                if (GenesCount == 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                Likelihood = Fitness/fitnessSum;
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate likelihood", e);
            }
        }

        public void AddGene(bool gene)
        {
            Alleles.Add(gene);
        }

        public bool GetGene(int index)
        {
            return Alleles[index];
        }

        private void AddGenes(IEnumerable<bool> genes)
        {
            Alleles.AddRange(genes);
        }

        public void AddFactorAndCode(GrayCode factor)
        {
            AddFactor(factor);
            AddGenes(factor.BooleanCode);
        }

        private void AddFactor(GrayCode factor)
        {
            _factors.Add(factor);
        }

        public List<GrayCode> GenerateFactors()
        {
            try
            {
                if (_factors.Count == 0) throw new ArgumentOutOfRangeException();

                var factors = new List<GrayCode>();
                var code = new List<bool>();
                code.AddRange(Alleles);
                foreach (GrayCode factor in _factors)
                {
                    uint bits = factor.BitAccuracy;
                    var newFactor = new GrayCode(0, factor.Config);
                    newFactor.BooleanCode = code.GetRange(0, (int) bits);
                    factors.Add(newFactor);
                    code.RemoveRange(0, (int) bits);
                }

                if (code.Count == 0)
                    return factors;

                throw new ArgumentOutOfRangeException();
            }
            catch (Exception e)
            {
                throw new Exception("Cant generate factors", e);
            }
        }

        public override string ToString()
        {
            string s = _factors.Aggregate(" Code:[", (current, grayCode) => current + grayCode.StringCode) + "]";
            return string.Format("Fitness: {0}, Likalihood: {1}", Math.Round(Fitness, 2), Math.Round(Likelihood, 2)) + s;
        }
    }
}