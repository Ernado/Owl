using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;
using Owl.Domain;
using Owl.GeneticAlgorithm.Domain;

namespace Owl.GeneticAlgorithm.Repositories
{
    class PageSegmentator : Domain.GeneticAlgorithm
    {
        private readonly AnalyzablePage _analyzablePage;
        private AnalyzablePage _solution;

        private class HypothesisFinder : Domain.GeneticAlgorithm
        {
            private readonly AnalyzablePage _analyzablePage;
            private AnalyzablePage _solution;
            public HypothesisFinder(Config configuration, AnalyzablePage analyzablePage)
            {
                GeneticConfiguration = configuration;
                _analyzablePage = analyzablePage;
            }

            private AnalyzablePage Solution (Organism organism)
            {
                try
                {
                    var page = _analyzablePage;
                    var factors = organism.GenerateFactors();
                    var count = factors.Count;

                    if (count % 2 != 0)
                        throw new Exception("Factors invalid, cannot form lines");

                    var odreredFactors = (from factor in factors orderby factor.Value select factor).ToList();

                    for (var i = 0; i < factors.Count - 1; i += 2)
                    {
                        var start = (int)odreredFactors[i].Value;
                        var height = (int)odreredFactors[i + 1].Value - start;
                        var line = new AnalyzableLine(start, height);
                        page.AddLine(line);
                    }
                    return page;
                }
                catch (Exception e)
                {
                    throw new Exception("Cant calculate solution",e);
                }
            }

            protected override double Fitness(Organism organism)
            {
                try
                {
                    var segmentation = Solution(organism);
                    var fitness = 1 /
                           ((1 + segmentation.HeightRange()) * (1 + segmentation.DistanceRange()) *
                            (1 + segmentation.DensityRange()));

                    if (!((fitness > 0) && (fitness <= 1)))
                        throw new Exception("Calculated fitness is invalid");

                    return fitness;
                }
                catch (Exception e)
                {
                    throw new Exception("Cant calculate fitness",e);
                }
                
            }

            public AnalyzablePage Hypothesis()
            {
                if (_solution != null)
                    return _solution;
                try
                {
                    Solve();
                    _solution = Solution(BestOrganism);
                    return _solution;
                }
                catch (Exception e)
                {
                    throw new Exception("Cant calculate Hypothesis",e);
                }
                
            }
        }

        public PageSegmentator(Config configuration, AnalyzablePage analyzablePage)
        {
            GeneticConfiguration = configuration;
            _analyzablePage = analyzablePage;
        }

        private AnalyzablePage Solution (Organism organism)
        {
            try
            {
                var page = _analyzablePage;
                var factors = new List<GrayCode>();
                var countFactor = organism.GenerateFactors()[0];
                var count = (int)countFactor.Value * 2;
                for (int i = 0; i < count; i++)
                    factors.Add(new GrayCode(0, 0, page.Height, GeneticConfiguration.Accuracy));

                var config = GeneticConfiguration;
                config.Factors = factors;
                var finder = new HypothesisFinder(config, page);
                page = finder.Hypothesis();
                return page;
            }
            catch (Exception e)
            {
                throw new Exception("Cant calculate solution",e);
            }
            
        }

        protected override double Fitness(Organism organism)
        {
            try
            {
                var segmentation = Solution(organism);
                return 1 /
                       ((1 + segmentation.HeightRange()) * (1 + segmentation.DistanceRange()) *
                        (1 + segmentation.DensityRange()));
            }
            catch (Exception e)
            {
                throw new Exception("Cant calculate fitness",e);
            }

            
        }

        public AnalyzablePage SegmentedPage ()
        {
            if (_solution != null)
                return _solution;
            try
            {
                Solve();
                _solution = Solution(BestOrganism);
                return _solution;
            }
            catch (Exception e)
            {
                throw new Exception("Cant segment page",e);
            }
            
        }
    }
}
