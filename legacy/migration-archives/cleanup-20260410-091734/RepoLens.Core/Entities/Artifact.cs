namespace RepoLens.Core.Entities;

public class Artifact
{
    public int Id { get; set; }
    public required int RepositoryId { get; set; }
    public required string Path { get; set; }
}