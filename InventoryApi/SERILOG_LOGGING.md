# Serilog Logging Configuration

This project uses **Serilog** for structured logging, providing better log organization, correlation, and output flexibility compared to the default ASP.NET Core logging.

## Features

✅ **Structured Logging**: JSON-formatted logs in development/production  
✅ **File Output**: Rolling daily log files in `logs/` directory  
✅ **Console Output**: Compact JSON format for easy parsing  
✅ **Log Context Enrichment**: HTTP request context (method, path, status code, duration)  
✅ **User Context**: Logs enriched with authenticated user information (userId, email)  
✅ **Environment-aware Configuration**: Different log levels for development vs production  

## Configuration

### appsettings.json (Production)

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "InventoryAPI": "Information"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/inventoryapi-.log",
        "rollingInterval": "Day",
        "fileSizeLimitBytes": 104857600,
        "retainedFileCountLimit": 30
      }
    }
  ],
  "Enrich": ["FromLogContext"]
}
```

### appsettings.Development.json

Development environment uses **Debug** level logging for deeper visibility:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug",
    "Override": {
      "Microsoft.EntityFrameworkCore": "Debug",
      "InventoryAPI": "Debug"
    }
  }
}
```

## Log Output Examples

### Console (Compact JSON)
```json
{"@t":"2026-03-12T15:30:45.1234567Z","@mt":"HTTP request started: {Method} {Path}{QueryString}","@l":"Information","@x":"...",Method:"GET",Path:"/api/products",QueryString:"?pageNumber=1","Application":"InventoryAPI"}
```

### File Logs
```
2026-03-12 15:30:45.123 +00:00 [INF] HTTP request started: GET /api/products?pageNumber=1 from 127.0.0.1
2026-03-12 15:30:45.234 +00:00 [INF] HTTP request completed: GET /api/products responded with 200 in 111ms
```

## HTTP Request Logging Middleware

The custom `SerilogHttpLoggingMiddleware` captures:
- **Request Details**: Method, path, query parameters, remote IP
- **Response Status**: HTTP status code
- **Performance**: Request duration in milliseconds
- **User Context**: Authenticated user ID and email (if available)

Located in: [`Infrastructure/SerilogHttpLoggingMiddleware.cs`](Infrastructure/SerilogHttpLoggingMiddleware.cs)

## Service-Level Logging

### AuthService
Logs user registration and authentication events:
```csharp
_logger.LogInformation("User registration succeeded for email {Email}, userId {UserId}", 
    user.Email, user.Id);
```

### OrderService
Logs order lifecycle events:
```csharp
_logger.LogInformation("Order created with {ItemCount} items, total {TotalAmount}", 
    createOrderDto.Items.Count, totalAmount);
```

## Log File Management

- **Location**: `logs/inventoryapi-{date}.log`
- **Rolling**: Creates new file daily (e.g., `inventoryapi-2026-03-12.log`)
- **Retention**: Keeps 30 days of logs
- **Size Limit**: 100 MB per file (enforced automatically)

## Using Serilog in Your Code

### Inject ILogger
```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public void DoSomething()
    {
        _logger.LogInformation("Action started for userId {UserId}", userId);
        _logger.LogWarning("Issue detected: {Issue}", issueDescription);
        _logger.LogError(exception, "Error occurred during processing");
    }
}
```

### Log Context Enrichment
For correlating logs across requests:
```csharp
using Serilog.Context;

LogContext.PushProperty("OrderId", orderId);
_logger.LogInformation("Processing order");
// All logs in this scope include OrderId automatically
```

## Log Levels

| Level | Usage | Production Default |
|-------|-------|-------------------|
| **Verbose** | Detailed diagnostic info | Not used |
| **Debug** | Low-level debugging | Not used |
| **Information** | General application flow | ✓ (InventoryAPI) |
| **Warning** | Potential issues (e.g., invalid data) | ✓ (Framework) |
| **Error** | Application errors | ✓ (All) |
| **Fatal** | Unrecoverable errors causing shutdown | ✓ (All) |

## Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| `Serilog.AspNetCore` | 9.0.0 | ASP.NET Core integration |
| `Serilog.Sinks.Console` | 6.0.0 | Console output |
| `Serilog.Sinks.File` | 6.0.0 | Rolling file output |
| `Serilog.Enrichers.Environment` | 3.0.0 | Environment/machine enrichment |
| `Serilog.Formatting.Compact` | 3.0.0 | Compact JSON formatting |

## Troubleshooting

### Logs not appearing in console?
- Check `MinimumLevel` in appsettings for your environment
- Verify logger is named correctly (namespace.ClassName)
- Ensure middleware is registered in Program.cs: `app.UseSerilogHttpLogging()`

### File logs growing too fast?
- Decrease `retainedFileCountLimit` to keep fewer days
- Reduce log levels for verbose components (Microsoft, EntityFrameworkCore)
- Implement additional filtering in Serilog configuration

### Want more enrichment?
- Add `Serilog.Enrichers.Environment` for machine name, OS info
- Add `Serilog.Enrichers.Thread` for thread ID tracking
- Create custom enrichers for business-specific context

## Best Practices

1. **Use structured logging**: Avoid string concatenation, use named properties
   ```csharp
   // ✓ Good
   _logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
   
   // ✗ Bad
   _logger.LogInformation($"User {userId} logged in from {ipAddress}");
   ```

2. **Log at appropriate levels**: Information for business events, Warning for unexpected conditions, Error for failures

3. **Include context**: Use LogContext to correlate related operations across multiple logs

4. **Don't log sensitive data**: Avoid logging passwords, tokens, or PII unless in development mode

5. **Use lazy evaluation**: Parameters are only evaluated if the log level is enabled
   ```csharp
   _logger.LogDebug("Value is {Value}", ExpensiveMethod()); // Safe - not called if Debug disabled
   ```

## References

- [Serilog Documentation](https://serilog.net)
- [Serilog Configuration](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)
