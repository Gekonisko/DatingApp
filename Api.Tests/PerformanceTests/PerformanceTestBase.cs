using System.Diagnostics;
using FluentAssertions;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

public abstract class PerformanceTestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;
    private readonly Stopwatch _stopwatch;
    private readonly List<PerformanceMetric> _metrics;

    protected PerformanceTestBase(ITestOutputHelper output)
    {
        Output = output;
        _stopwatch = new Stopwatch();
        _metrics = new List<PerformanceMetric>();
    }

    protected async Task<PerformanceResult> MeasurePerformanceAsync(
        string operationName,
        Func<Task> operation,
        int iterations = 1)
    {
        var results = new List<long>();
        var errors = 0;

        // Warmup
        try
        {
            await operation();
        }
        catch
        {
            // Ignore warmup errors
        }

        // Actual measurements
        for (int i = 0; i < iterations; i++)
        {
            _stopwatch.Restart();
            try
            {
                await operation();
                _stopwatch.Stop();
                results.Add(_stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                errors++;
                Output.WriteLine($"Error in iteration {i + 1}: {ex.Message}");
            }
        }

        var metric = new PerformanceMetric
        {
            OperationName = operationName,
            Iterations = iterations,
            Results = results,
            Errors = errors
        };

        _metrics.Add(metric);

        var result = new PerformanceResult
        {
            OperationName = operationName,
            TotalIterations = iterations,
            SuccessfulIterations = results.Count,
            FailedIterations = errors,
            AverageMs = results.Any() ? results.Average() : 0,
            MedianMs = results.Any() ? CalculateMedian(results) : 0,
            MinMs = results.Any() ? results.Min() : 0,
            MaxMs = results.Any() ? results.Max() : 0,
            P95Ms = results.Any() ? CalculatePercentile(results, 95) : 0,
            P99Ms = results.Any() ? CalculatePercentile(results, 99) : 0
        };

        LogPerformanceResult(result);

        return result;
    }

    protected async Task<PerformanceResult> MeasureConcurrentPerformanceAsync(
        string operationName,
        Func<Task> operation,
        int concurrentRequests = 10,
        int iterations = 1)
    {
        var results = new List<long>();
        var errors = 0;

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            var tasks = new List<Task<long>>();

            for (int i = 0; i < concurrentRequests; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        await operation();
                        sw.Stop();
                        return sw.ElapsedMilliseconds;
                    }
                    catch (Exception ex)
                    {
                        Output.WriteLine($"Error in concurrent request: {ex.Message}");
                        Interlocked.Increment(ref errors);
                        return -1;
                    }
                }));
            }

            var taskResults = await Task.WhenAll(tasks);
            results.AddRange(taskResults.Where(r => r >= 0));
        }

        var result = new PerformanceResult
        {
            OperationName = $"{operationName} (Concurrent: {concurrentRequests})",
            TotalIterations = concurrentRequests * iterations,
            SuccessfulIterations = results.Count,
            FailedIterations = errors,
            AverageMs = results.Any() ? results.Average() : 0,
            MedianMs = results.Any() ? CalculateMedian(results) : 0,
            MinMs = results.Any() ? results.Min() : 0,
            MaxMs = results.Any() ? results.Max() : 0,
            P95Ms = results.Any() ? CalculatePercentile(results, 95) : 0,
            P99Ms = results.Any() ? CalculatePercentile(results, 99) : 0
        };

        LogPerformanceResult(result);

        return result;
    }

    protected void AssertPerformance(
        PerformanceResult result,
        long maxAverageMs,
        long maxP95Ms,
        double minSuccessRate = 0.95)
    {
        var successRate = (double)result.SuccessfulIterations / result.TotalIterations;

        Output.WriteLine($"Asserting performance for: {result.OperationName}");
        Output.WriteLine($"Success Rate: {successRate:P2} (Min: {minSuccessRate:P2})");
        Output.WriteLine($"Average: {result.AverageMs}ms (Max: {maxAverageMs}ms)");
        Output.WriteLine($"P95: {result.P95Ms}ms (Max: {maxP95Ms}ms)");

        successRate.Should().BeGreaterThanOrEqualTo(minSuccessRate,
            $"Success rate should be at least {minSuccessRate:P2}");

        result.AverageMs.Should().BeLessThanOrEqualTo(maxAverageMs,
            $"Average response time should be under {maxAverageMs}ms");

        result.P95Ms.Should().BeLessThanOrEqualTo(maxP95Ms,
            $"95th percentile should be under {maxP95Ms}ms");
    }

    private double CalculateMedian(List<long> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        int count = sorted.Count;

        if (count == 0) return 0;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private double CalculatePercentile(List<long> values, int percentile)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(x => x).ToList();
        int index = (int)Math.Ceiling(percentile / 100.0 * sorted.Count) - 1;
        index = Math.Max(0, Math.Min(sorted.Count - 1, index));

        return sorted[index];
    }

    private void LogPerformanceResult(PerformanceResult result)
    {
        Output.WriteLine($"\n=== Performance Results: {result.OperationName} ===");
        Output.WriteLine($"Total Iterations: {result.TotalIterations}");
        Output.WriteLine($"Successful: {result.SuccessfulIterations}");
        Output.WriteLine($"Failed: {result.FailedIterations}");
        Output.WriteLine($"Average: {result.AverageMs:F2}ms");
        Output.WriteLine($"Median: {result.MedianMs:F2}ms");
        Output.WriteLine($"Min: {result.MinMs}ms");
        Output.WriteLine($"Max: {result.MaxMs}ms");
        Output.WriteLine($"P95: {result.P95Ms:F2}ms");
        Output.WriteLine($"P99: {result.P99Ms:F2}ms");
        Output.WriteLine($"==========================================\n");
    }

    public virtual void Dispose()
    {
        if (_metrics.Any())
        {
            Output.WriteLine("\n=== Summary of All Performance Tests ===");
            foreach (var metric in _metrics)
            {
                var avg = metric.Results.Any() ? metric.Results.Average() : 0;
                Output.WriteLine($"{metric.OperationName}: Avg={avg:F2}ms, Iterations={metric.Iterations}, Errors={metric.Errors}");
            }
            Output.WriteLine("=========================================\n");
        }
    }
}

public class PerformanceMetric
{
    public string OperationName { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public List<long> Results { get; set; } = new();
    public int Errors { get; set; }
}

public class PerformanceResult
{
    public string OperationName { get; set; } = string.Empty;
    public int TotalIterations { get; set; }
    public int SuccessfulIterations { get; set; }
    public int FailedIterations { get; set; }
    public double AverageMs { get; set; }
    public double MedianMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public double P95Ms { get; set; }
    public double P99Ms { get; set; }
}
