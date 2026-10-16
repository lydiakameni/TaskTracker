namespace TaskTracker.Application.Exceptions;

/// <summary>
/// Levée lorsqu'une opération entre en conflit avec l'état actuel des données
/// (ex. : nom d'utilisateur déjà utilisé). Traduite en HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}