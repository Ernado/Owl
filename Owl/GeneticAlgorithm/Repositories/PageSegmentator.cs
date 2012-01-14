using System;
using System.Collections.Generic;
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
                var page = _analyzablePage;
                var factors = organism.Factors;
                var count = factors.Count;

                if (count%2 == 0)
                    throw new Exception("Factors invalid, cannot form lines");

                for (var i = 0; i < factors.Count; i+=2)
                {
                    var start = (int)factors[i].Value;
                    var height = (int)factors[i + 1].Value - start;
                    var line = new Line(start, height);
                    page.AddLine(line);
                }
                return page;
            }

            protected override double Fitness(Organism organism)
            {
                var segmentation = Solution(organism);
                return 1/
                       ((1 + segmentation.HeightRange())*(1 + segmentation.DistanceRange())*
                        (1 + segmentation.DensityRange()));
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

        public PageSegmentator(Config configuration, AnalyzablePage analyzablePage)
        {
            GeneticConfiguration = configuration;
            _analyzablePage = analyzablePage;
        }

        private AnalyzablePage Solution (Organism organism)
        {
            var page = _analyzablePage;
            var factors = new List<GrayCode>();
            var count = (int)organism.Factors[0].Value;
            for (int i = 0; i < count; i++)
                factors.Add(new GrayCode(0, 0, page.Height, GeneticConfiguration.Accuracy));

            var config = GeneticConfiguration;
            config.Factors = factors;
            var finder = new HypothesisFinder(config, page);
            page = finder.Hypothesis();
            return page;
        }

        protected override double Fitness(Organism organism)
        {
            var segmentation = Solution(organism);
            return 1 /
                   ((1 + segmentation.HeightRange()) * (1 + segmentation.DistanceRange()) *
                    (1 + segmentation.DensityRange()));
        }

        public AnalyzablePage SegmentedPage ()
        {
            if (_solution != null)
                return _solution;

            Solve();
            _solution = Solution(BestOrganism);
            return _solution;
        }
    }
}
