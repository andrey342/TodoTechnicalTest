namespace TodoManagement.API.Apis.Common.Gateways;

/// <summary>
/// Enum containing all supported API Gateways for TodoManagement.
/// </summary>
public enum GatewayTarget
{
    ApiGateway = 1
}

/// <summary>
/// Utility extensions for <see cref="GatewayTarget"/>.
/// </summary>
public static class GatewayTargetExtensions
{
    /// <summary>
    /// Converts a <see cref="GatewayTarget"/> value to its configuration string representation.
    /// </summary>
    /// <param name="target">The gateway target to convert.</param>
    /// <returns>The configuration string for the specified gateway target.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the target is not a valid <see cref="GatewayTarget"/> value.</exception>
    public static string ToConfigString(this GatewayTarget target) => target switch
    {
        GatewayTarget.ApiGateway => "ApiGateway"
,
        _ => throw new ArgumentOutOfRangeException(nameof(target))
    };

    /// <summary>
    /// Retrieves all defined <see cref="GatewayTarget"/> values.
    /// </summary>
    /// <returns>An array containing all <see cref="GatewayTarget"/> values.</returns>
    public static GatewayTarget[] GetAllTargets() => Enum.GetValues<GatewayTarget>();
}