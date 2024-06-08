using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webserver_csharp;

namespace WebServerCS
{
    public class CholeskySolver : ILinearSolver
    {
        public void Execute(double[,] A, double[] b, out double[,] L, out double[,] U, out double[] x)
        {
            int n = A.GetLength(0);
            L = new double[n, n];
            U = new double[n, n];
            x = new double[n];
            double[] y = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    L[i, j] = 0;
                    U[i, j] = 0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < j; k++)
                    {
                        sum += L[i, k] * L[j, k];
                    }

                    if (i == j)
                    {
                        L[i, j] = Math.Sqrt(A[i, i] - sum);
                    }
                    else
                    {
                        L[i, j] = (1.0 / L[j, j]) * (A[i, j] - sum);
                    }
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    U[j, i] = L[i, j];
                }
            }

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int k = 0; k < i; k++)
                {
                    sum += L[i, k] * y[k];
                }
                y[i] = (b[i] - sum) / L[i, i];
            }

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int k = i + 1; k < n; k++)
                {
                    sum += U[i, k] * x[k];
                }
                x[i] = (y[i] - sum) / U[i, i];
            }
        }
    }
}
