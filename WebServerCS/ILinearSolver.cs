using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCS
{
    public interface ILinearSolver
    {
        void Execute(double[,] A, double[] b, out double[,] L, out double[,] U, out double[] x);
    }
}
