namespace RepoLens.Core.Exceptions;

public class StorageException : RepoLensException
{
    public StorageException(string message) : base(message) { }
    public StorageException(string message, Exception inner) : base(message, inner) { }
}