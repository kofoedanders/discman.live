using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Web.PerformanceTests.Tests;

[TestFixture]
[Category("Performance")]
public sealed class K6PerfTests
{
    [Test]
    [CancelAfter(300_000)]
    public async Task RunK6PerformanceTests()
    {
        var serverUrl = AppFixture.Instance.ServerUrl;
        TestContext.Out.WriteLine($"Server URL: {serverUrl}");

        var k6Path = FindK6();
        if (k6Path == null)
        {
            Assert.Inconclusive("k6 is not installed. Install via: brew install k6");
            return;
        }

        var scriptPath = FindK6Script();
        Assert.That(File.Exists(scriptPath), Is.True, $"k6 script not found at {scriptPath}");

        var resultsPath = Path.Combine(Path.GetDirectoryName(scriptPath)!, "results.json");

        var psi = new ProcessStartInfo
        {
            FileName = k6Path,
            Arguments = $"run --env BASE_URL={serverUrl} --summary-export \"{resultsPath}\" \"{scriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        TestContext.Out.WriteLine($"Running: {psi.FileName} {psi.Arguments}");
        TestContext.Out.WriteLine();

        using var process = Process.Start(psi)!;

        var stdoutTask = Task.Run(async () =>
        {
            while (await process.StandardOutput.ReadLineAsync() is { } line)
            {
                TestContext.Out.WriteLine(line);
            }
        });

        var stderrTask = Task.Run(async () =>
        {
            while (await process.StandardError.ReadLineAsync() is { } line)
            {
                TestContext.Error.WriteLine(line);
            }
        });

        await process.WaitForExitAsync();
        await Task.WhenAll(stdoutTask, stderrTask);

        TestContext.Out.WriteLine();
        TestContext.Out.WriteLine($"k6 exit code: {process.ExitCode}");

        if (File.Exists(resultsPath))
        {
            TestContext.Out.WriteLine($"Results saved to: {resultsPath}");
        }

        Assert.That(process.ExitCode, Is.EqualTo(0), "k6 exited with non-zero status");
    }

    private static string? FindK6()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "k6",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi)!;
            var path = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return p.ExitCode == 0 && !string.IsNullOrEmpty(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }

    private static string FindK6Script()
    {
        var dir = Directory.GetCurrentDirectory();
        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "tests", "Web.PerformanceTests", "perf-test.js");
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(dir, "perf-test.js");
            if (File.Exists(candidate)) return candidate;

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        return Path.Combine(AppContext.BaseDirectory, "perf-test.js");
    }
}
