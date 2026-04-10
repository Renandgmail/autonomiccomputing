namespace RepoLens.Core.Exceptions;

public abstract class RepoLensException : Exception
{
    protected RepoLensException(string message) : base(message) { }
    protected RepoLensException(string message, Exception inner) : base(message, inner) { }
}