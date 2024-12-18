﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using org.mariuszgromada.math.mxparser;


namespace Core
{
    public static class Experiment
    {
        readonly static int repetitions = 100;

        /// <returns>Average of the best results f(x) from each generation.</returns>
        public static double Run(in UserInputs inputs)
        {
            var bestResults = new List<double>();

            for (int i = 1; i <= repetitions; i++)
            {
                var algo = new Algorithm(inputs);
                var finalPopulation = algo.Run(out _);
                var best = finalPopulation.fs.Max();
                bestResults.Add(best);
            }
            var average = Math.Round(bestResults.Average(), inputs.genotypeSpace.precision.decimalPlaces);

            return average;
        }
    }
    public struct ExperimentParameterSet
    {
        public int N, T;
        public double pk, pm;
        public ExperimentParameterSet(int N, double pk, double pm, int T)
        {
            this.N = N;
            this.pm = pm;
            this.pk = pk;
            this.T = T;
        }
    }
    public class ExperimentsRunner
    {
        public readonly int totalCombinations;

        private readonly UserInputs defaultInput;
        private readonly List<int> N, T;
        private readonly List<double> pk, pm;
        public ExperimentsRunner(List<int> Ns, List<double> pks, List<double> pms, List<int> Ts)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(3, -4, 12);
            org.mariuszgromada.math.mxparser.License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");

            defaultInput = new(space, Ns[0], Ts[0], pks[0], pms[0], elitism: true, f, OptimizationGoal.Max);
            totalCombinations = Ns.Count * pks.Count * pms.Count * Ts.Count;

            this.N = Ns;
            this.pk = pks;
            this.pm = pms;
            this.T = Ts;
        }
        public IEnumerable<ExperimentParameterSet> GetAllParameterSets(int skip = 0)
        {
            for (int combination = skip; combination < totalCombinations; combination++)
            {
                int nIndex = combination % N.Count;
                int pkIndex = (combination / N.Count) % pk.Count;
                int pmIndex = (combination / (N.Count * pk.Count)) % pm.Count;
                int tIndex = (combination / (N.Count * pk.Count * pm.Count)) % T.Count;

                yield return new ExperimentParameterSet(N[nIndex], pk[pkIndex], pm[pmIndex], T[tIndex]);
            }
        }
        public UserInputs UserInputFromParameterSet(ExperimentParameterSet parameters)
        {
            UserInputs res = defaultInput;
            res.T = parameters.T;
            res.pk = parameters.pk;
            res.pm = parameters.pm;
            res.N = parameters.N;
            return res;
        }

        /// <param name="progress">
        /// <see cref="IProgress"/> object to which to report 
        /// <see cref="ExperimentParameterSet"/> as well as the experiment result (double)
        /// </param>
        public void Run(IProgress<(ExperimentParameterSet, double)> progress, Action onComplete)
        {
            int maxConcurrentThreads = Environment.ProcessorCount;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker.DoWork += (sender, e) =>
            {
                foreach (var parameterSet in GetAllParameterSets())
                {
                    semaphore.Wait();

                    var fx = Experiment.Run(UserInputFromParameterSet(parameterSet));
                    worker.ReportProgress(0, (parameterSet, fx));

                    semaphore.Release();
                }
            };

            worker.ProgressChanged += (sender, e) =>
            {
                var result = ((ExperimentParameterSet, double))e.UserState!;
                progress.Report(result);
            };

            worker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    Console.WriteLine("Error occurred: " + e.Error.Message);
                }
                else if (e.Cancelled)
                {
                    Console.WriteLine("Operation cancelled.");
                }
                onComplete?.Invoke();
            };

            worker.RunWorkerAsync();
        }
    }
}
