namespace AntiFraudService.API.Models;

public class ApiSettings
{
    public int Port { get; set; } = 5000;
    public int HttpsPort { get; set; } = 5001;
    public bool SwaggerEnabled { get; set; } = true;
    public bool CorsEnabled { get; set; } = false;
    public bool RequireHttps { get; set; } = false;
    public bool EnableCompression { get; set; } = false;
    public RateLimitingSettings? RateLimiting { get; set; }
    public SecuritySettings? Security { get; set; }
}

public class RateLimitingSettings
{
    public bool Enabled { get; set; } = false;
    public int MaxRequestsPerMinute { get; set; } = 100;
}

public class SecuritySettings
{
    public bool RequireHttps { get; set; } = false;
    public bool EnableCompression { get; set; } = false;
} 