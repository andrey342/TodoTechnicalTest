namespace TodoManagement.API.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Activity"/> class to enhance exception tracing.
/// </summary>
internal static class ActivityExtensions
{
    // See https://opentelemetry.io/docs/specs/otel/trace/semantic_conventions/exceptions/

    /// <summary>
    /// Sets standardized exception tags on the specified <see cref="Activity"/> instance,
    /// following OpenTelemetry semantic conventions for exception reporting.
    /// </summary>
    /// <param name="activity">The activity to which exception tags will be added.</param>
    /// <param name="ex">The exception to extract information from.</param>
    public static void SetExceptionTags(this Activity activity, Exception ex)
    {
        // If the activity is null, do not attempt to set tags.
        if (activity is null)
        {
            return;
        }

        // Add the exception message as a tag.
        activity.AddTag("exception.message", ex.Message);

        // Add the full exception stack trace as a tag.
        activity.AddTag("exception.stacktrace", ex.ToString());

        // Add the exception type as a tag.
        activity.AddTag("exception.type", ex.GetType().FullName);

        // Set the activity status to Error to indicate an exception occurred.
        activity.SetStatus(ActivityStatusCode.Error);
    }
}