using FluentValidation;
using MediatR;

namespace TaskTracker.Application.Behaviors;

/// <summary>
/// Étape du pipeline MediatR qui valide automatiquement chaque requête
/// avant qu'elle n'atteigne son handler.
/// </summary>
/// <remarks>
/// Si au moins un validateur FluentValidation existe pour la requête et qu'une règle
/// n'est pas respectée, une <see cref="ValidationException"/> est levée.
/// Le GlobalExceptionHandler la traduit en réponse HTTP 400 détaillant chaque erreur.
/// Les handlers n'ont donc jamais à vérifier eux-mêmes le format des données.
/// </remarks>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}