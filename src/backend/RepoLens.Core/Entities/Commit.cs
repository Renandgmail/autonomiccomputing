namespace RepoLens.Core.Entities;

public class Commit
{
    public required string Sha { get; set; }
    public required int RepositoryId { get; set; }
    public required string Author { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string Message { get; set; }
}