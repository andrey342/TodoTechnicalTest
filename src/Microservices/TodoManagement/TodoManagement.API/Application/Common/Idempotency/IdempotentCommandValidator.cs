namespace TodoManagement.API.Application.Common.Idempotency;

/// <summary>
/// Validador genérico: garantiza que todo comando idempotente traiga
/// un RequestId distinto de Guid.Empty.
/// </summary>
/// <remarks>
/// Al heredar de <see cref="BaseValidator{T}"/> mantenemos acceso a métodos
/// reutilizables y al service-provider para futuros checks (si los necesitas).
/// </remarks>
public sealed class IdempotentCommandValidator<TCommand> : BaseValidator<TCommand>
    where TCommand : class, IIdempotentRequest
{
    public IdempotentCommandValidator(IServiceProvider sp) : base(sp)
    {
        Require(x => x.RequestId);
    }
}