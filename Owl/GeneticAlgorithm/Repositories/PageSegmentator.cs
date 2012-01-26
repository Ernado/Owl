using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;
using Owl.Domain;
using Owl.GeneticAlgorithm.Domain;

namespace Owl.GeneticAlgorithm.Repositories
{
    internal class PageSegmentator : Domain.GeneticAlgorithm
    {
        private readonly AnalyzablePage _analyzablePage;
        private AnalyzablePage _solution;

        public PageSegmentator(Config configuration, AnalyzablePage analyzablePage)
        {
            GeneticConfiguration = configuration;
            _analyzablePage = analyzablePage;
        }

        private AnalyzablePage Solution(Organism organism)
        {
            AnalyzablePage page = _analyzablePage;
            var factorConfigs = new List<GrayCodeConfig>();
            GrayCode countFactor = organism.GenerateFactors()[0];
            int count = (int) countFactor.Value*2;
            for (int i = 0; i < count; i++)
                factorConfigs.Add(new GrayCodeConfig(0, page.Height, GeneticConfiguration.Accuracy));

            Config config = GeneticConfiguration;
            config.FactorConfigs = factorConfigs;
            var finder = new HypothesisFinder(config, page);
            page = finder.Hypothesis();
            return page;
        }

        protected override double Fitness(Organism organism)
        {
            AnalyzablePage segmentation = Solution(organism);
            return 1/
                   ((1 + segmentation.HeightRange())*(1 + segmentation.DistanceRange())*
                    (1 + segmentation.DensityRange()));
        }

        public AnalyzablePage SegmentedPage()
        {
            if (_solution != null)
                return _solution;
            Solve();
            _solution = Solution(BestOrganism);
            return _solution;
        }

        #region Nested type: HypothesisFinder

        private class HypothesisFinder : Domain.GeneticAlgorithm
        {
            private readonly AnalyzablePage _analyzablePage;
            private AnalyzablePage _solution;

            public HypothesisFinder(Config configuration, AnalyzablePage analyzablePage)
            {
                GeneticConfiguration = configuration;
                _analyzablePage = analyzablePage;
            }

            private AnalyzablePage Solution(Organism organism)
            {
                AnalyzablePage page = _analyzablePage;
                List<GrayCode> factors = organism.GenerateFactors();
                int count = factors.Count;

                if (count%2 != 0)
                    throw new Exception("Factors invalid, cannot form lines");

                List<GrayCode> odreredFactors = (from factor in factors orderby factor.Value select factor).ToList();

                for (int i = 0; i < factors.Count - 1; i += 2)
                {
                    var start = (int) odreredFactors[i].Value;
                    int height = (int) odreredFactors[i + 1].Value - start;
                    var line = new AnalyzableLine(start, height);
                    page.AddLine(line);
                }
                return page;
            }

            protected override double Fitness(Organism organism)
            {
                AnalyzablePage segmentation = Solution(organism);
                var heightsRange = segmentation.HeightRange();
                var distanceRange = segmentation.DistanceRange();
                var densityRange = segmentation.DensityRange();
                double fitness = 1/
                                 ((1 + heightsRange) * (1 + distanceRange) *
                                  (1 + densityRange));

                if (!((fitness > 0) && (fitness <= 1)))
                    throw new Exception("Calculated fitness is invalid");

                return fitness;
            }

            public AnalyzablePage Hypothesis()
            {
                if (_solution != null)
                    return _solution;

                Solve();
                _solution = Solution(BestOrganism);
                return _solution;
            }
        }

        #endregion
    }
}