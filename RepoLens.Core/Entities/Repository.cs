namespace RepoLens.Core.Entities;

public class Repository
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string LastSyncCommit { get; set; }
}