using Microsoft.AspNetCore.Http;

namespace WorldCupPlanner.UI.Tests;

[TestClass]
public class SecurityHeadersTests
{
    private const string ContentSecurityPolicy = "Content-Security-Policy";

    private static IHeaderDictionary Apply(bool isDevelopment = false)
    {
        var context = new DefaultHttpContext();
        SecurityHeaders.Apply(context.Response.Headers, isDevelopment);
        return context.Response.Headers;
    }

    // "script-src 'self'; img-src 'self' data:" -> { "script-src": "'self'", "img-src": "'self' data:" }
    private static Dictionary<string, string> PolicyDirectives(bool isDevelopment = false) =>
        Apply(isDevelopment)[ContentSecurityPolicy].ToString()
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(directive => directive.Split(' ', 2))
            .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1] : "");

    [DataTestMethod]
    [DataRow("X-Content-Type-Options", "nosniff")]
    [DataRow("X-Frame-Options", "DENY")]
    [DataRow("Referrer-Policy", "strict-origin-when-cross-origin")]
    [DataRow("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=()")]
    [DataRow("Cross-Origin-Opener-Policy", "same-origin")]
    public void Apply_SetsTheHeader(string header, string expectedValue)
    {
        Assert.AreEqual(expectedValue, Apply()[header].ToString());
    }

    [DataTestMethod]
    [DataRow("default-src", "'self'")]
    [DataRow("base-uri", "'self'")]
    [DataRow("object-src", "'none'")]
    [DataRow("frame-ancestors", "'none'")]
    [DataRow("form-action", "'self'")]
    [DataRow("script-src", "'self'")]
    [DataRow("style-src", "'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net")]
    [DataRow("font-src", "https://fonts.gstatic.com https://cdn.jsdelivr.net")]
    [DataRow("img-src", "'self' data:")]
    [DataRow("connect-src", "'self'")]
    public void ContentSecurityPolicy_Directive_AllowsOnlyWhatTheAppLoads(string directive, string expectedSources)
    {
        Assert.AreEqual(expectedSources, PolicyDirectives()[directive]);
    }

    [TestMethod]
    public void ContentSecurityPolicy_HasNoOtherDirectives()
    {
        Assert.AreEqual(10, PolicyDirectives().Count);
    }

    [TestMethod]
    public void ContentSecurityPolicy_InDevelopment_OnlyAddsTheLocalWebSocketsOfDotnetWatch()
    {
        Dictionary<string, string> production = PolicyDirectives();
        Dictionary<string, string> development = PolicyDirectives(isDevelopment: true);

        Assert.AreEqual("'self' ws://localhost:* wss://localhost:*", development["connect-src"]);
        production["connect-src"] = development["connect-src"];
        CollectionAssert.AreEquivalent(production, development);
    }

    [TestMethod]
    public void Apply_CalledTwiceOnTheSameResponse_DoesNotRepeatTheHeaders()
    {
        var context = new DefaultHttpContext();

        SecurityHeaders.Apply(context.Response.Headers, isDevelopment: false);
        SecurityHeaders.Apply(context.Response.Headers, isDevelopment: false);

        Assert.AreEqual(1, context.Response.Headers[ContentSecurityPolicy].Count);
        Assert.AreEqual(1, context.Response.Headers.XFrameOptions.Count);
    }
}
