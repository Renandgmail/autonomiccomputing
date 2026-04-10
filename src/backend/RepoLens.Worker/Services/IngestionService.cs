using System.Security.Cryptography;
using System.Text.Json;
using LibGit2Sharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepoLens.Core.Entities;
using RepoLens.Core.Exceptions;
using RepoLens.Core.Repositories;
using RepoLens.Infrastructure.Git;
using RepoLens.Infrastructure.Storage;
using GitRepository = LibGit2Sharp.Repository;

namespace RepoLens.Worker.Services;

public class IngestionService
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IArtifactRepository _artifactRepository;
    private readonly GitService _gitService;
    private readonly StorageService _storageService;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(
        IRepositoryRepository repositoryRepository,
        IArtifactRepository artifactRepository,
        GitService gitService,
        StorageService storageService,
        ILogger<IngestionService> logger)
    {
        _repositoryRepository = repositoryRepository;
        _artifactRepository = artifactRepository;
        _gitService = gitService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task IngestRepositoryAsync(string repositoryUrl, string localPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting ingestion for repository: {RepositoryUrl}", repositoryUrl);

        var repo = await _repositoryRepository.GetByUrlAsync(repositoryUrl, ct);
        if (repo == null)
        {
            repo = new RepoLens.Core.Entities.Repository
            {
                Name = Path.GetFileNameWithoutExtension(repositoryUrl),
                Url = repositoryUrl,
                LastSyncCommit = string.Empty
            };
            repo = await _repositoryRepository.AddAsync(repo, ct);
        }

        GitRepository gitRepo = await _gitService.OpenOrCloneRepositoryAsync(repositoryUrl, localPath, ct);
        var commits = await _gitService.GetCommitsAsync(gitRepo, repo.Id, ct);

        foreach (var commit in commits)
        {
            if (commit.Sha == repo.LastSyncCommit)
            {
                _logger.LogInformation("Reached last synced commit {CommitSha}, skipping remaining commits", commit.Sha);
                break;
            }

            try
            {
                await ProcessCommitAsync(repo.Id, commit, gitRepo, ct);
                repo.LastSyncCommit = commit.Sha;
                await _repositoryRepository.UpdateAsync(repo, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process commit {CommitSha}", commit.Sha);
                throw new GitException($"Failed to process commit {commit.Sha}", ex);
            }
        }

        _logger.LogInformation("Completed ingestion for repository: {RepositoryUrl}", repositoryUrl);
    }

    private async Task ProcessCommitAsync(int repositoryId, RepoLens.Core.Entities.Commit commit, GitRepository gitRepo, CancellationToken ct)
    {
        var libGitCommit = gitRepo.Lookup<LibGit2Sharp.Commit>(commit.Sha);
        var tree = libGitCommit.Tree;

        foreach (var entry in tree)
        {
            if (entry.TargetType != TreeEntryTargetType.Blob)
                continue;

            var filePath = entry.Path;
            var content = await _gitService.GetBlobContentAsync(gitRepo, commit.Sha, filePath, ct);
            var contentHash = ComputeSha256Hash(content);

            var artifact = await _artifactRepository.GetByPathAsync(repositoryId, filePath, ct);
            if (artifact == null)
            {
                artifact = new Artifact
                {
                    RepositoryId = repositoryId,
                    Path = filePath
                };
                artifact = await _artifactRepository.AddAsync(artifact, ct);
            }

            var existingVersion = await _artifactRepository.GetByHashAsync(artifact.Id, contentHash, ct);
            if (existingVersion != null)
            {
                _logger.LogInformation("Artifact {FilePath} with hash {ContentHash} already exists, skipping upload", filePath, contentHash);
                continue;
            }

            var storedAt = await _storageService.UploadAsync(key: contentHash, content: content, ct: ct);
            var metadata = await ExtractMetadataAsync(filePath, content, ct);

            var artifactVersion = new ArtifactVersion
            {
                ArtifactId = artifact.Id,
                CommitSha = commit.Sha,
                ContentHash = contentHash,
                StoredAt = storedAt,
                Metadata = metadata
            };

            await _artifactRepository.AddVersionAsync(artifactVersion, ct);
            _logger.LogInformation("Processed artifact {FilePath} with hash {ContentHash}", filePath, contentHash);
        }
    }

    private string ComputeSha256Hash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(content);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private async Task<string> ExtractMetadataAsync(string filePath, byte[] content, CancellationToken ct)
    {
        if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return "{}";
        }

        try
        {
            var contentString = System.Text.Encoding.UTF8.GetString(content);
            var sourceText = Microsoft.CodeAnalysis.Text.SourceText.From(contentString);
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceText);
            
            var metadata = new
            {
                Classes = syntaxTree.GetRoot().DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                    .Select(c => c.Identifier.ValueText)
                    .ToList(),
                Methods = syntaxTree.GetRoot().DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
                    .Select(m => new
                    {
                        Name = m.Identifier.ValueText,
                        ReturnType = m.ReturnType.ToString(),
                        Parameters = m.ParameterList.Parameters
                            .Select(p => new { Name = p.Identifier.ValueText, Type = p.Type?.ToString() })
                            .ToList()
                    })
                    .ToList(),
                Namespaces = syntaxTree.GetRoot().DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax>()
                    .Select(n => n.Name.ToString())
                    .ToList()
            };

            return JsonSerializer.Serialize(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract metadata for file {FilePath}", filePath);
            return "{}";
        }
    }
}
