namespace RepoLens.Core.Exceptions;

public class GitException : RepoLensException
{
    public GitException(string message) : base(message) { }
    public GitException(string message, Exception inner) : base(message, inner) { }
}