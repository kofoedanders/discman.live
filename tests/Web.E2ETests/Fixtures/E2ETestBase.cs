using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Web.E2ETests;

[Category("E2E")]
public abstract class E2ETestBase
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    protected IPage Page { get; private set; } = null!;
    protected string ServerUrl => AppFixture.Instance.ServerUrl;

    [SetUp]
    public async Task BaseSetUp()
    {
        _playwright = await Playwright.CreateAsync();

        var headless = Environment.GetEnvironmentVariable("HEADED") != "1";

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
        });

        Page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            await CaptureScreenshotOnFailure();
        }

        if (Page != null) await Page.CloseAsync();
        if (_context != null) await _context.DisposeAsync();
        if (_browser != null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }

    private async Task CaptureScreenshotOnFailure()
    {
        try
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var dir = Path.Combine(TestContext.CurrentContext.TestDirectory, "test-results", "screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"{testName}-{timestamp}.png");

            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = path,
                FullPage = true
            });

            TestContext.AddTestAttachment(path, "Failure screenshot");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Failed to capture screenshot: {ex.Message}");
        }
    }
}
