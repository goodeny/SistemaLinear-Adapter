using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCS
{
    public class LinearSystemAdapter
    {
        private readonly Dictionary<string, string> parameters;

        public LinearSystemAdapter(Dictionary<string, string> parameters)
        {
            this.parameters = parameters;
        }

        public (string solution, string algorithmName) Solve()
        {
            string algorithm = parameters["algorithm"];
            double[,] A = ParseMatrix(parameters["A"]);
            double[] b = ParseVector(parameters["b"]);

            ILinearSolver solver;

            switch (algorithm)
            {
                case "lu":
                    solver = new LUSolver();
                    break;
                case "cholesky":
                    solver = new CholeskySolver();
                    break;
                case "gauss":
                    solver = new GaussSolver();
                    break;
                default:
                    throw new NotImplementedException("Algorithm not supported");
            }

            solver.Execute(A, b, out double[,] L, out double[,] U, out double[] x);

            return (string.Join(", ", x), algorithm);
        }

        private double[,] ParseMatrix(string matrix)
        {
            var rows = matrix.Split(';');
            int numRows = rows.Length;
            int numCols = rows[0].Split(',').Length;
            double[,] result = new double[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                var cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length; j++)
                {
                    result[i, j] = double.Parse(cols[j]);
                }
            }

            return result;
        }

        private double[] ParseVector(string vector)
        {
            return vector.Split(',').Select(double.Parse).ToArray();
        }
    }
}
