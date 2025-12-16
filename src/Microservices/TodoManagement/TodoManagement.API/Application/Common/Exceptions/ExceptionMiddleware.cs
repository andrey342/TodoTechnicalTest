namespace TodoManagement.API.Application.Common.Exceptions;

/// <summary>
/// Middleware for handling exceptions and returning standardized error responses.
/// </summary>
public class ExceptionMiddleware
{
    #region Fields

    // Serializer options for JSON responses
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger instance for logging errors.</param>
    /// <param name="env">Web hosting environment.</param>
    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
    }

    #endregion

    #region Middleware Logic

    /// <summary>
    /// Invokes the middleware to handle exceptions during request processing.
    /// </summary>
    /// <param name="context">HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception with the current trace identifier
            using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
            {
                _logger.LogError(ex, "Unhandled exception");
            }
            await HandleExceptionAsync(context, ex);
        }
    }

    #endregion

    #region Exception Handling

    /// <summary>
    /// Handles the exception and writes a standardized error response.
    /// </summary>
    /// <param name="context">HTTP context for the current request.</param>
    /// <param name="exception">The exception to handle.</param>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Map exception types to HTTP status codes and problem details
        var (statusCode, problem) = exception switch
        {
            ValidationException ve => ((int)HttpStatusCode.BadRequest, CreateProblemDetails(ve, StatusCodes.Status400BadRequest, context)),
            UnauthorizedAccessException ue => ((int)StatusCodes.Status401Unauthorized, CreateProblemDetails(ue, StatusCodes.Status401Unauthorized, context)),
            OperationCanceledException _ => ((int)499, CreateProblemDetails(exception, 499, context)),
            _ => ((int)HttpStatusCode.InternalServerError, CreateProblemDetails(exception, StatusCodes.Status500InternalServerError, context))
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, SerializerOptions));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> object for the given exception.
    /// </summary>
    /// <param name="exception">The exception to describe.</param>
    /// <param name="statusCode">The HTTP status code to use.</param>
    /// <param name="context">HTTP context for the current request.</param>
    /// <returns>A populated <see cref="ProblemDetails"/> instance.</returns>
    private static ProblemDetails CreateProblemDetails(Exception exception, int statusCode, HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var pd = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = exception switch
            {
                ValidationException _ => "Validation error",
                UnauthorizedAccessException _ => "Unauthorized",
                _ => "Unexpected error"
            },
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path
        };
        pd.Extensions["traceId"] = traceId;
        return pd;
    }

    #endregion
}