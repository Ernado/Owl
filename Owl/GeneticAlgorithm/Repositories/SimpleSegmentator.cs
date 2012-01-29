using System;
using System.Collections.Generic;
using System.Linq;
using Owl.Algorythms;
using Owl.Domain;
using Owl.GeneticAlgorithm.Domain;
using Owl.Repositories;

namespace Owl.GeneticAlgorithm.Repositories
{
    internal class SimpleSegmentator : Domain.GeneticAlgorithm
    {
        private readonly AnalyzablePage _analyzablePage;
        private AnalyzablePage _solution;
        private readonly float[] _hystogram;
        private int _width;

        public SimpleSegmentator(Config configuration, AnalyzablePage analyzablePage)
        {
            GeneticConfiguration = configuration;
            _analyzablePage = analyzablePage;
            _hystogram = Functions.FeautureScaling(Hystogram.GetHystogram(_analyzablePage.Original));
            _width = _analyzablePage.Width;
        }

        private AnalyzablePage Solution(Organism organism)
        {
            try
            {
                var triggerValue = organism.Factors[0].Value;
                var minHeight = organism.Factors[1].Value;
                var height = _hystogram.GetLength(0);
                var page = new AnalyzablePage(height,_width);

                int start = 0;

                var isLine = false;

                for (var y = 0; y < height; y++)
                {
                    var trigger = (_hystogram[y] > triggerValue);

                    if (trigger)
                    {
                        if (!isLine)
                        {
                            start = y;
                            isLine = true;
                        }
                    }
                    else
                    {
                        if (isLine)
                        {
                            int end = y;
                            var lineHeight = end - start + 1;
                            if (lineHeight > minHeight)
                                page.AddLine(new AnalyzableLine(start, lineHeight));
                            isLine = false;
                        }

                    }
                }

                return page;

            }
            catch (Exception e)
            {
                throw new Exception("Cant generate solution",e);
            }
            
           
        }

        protected override double Fitness(Organism organism)
        {
            try
            {
                AnalyzablePage segmentation = Solution(organism);
                int heightsRange = segmentation.HeightRange();
                int distanceRange = segmentation.DistanceRange();
                //double densityRange = segmentation.DensityRange();
                var summ = (segmentation.Lines.Count > 2) ? segmentation.Lines.Sum(line => line.Height) : 0;
                double fitness = 1 /
                                 ((double)(1 + heightsRange) * (1 + distanceRange)*(1+Math.Abs(2*segmentation.Height - summ)));

                if (!((fitness >= 0) && (fitness <= 1)))
                    throw new ArgumentOutOfRangeException();

                return fitness;
            }
            catch (Exception e)
            {
                throw new Exception("Cant calculate fitness",e);
            }
            
        }

        public AnalyzablePage GetSegmentation()
        {
            if (_solution != null)
                return _solution;

            Solve();
            _solution = Solution(BestOrganism);
            return _solution;
        }
    }
}