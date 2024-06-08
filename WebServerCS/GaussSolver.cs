using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCS
{
    public class GaussSolver : ILinearSolver
    {
        public void Execute(double[,] A, double[] b, out double[,] L, out double[,] U, out double[] x)
        {
            int n = A.GetLength(0);
            L = new double[n, n];
            U = new double[n, n];
            x = new double[n];
            double[] y = new double[n];

            // Initialize L and U matrices
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        L[i, j] = 1;
                    else
                        L[i, j] = 0;

                    U[i, j] = A[i, j];
                }
            }

            // Gaussian elimination
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    double factor = U[i, k] / U[k, k];
                    L[i, k] = factor;
                    for (int j = k; j < n; j++)
                    {
                        U[i, j] -= factor * U[k, j];
                    }
                }
            }

            // Forward substitution to solve Ly = b
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int k = 0; k < i; k++)
                    sum += L[i, k] * y[k];
                y[i] = (b[i] - sum);
            }

            // Backward substitution to solve Ux = y
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int k = i + 1; k < n; k++)
                    sum += U[i, k] * x[k];
                x[i] = (y[i] - sum) / U[i, i];
            }
        }
    }
}
