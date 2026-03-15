namespace RepoLens.Core.Entities;

public class ArtifactVersion
{
    public int Id { get; set; }
    public required int ArtifactId { get; set; }
    public required string CommitSha { get; set; }
    public required string ContentHash { get; set; }
    public required string StoredAt { get; set; }
    public required string Metadata { get; set; }
}