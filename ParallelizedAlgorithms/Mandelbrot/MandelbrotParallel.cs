using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public class MandelbrotParallel : MandelbrotBase
    {
        public override string Name { 
            get { return "MandelbrotParallel"; }
        }

        public MandelbrotParallel(int pixelsX, int pixelsY) : base(pixelsX, pixelsY)
        {
        }
        public override void Compute()
        { }
        public override void ParallelCompute()
        {
            ParallelCompute(new Tuple<double, double>(LowerX, UpperX),
                    new Tuple<double, double>(LowerY, UpperY),
                    Image);
        }
    }
}
