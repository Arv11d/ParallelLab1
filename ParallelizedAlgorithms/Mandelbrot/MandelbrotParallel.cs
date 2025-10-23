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
        {
            ParallelCompute(new Tuple<double, double>(LowerX, UpperX),
        new Tuple<double, double>(LowerY, UpperY),
        Image);
        }
        protected void ParallelCompute(Tuple<double, double> xRange, Tuple<double, double> yRange, int[,] image)
        {
            int widthPixels = image.GetLength(0);
            int heightPixels = image.GetLength(1);
            double stepx = (xRange.Item2 - xRange.Item1) / widthPixels;
            double stepy = (yRange.Item2 - yRange.Item1) / heightPixels;

            System.Threading.Tasks.Parallel.For(0, widthPixels, i =>
            {
                for (int j = 0; j < heightPixels; j++)
                {
                    double tempx = xRange.Item1 + i * stepx;
                    double tempy = yRange.Item1 + j * stepy;
                    int color = Diverge(tempx, tempy);
                    image[i, j] = MAX_ITERATIONS - color;
                }
            });
        }
    }
}
