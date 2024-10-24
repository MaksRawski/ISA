﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ISA
{
    public enum FunctionGoal
    {
        Max,
        Min,
    }
    public class Utils
    {
        public static int Bin2Int(string binaryString)
        {
            return Convert.ToInt32(binaryString, 2);
        }
        public static string Int2Bin(int x, int l)
        {
            return Convert.ToString(x, 2).PadLeft(l, '0');
        }

        public static double Int2Real(int x, double a, double b, int l)
        {
            return x * (b - a) / (Math.Pow(2, l) - 1) + a;
        }
        public static int Real2Int(double x, double a, double b, int l)
        {
            return (int)((x - a) / (b - a) * (Math.Pow(2, l) - 1));
        }

        public static string Real2Bin(double x, double a, double b, int l)
        {
            return Int2Bin(Real2Int(x, a, b, l), l);
        }
        public static double Bin2Real(string x, double a, double b, int l)
        {
            return Int2Real(Bin2Int(x), a, b, l);
        }
        private static double Gmax(Func<double, double> f, double x, double fMin, double d)
        {
            return f(x) - fMin + d;
        }
        private static double Gmin(Func<double, double> f, double x, double fMax, double d)
        {
            return -(f(x) - fMax) + d;
        }
        public static double G(Func<double, double> f, double x, FunctionGoal functionGoal, double fExtreme, double d)
        {
            return functionGoal == FunctionGoal.Max ?
                Gmax(f, x, fExtreme, d) : Gmin(f, x, fExtreme, d);
        }
        /// <summary>
        /// Finds the index of the smallest element in the sorted list 'qs' that is greater than or equal to the given value 'r'.
        /// This function performs a binary search on the list to achieve O(log n) time complexity.
        /// </summary>
        /// <param name="r">The threshold value to compare against the elements in 'qs'.</param>
        /// <param name="qs">A sorted list of double values representing cumulative distribution function (CDF).</param>
        /// <returns>The index of the first element in 'qs' that is greater than or equal to 'r'.</returns>
        public static int GetCDFIndex(double r, List<double> qs)
        {
            int low = 0, high = qs.Count - 1;

            while (low < high)
            {
                int mid = (low + high) / 2;

                if (qs[mid] < r)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
        public static Func<double, double>? ParseFunction(string expression)
        {
            // Define a parameter 'x' of type double
            ParameterExpression param = Expression.Parameter(typeof(double), "x");

            Func<double, double>? f = null;
            try
            {
                // Use DataTable's Compute to parse and evaluate the expression as a double
                var lambda = System.Linq.Dynamic.Core.DynamicExpressionParser
                    .ParseLambda(new[] { param }, typeof(double), expression);
                f = (Func<double, double>)lambda.Compile();
            }
            catch (System.Linq.Dynamic.Core.Exceptions.ParseException)
            {
                return null;
            }

            return f;
        }
    }
    public class TableRow
    {
        public int Lp { get; set; }
        public double XReal { get; set; }
        public double Fx { get; set; }
        public double Gx { get; set; }
        public double P { get; set; }
        public double Q { get; set; }
        public double R { get; set; }
        public double XCrossReal { get; set; }
        public string? XCrossBin { get; set; }
        public string? ParentFirstPart { get; set; }
        public string? ParentSecondPart { get; set; }
        public string? ParentColor { get; set; }
        public string? Pc { get; set; }
        public string? ChildFirstPart { get; set; }
        public string? ChildSecondPart { get; set; }
        public string? ChildFirstColor { get; set; }
        public string? ChildSecondColor { get; set; }
        public string? PopulationPostCross { get; set; }
        public string? MutPoint { get; set; }
        public string? PostMutBin { get; set; }
        public double PostMutReal { get; set; }
        public double PostMutFx { get; set; }
    }
}
