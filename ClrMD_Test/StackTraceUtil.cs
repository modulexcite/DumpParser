using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;
using DiffLib; 
namespace ClrMD_Test
{
    public class StackTraceUtil
    {

        public static decimal StackTraceSimilarity(
            IList<ClrStackFrame> stackTrace,
            IList<ClrStackFrame> comparisonStackTrace)
        {
            if (stackTrace.Count == 0 || comparisonStackTrace.Count == 0) return 0;

            var originStackTraceDisplayNames = stackTrace.Select(row => row.DisplayString).ToList();
            var comparisonStackTraceDisplayNames = comparisonStackTrace.Select(row => row.DisplayString).ToList();

            var collectionDifference = new Diff<string>(originStackTraceDisplayNames, comparisonStackTraceDisplayNames).Generate()
                                                                                                                       .ToList();

            if (!collectionDifference.Any(diff => diff.Equal))
                return 0;

            var maxDifference = collectionDifference.Where(diff => diff.Equal)
                                                    .Max(diff => Math.Max(diff.Length1, diff.Length2));

            var similarityInPercent =  ((decimal)maxDifference / Math.Min(stackTrace.Count,comparisonStackTrace.Count)) * 100.0m;

            return similarityInPercent;
        }

        /// <summary>
        /// compare two stack traces - and return in percents how much two of them are similar
        /// </summary>
        public static decimal CompareStackTraces(
            IList<ClrStackFrame> stackTrace,
            IList<ClrStackFrame> comparisonStackTrace)
        {
            if (stackTrace.Count == 0 || comparisonStackTrace.Count == 0) return 0;

            var similarityPercentStep = 1.0m / stackTrace.Count;
            var totalSimilarityPercentage = 0.0m;

            var originStackTraceDisplayNames = stackTrace.Select(row => row.DisplayString).ToList();
            var comparisonStackTraceDisplayNames = comparisonStackTrace.Select(row => row.DisplayString).ToList();
            bool shouldContinueComparing = true;

            do
            {
                while (comparisonStackTraceDisplayNames.Any() && originStackTraceDisplayNames.Any())
                {
                    if (comparisonStackTraceDisplayNames.First() != originStackTraceDisplayNames.First())
                        comparisonStackTraceDisplayNames.RemoveAt(0);
                    else break;
                }

                while (originStackTraceDisplayNames.Any() && comparisonStackTraceDisplayNames.Any())
                {
                    if (comparisonStackTraceDisplayNames.First() != originStackTraceDisplayNames.First())
                        originStackTraceDisplayNames.RemoveAt(0);
                    else break;
                }

                if (originStackTraceDisplayNames.Any() && comparisonStackTraceDisplayNames.Any())
                {
                    totalSimilarityPercentage += similarityPercentStep;
                    originStackTraceDisplayNames.RemoveAt(0);
                    comparisonStackTraceDisplayNames.RemoveAt(0);
                }
                else
                {
                    shouldContinueComparing = false;
                }


            } while (shouldContinueComparing);

            return totalSimilarityPercentage * 100.0m;
        }
    }
}