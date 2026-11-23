namespace WorldCupPlanner.UI;

public static class SecurityHeaders
{
    private const string NoSniff = "nosniff";
    private const string DenyFraming = "DENY";
    private const string ReferrerPolicy = "strict-origin-when-cross-origin";
    private const string PermissionsPolicy = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    private const string CrossOriginOpenerPolicy = "same-origin";

    // Solo lo que carga la app: Archivo (Google Fonts), Bootstrap Icons (jsDelivr) y banderas como data URI.
    // style-src lleva 'unsafe-inline' porque los .razor y el aviso de reconexión de Blazor usan atributos style.
    // connect-src 'self' ya cubre el WebSocket de Blazor: en CSP3 'self' incluye ws/wss del mismo host y puerto.
    private const string ContentSecurityPolicy =
        "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; form-action 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net; " +
        "font-src https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "connect-src 'self'";

    // dotnet watch refresca el navegador por un WebSocket en otro puerto de localhost.
    private const string DevelopmentConnectSources = " ws://localhost:* wss://localhost:*";

    public static void Apply(IHeaderDictionary headers, bool isDevelopment)
    {
        // Se asigna y no se agrega: tras un error el pipeline corre otra vez sobre la misma respuesta.
        headers.XContentTypeOptions = NoSniff;
        headers.XFrameOptions = DenyFraming;
        headers["Referrer-Policy"] = ReferrerPolicy;
        headers["Permissions-Policy"] = PermissionsPolicy;
        headers["Cross-Origin-Opener-Policy"] = CrossOriginOpenerPolicy;
        headers.ContentSecurityPolicy = isDevelopment
            ? ContentSecurityPolicy + DevelopmentConnectSources
            : ContentSecurityPolicy;
    }
}
