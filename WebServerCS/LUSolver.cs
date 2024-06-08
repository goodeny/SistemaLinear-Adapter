using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCS
{
    public class LUSolver : ILinearSolver
    {
        public void Execute(double[,] A, double[] b, out double[,] L, out double[,] U, out double[] x)
        {
            int n = A.GetLength(0);
            L = new double[n, n];
            U = new double[n, n];
            x = new double[n];
            double[] y = new double[n];

            // Decomposição LU (método simplificado, sem pivotamento)
            for (int i = 0; i < n; i++)
            {
                for (int k = i; k < n; k++)
                {
                    double sum = 0;
                    for (int j = 0; j < i; j++)
                        sum += (L[i, j] * U[j, k]);
                    U[i, k] = A[i, k] - sum;
                }
                for (int k = i; k < n; k++)
                {
                    if (i == k)
                        L[i, i] = 1; // Diagonal da L é 1
                    else
                    {
                        double sum = 0;
                        for (int j = 0; j < i; j++)
                            sum += (L[k, j] * U[j, i]);
                        L[k, i] = (A[k, i] - sum) / U[i, i];
                    }
                }
            }

            // Resolução do sistema Ly = b
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < i; j++)
                    sum += L[i, j] * y[j];
                y[i] = b[i] - sum;
            }

            // Resolução do sistema Ux = y
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                    sum += U[i, j] * x[j];
                x[i] = (y[i] - sum) / U[i, i];
            }
        }
    }
}
