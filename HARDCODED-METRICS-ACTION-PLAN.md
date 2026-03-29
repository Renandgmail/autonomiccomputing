# Hardcoded Metrics Analysis & Real Implementation Action Plan

## Executive Summary

This document identifies all hardcoded/placeholder metrics values in the RepoLens system and provides detailed action items for implementing real calculation methods. The current system uses mock data and placeholder values to demonstrate the UI and API structure, but requires real implementation for production use.

## 🔍 Hardcoded Values Identified

### 1. Repository Statistics (RepositoriesController.cs)

#### **HARDCODED VALUES:**
```csharp
// Language Distribution (Line 498)
LanguageDistribution = new Dictionary<string, int>
{
    { "C#", 45 }, { "TypeScript", 25 }, { "JavaScript", 15 }, 
    { "CSS", 10 }, { "HTML", 5 }
}

// Code Quality Metrics
CodeQualityScore = 85.5;        // Line 502
ProjectHealthScore = 92.0;      // Line 503

// Performance Metrics (Lines 846-850)
private static double CalculateCodeDuplication() => 2.1;
private static double CalculateBuildSuccessRate() => 96.8;
private static double CalculateTestCoverage(Repository repository) => 78.4;
private static int CalculateSecurityVulnerabilities() => 2;
private static int CalculateOutdatedDependencies() => 12;

// Language Distribution (Lines 824-837)
return new Dictionary<string, double>
{
    { "TypeScript", 68.4 }, { "JavaScript", 18.7 }, 
    { "CSS", 8.2 }, { "HTML", 3.1 }, { "JSON", 1.6 }
};

// Top Contributors (Lines 856-864)
new List<ContributorInfo>
{
    new() { Name = "Alex Johnson", Commits = 45, Percentage = 35 },
    new() { Name = "Sarah Chen", Commits = 32, Percentage = 25 },
    new() { Name = "Mike Wilson", Commits = 28, Percentage = 22 },
    new() { Name = "Others", Commits = 23, Percentage = 18 }
};

// Activity Patterns (Lines 843-853)
var weekPattern = new[] { 12, 18, 24, 15, 22, 19, 27, 31 };
```

### 2. File Metrics Service (FileMetricsService.cs)

#### **HARDCODED VALUES:**
```csharp
// Security Analysis Patterns
["HARDCODED_SECRETS"] = new List<string>
{
    "password", "secret", "key", "token", "api_key"
}

// Placeholder calculations
result.LinesAdded = 0; // TODO comment indicates missing implementation
```

### 3. Analytics Controller (AnalyticsController.cs)

#### **HARDCODED VALUES:**
```csharp
// Code Graph Placeholder (Line 715)
message = "Code graph analysis requires AST analysis and graph construction to be enabled"
nodes = new List<object>()
edges = new List<object>()
```

### 4. Test Data & Mock Services

#### **EXTENSIVE MOCK DATA:**
- Sample repository data in integration tests
- Mock metrics notification services
- Placeholder file metrics in test fixtures
- Dummy contributor data for testing

## 📋 COMPREHENSIVE ACTION PLAN

### **SPECIAL FOCUS: "Coming Soon" Dashboard Features**

Before diving into the main phases, let's address the specific "coming soon" messages that users currently see:

#### **Action Item 0.1: Contributor Analytics Dashboard (HIGH Priority)**
**Current State:** "Advanced contributor analytics dashboard coming soon"
**Features to Implement:**
```csharp
// 1. Team Collaboration Patterns
public async Task<TeamCollaborationAnalysis> AnalyzeTeamCollaborationAsync(int repositoryId)
{
    var commits = await _commitRepository.GetByRepositoryIdAsync(repositoryId);
    var collaborations = new List<CollaborationPattern>();
    
    // Analyze co-authorship patterns
    foreach (var commit in commits)
    {
        var coAuthors = ExtractCoAuthors(commit.Message);
        if (coAuthors.Any())
        {
            collaborations.Add(new CollaborationPattern
            {
                PrimaryAuthor = commit.AuthorName,
                Collaborators = coAuthors,
                Date = commit.CommitDate,
                FilesInvolved = commit.FilesChanged.Count
            });
        }
    }
    
    // Calculate collaboration metrics
    return new TeamCollaborationAnalysis
    {
        CollaborationFrequency = CalculateCollaborationFrequency(collaborations),
        CrossFunctionalTeams = IdentifyCrossFunctionalTeams(collaborations),
        KnowledgeSharingScore = CalculateKnowledgeSharingScore(collaborations),
        IsolatedContributors = FindIsolatedContributors(commits, collaborations)
    };
}

// 2. Individual Productivity Metrics
public async Task<IndividualProductivityAnalysis> AnalyzeIndividualProductivityAsync(int repositoryId, string contributorEmail)
{
    var commits = await _commitRepository.GetByAuthorEmailAsync(contributorEmail, repositoryId);
    var fileChanges = commits.SelectMany(c => c.FilesChanged).ToList();
    
    return new IndividualProductivityAnalysis
    {
        ContributorEmail = contributorEmail,
        CommitsPerDay = CalculateCommitsPerDay(commits),
        LinesOfCodePerCommit = CalculateLinesPerCommit(commits),
        FilesModifiedPerCommit = CalculateFilesPerCommit(commits),
        CodeReviewParticipation = await AnalyzeCodeReviewParticipationAsync(contributorEmail),
        ExpertiseBreadth = CalculateExpertiseBreadth(fileChanges),
        ConsistencyScore = CalculateActivityConsistency(commits),
        ImpactScore = CalculateContributorImpact(commits, fileChanges)
    };
}

// 3. Knowledge Sharing Analysis
public async Task<KnowledgeSharingAnalysis> AnalyzeKnowledgeSharingAsync(int repositoryId)
{
    var commits = await _commitRepository.GetByRepositoryIdAsync(repositoryId);
    var contributors = commits.GroupBy(c => c.AuthorEmail).ToList();
    
    var knowledgeMap = new Dictionary<string, HashSet<string>>();
    
    foreach (var contributor in contributors)
    {
        var files = contributor.SelectMany(c => c.FilesChanged)
                              .Select(fc => fc.FilePath)
                              .Distinct()
                              .ToHashSet();
        knowledgeMap[contributor.Key] = files;
    }
    
    return new KnowledgeSharingAnalysis
    {
        KnowledgeOverlap = CalculateKnowledgeOverlap(knowledgeMap),
        SinglePointsOfFailure = IdentifySinglePointsOfFailure(knowledgeMap),
        KnowledgeDistribution = CalculateKnowledgeDistribution(knowledgeMap),
        MentorshipOpportunities = IdentifyMentorshipOpportunities(knowledgeMap)
    };
}

// 4. Risk Assessment (Bus Factor)
public async Task<RiskAssessmentAnalysis> AnalyzeRepositoryRisksAsync(int repositoryId)
{
    var busFactor = await CalculateBusFactorAsync(repositoryId);
    var knowledgeSharing = await AnalyzeKnowledgeSharingAsync(repositoryId);
    
    return new RiskAssessmentAnalysis
    {
        OverallRiskLevel = CalculateOverallRisk(busFactor, knowledgeSharing),
        CriticalFiles = busFactor.CriticalFiles,
        VulnerableAreas = IdentifyVulnerableAreas(knowledgeSharing),
        RecommendedActions = GenerateRiskMitigationPlan(busFactor, knowledgeSharing),
        BackupContributors = IdentifyBackupContributors(knowledgeSharing)
    };
}
```

**Dependencies:**
- Git commit history with detailed file change information
- Co-author parsing from commit messages
- Code review system integration (GitHub PRs, Azure DevOps)

**Estimated Effort:** 12-15 days

#### **Action Item 0.2: Security Analysis Dashboard (HIGH Priority)**
**Current State:** "Comprehensive security dashboard coming soon"
**Features to Implement:**
```csharp
// 1. Vulnerability Scanning Results
public async Task<VulnerabilityAnalysisResult> PerformVulnerabilityScanAsync(int repositoryId)
{
    var files = await _fileRepository.GetByRepositoryIdAsync(repositoryId);
    var vulnerabilities = new List<SecurityVulnerability>();
    
    foreach (var file in files)
    {
        var content = await GetFileContentAsync(file);
        
        // Scan for different types of vulnerabilities
        vulnerabilities.AddRange(await ScanForSQLInjectionAsync(file, content));
        vulnerabilities.AddRange(await ScanForXSSVulnerabilitiesAsync(file, content));
        vulnerabilities.AddRange(await ScanForHardcodedSecretsAsync(file, content));
        vulnerabilities.AddRange(await ScanForInsecureCryptographyAsync(file, content));
        vulnerabilities.AddRange(await ScanForPathTraversalAsync(file, content));
    }
    
    return new VulnerabilityAnalysisResult
    {
        TotalVulnerabilities = vulnerabilities.Count,
        CriticalVulnerabilities = vulnerabilities.Count(v => v.Severity == "Critical"),
        HighVulnerabilities = vulnerabilities.Count(v => v.Severity == "High"),
        MediumVulnerabilities = vulnerabilities.Count(v => v.Severity == "Medium"),
        LowVulnerabilities = vulnerabilities.Count(v => v.Severity == "Low"),
        VulnerabilityDetails = vulnerabilities,
        SecurityScore = CalculateSecurityScore(vulnerabilities),
        ComplianceStatus = AssessComplianceStatus(vulnerabilities)
    };
}

// 2. Dependency Security Analysis
public async Task<DependencySecurityAnalysis> AnalyzeDependencySecurityAsync(int repositoryId)
{
    var dependencyFiles = await FindDependencyFilesAsync(repositoryId);
    var securityIssues = new List<DependencySecurityIssue>();
    
    foreach (var file in dependencyFiles)
    {
        switch (Path.GetFileName(file.FilePath).ToLower())
        {
            case "package.json":
                securityIssues.AddRange(await ScanNpmSecurityAsync(file));
                break;
            case "requirements.txt":
                securityIssues.AddRange(await ScanPythonSecurityAsync(file));
                break;
            case var name when name.EndsWith(".csproj"):
                securityIssues.AddRange(await ScanNuGetSecurityAsync(file));
                break;
        }
    }
    
    return new DependencySecurityAnalysis
    {
        VulnerableDependencies = securityIssues.Count,
        CriticalSecurityIssues = securityIssues.Count(s => s.Severity == "Critical"),
        SecurityAdvisories = securityIssues.SelectMany(s => s.Advisories).ToList(),
        RecommendedUpdates = GenerateSecurityUpdateRecommendations(securityIssues),
        ComplianceViolations = IdentifyComplianceViolations(securityIssues)
    };
}

// 3. Code Security Hotspots
public async Task<SecurityHotspotsAnalysis> IdentifySecurityHotspotsAsync(int repositoryId)
{
    var files = await _fileRepository.GetByRepositoryIdAsync(repositoryId);
    var hotspots = new List<SecurityHotspot>();
    
    foreach (var file in files)
    {
        var securityScore = await CalculateFileSecurityScoreAsync(file);
        if (securityScore < 70) // Threshold for hotspot
        {
            hotspots.Add(new SecurityHotspot
            {
                FilePath = file.FilePath,
                SecurityScore = securityScore,
                VulnerabilityTypes = await IdentifyVulnerabilityTypesAsync(file),
                RiskLevel = CalculateRiskLevel(securityScore),
                RecommendedActions = GenerateSecurityRecommendations(file, securityScore)
            });
        }
    }
    
    return new SecurityHotspotsAnalysis
    {
        TotalHotspots = hotspots.Count,
        CriticalHotspots = hotspots.Count(h => h.RiskLevel == "Critical"),
        SecurityHotspots = hotspots.OrderByDescending(h => 100 - h.SecurityScore).ToList(),
        ImprovementPlan = GenerateSecurityImprovementPlan(hotspots)
    };
}

// 4. Compliance Checking
public async Task<ComplianceAnalysisResult> PerformComplianceCheckAsync(int repositoryId, ComplianceStandard standard)
{
    var complianceRules = GetComplianceRules(standard); // OWASP, NIST, etc.
    var violations = new List<ComplianceViolation>();
    
    foreach (var rule in complianceRules)
    {
        var ruleViolations = await CheckComplianceRuleAsync(repositoryId, rule);
        violations.AddRange(ruleViolations);
    }
    
    return new ComplianceAnalysisResult
    {
        ComplianceStandard = standard,
        OverallComplianceScore = CalculateComplianceScore(violations, complianceRules),
        TotalViolations = violations.Count,
        CriticalViolations = violations.Count(v => v.Severity == "Critical"),
        ComplianceViolations = violations,
        RemediationPlan = GenerateRemediationPlan(violations)
    };
}
```

**Dependencies:**
- Security vulnerability databases (CVE, NVD)
- Package security APIs (npm audit, Snyk)
- Compliance framework definitions
- Static analysis security tools integration

**Estimated Effort:** 15-20 days

#### **Action Item 0.3: Dependency Management Dashboard (MEDIUM Priority)**
**Current State:** "Advanced dependency analysis coming soon"
**Features to Implement:**
```csharp
// 1. Dependency Tree Visualization
public async Task<DependencyTreeAnalysis> BuildDependencyTreeAsync(int repositoryId)
{
    var dependencyFiles = await FindDependencyFilesAsync(repositoryId);
    var dependencyTree = new DependencyNode { Name = "Root", Children = new List<DependencyNode>() };
    
    foreach (var file in dependencyFiles)
    {
        var dependencies = await ParseDependenciesAsync(file);
        foreach (var dependency in dependencies)
        {
            var node = await BuildDependencyNodeAsync(dependency, 0, 3); // Max depth 3
            dependencyTree.Children.Add(node);
        }
    }
    
    return new DependencyTreeAnalysis
    {
        DependencyTree = dependencyTree,
        TotalDependencies = CountTotalDependencies(dependencyTree),
        DependencyDepth = CalculateMaxDepth(dependencyTree),
        CircularDependencies = DetectCircularDependencies(dependencyTree),
        UnusedDependencies = await DetectUnusedDependenciesAsync(repositoryId, dependencyTree)
    };
}

// 2. Outdated Package Tracking
public async Task<OutdatedPackageAnalysis> TrackOutdatedPackagesAsync(int repositoryId)
{
    var dependencies = await GetAllDependenciesAsync(repositoryId);
    var outdatedPackages = new List<OutdatedPackage>();
    
    foreach (var dependency in dependencies)
    {
        var currentVersion = dependency.Version;
        var latestVersion = await GetLatestVersionAsync(dependency.Name, dependency.PackageManager);
        
        if (IsOutdated(currentVersion, latestVersion))
        {
            var updateInfo = await AnalyzeUpdateImpactAsync(dependency, latestVersion);
            outdatedPackages.Add(new OutdatedPackage
            {
                Name = dependency.Name,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                VersionsBehind = CalculateVersionsBehind(currentVersion, latestVersion),
                UpdateType = DetermineUpdateType(currentVersion, latestVersion),
                BreakingChanges = updateInfo.HasBreakingChanges,
                SecurityImplications = updateInfo.SecurityImplications,
                UpdatePriority = CalculateUpdatePriority(dependency, updateInfo)
            });
        }
    }
    
    return new OutdatedPackageAnalysis
    {
        OutdatedPackages = outdatedPackages.OrderByDescending(p => p.UpdatePriority).ToList(),
        TotalOutdated = outdatedPackages.Count,
        CriticalUpdates = outdatedPackages.Count(p => p.UpdatePriority >= 8),
        SecurityUpdates = outdatedPackages.Count(p => p.SecurityImplications.Any()),
        UpdateRecommendations = GenerateUpdateRecommendations(outdatedPackages)
    };
}

// 3. License Compatibility Analysis
public async Task<LicenseCompatibilityAnalysis> AnalyzeLicenseCompatibilityAsync(int repositoryId)
{
    var dependencies = await GetAllDependenciesAsync(repositoryId);
    var licenses = new List<DependencyLicense>();
    
    foreach (var dependency in dependencies)
    {
        var license = await GetDependencyLicenseAsync(dependency);
        if (license != null)
        {
            licenses.Add(new DependencyLicense
            {
                DependencyName = dependency.Name,
                LicenseType = license.Type,
                LicenseText = license.Text,
                Commercial = license.AllowCommercialUse,
                Copyleft = license.IsCopyleft,
                Attribution = license.RequiresAttribution
            });
        }
    }
    
    var conflicts = DetectLicenseConflicts(licenses);
    
    return new LicenseCompatibilityAnalysis
    {
        TotalLicenses = licenses.Count,
        UniqueLicenses = licenses.Select(l => l.LicenseType).Distinct().Count(),
        LicenseBreakdown = licenses.GroupBy(l => l.LicenseType).ToDictionary(g => g.Key, g => g.Count()),
        LicenseConflicts = conflicts,
        ComplianceRisk = CalculateLicenseComplianceRisk(licenses, conflicts),
        RecommendedActions = GenerateLicenseRecommendations(conflicts)
    };
}

// 4. Bundle Size Impact Assessment
public async Task<BundleSizeAnalysis> AnalyzeBundleSizeImpactAsync(int repositoryId)
{
    var webPackages = await GetWebDependenciesAsync(repositoryId); // JS/CSS packages
    var bundleAnalysis = new List<PackageSizeImpact>();
    
    foreach (var package in webPackages)
    {
        var sizeInfo = await GetPackageSizeInfoAsync(package);
        bundleAnalysis.Add(new PackageSizeImpact
        {
            PackageName = package.Name,
            UnpackedSize = sizeInfo.UnpackedSize,
            GzippedSize = sizeInfo.GzippedSize,
            Dependencies = sizeInfo.DependencyCount,
            TreeShakeable = await IsTreeShakeableAsync(package),
            AlternativePackages = await FindSmallerAlternativesAsync(package),
            ImpactScore = CalculateSizeImpactScore(sizeInfo)
        });
    }
    
    return new BundleSizeAnalysis
    {
        TotalBundleSize = bundleAnalysis.Sum(b => b.GzippedSize),
        LargestContributors = bundleAnalysis.OrderByDescending(b => b.GzippedSize).Take(10).ToList(),
        OptimizationOpportunities = bundleAnalysis.Where(b => b.AlternativePackages.Any()).ToList(),
        TreeShakeOpportunities = bundleAnalysis.Where(b => !b.TreeShakeable).ToList(),
        SizeOptimizationPlan = GenerateSizeOptimizationPlan(bundleAnalysis)
    };
}
```

**Dependencies:**
- Package registry APIs (npm, NuGet, PyPI)
- License detection tools
- Bundle analysis tools (webpack-bundle-analyzer equivalent)
- Package size databases

**Estimated Effort:** 10-12 days

### **PHASE 1: Core Metrics Collection (Priority: HIGH)**

#### **Action Item 1.1: Language Distribution Analysis**
**Current State:** Hardcoded language percentages
**Implementation Required:**
```csharp
// Real Implementation Method
public async Task<Dictionary<string, double>> CalculateLanguageDistributionAsync(int repositoryId)
{
    // 1. Scan all files in repository
    var files = await _fileRepository.GetByRepositoryIdAsync(repositoryId);
    
    // 2. Detect language by file extension and content analysis
    var languageCounts = new Dictionary<string, long>();
    foreach (var file in files)
    {
        var language = DetectLanguage(file.FilePath, await GetFileContentAsync(file));
        var fileSize = await GetFileSizeAsync(file.FilePath);
        
        if (!languageCounts.ContainsKey(language))
            languageCounts[language] = 0;
        languageCounts[language] += fileSize;
    }
    
    // 3. Calculate percentages
    var totalSize = languageCounts.Values.Sum();
    return languageCounts.ToDictionary(
        kvp => kvp.Key,
        kvp => Math.Round((double)kvp.Value / totalSize * 100, 2)
    );
}

// Supporting methods needed:
private string DetectLanguage(string filePath, string content)
{
    // Use file extension mapping + content analysis
    // Libraries: GitHubLinguist.NET or custom implementation
}
```

**Dependencies:**
- File content reading service
- Language detection library (GitHub Linguist equivalent for .NET)
- File system access for repository clones

**Estimated Effort:** 5-8 days

#### **Action Item 1.2: Code Quality Score Calculation**
**Current State:** Fixed value of 85.5
**Implementation Required:**
```csharp
public async Task<double> CalculateCodeQualityScoreAsync(int repositoryId)
{
    var qualityMetrics = new List<double>();
    
    // 1. Complexity Analysis (25% weight)
    var avgComplexity = await CalculateAverageCyclomaticComplexityAsync(repositoryId);
    qualityMetrics.Add(NormalizeComplexityScore(avgComplexity) * 0.25);
    
    // 2. Code Duplication Analysis (20% weight)
    var duplicationPercent = await CalculateCodeDuplicationAsync(repositoryId);
    qualityMetrics.Add((100 - duplicationPercent) * 0.20);
    
    // 3. Documentation Coverage (20% weight)
    var docCoverage = await CalculateDocumentationCoverageAsync(repositoryId);
    qualityMetrics.Add(docCoverage * 0.20);
    
    // 4. Code Smell Detection (20% weight)
    var smellScore = await CalculateCodeSmellScoreAsync(repositoryId);
    qualityMetrics.Add(smellScore * 0.20);
    
    // 5. Test Coverage (15% weight)
    var testCoverage = await CalculateTestCoverageAsync(repositoryId);
    qualityMetrics.Add(testCoverage * 0.15);
    
    return qualityMetrics.Sum();
}

// Supporting calculations needed:
private async Task<double> CalculateAverageCyclomaticComplexityAsync(int repositoryId)
{
    // Parse AST of code files and calculate cyclomatic complexity
    // Tools: Microsoft.CodeAnalysis.CSharp for C#, Acorn.js for JavaScript
}

private async Task<double> CalculateCodeDuplicationAsync(int repositoryId)
{
    // Use algorithms like:
    // 1. Token-based comparison
    // 2. AST similarity analysis
    // 3. String matching with normalization
}
```

**Dependencies:**
- AST parsing libraries (Microsoft.CodeAnalysis for C#, Babel/Acorn for JS)
- Code duplication detection algorithms
- Documentation parsing (XML docs, JSDoc, etc.)

**Estimated Effort:** 10-15 days

#### **Action Item 1.3: Real Test Coverage Calculation**
**Current State:** Hardcoded 78.4%
**Implementation Required:**
```csharp
public async Task<double> CalculateTestCoverageAsync(int repositoryId)
{
    // 1. Identify test files by convention and content
    var testFiles = await IdentifyTestFilesAsync(repositoryId);
    var sourceFiles = await GetSourceFilesAsync(repositoryId);
    
    // 2. Analyze test-to-source relationships
    var coverage = new Dictionary<string, CoverageInfo>();
    
    foreach (var testFile in testFiles)
    {
        var referencedFiles = await AnalyzeTestReferencesAsync(testFile);
        foreach (var sourceFile in referencedFiles)
        {
            if (!coverage.ContainsKey(sourceFile))
                coverage[sourceFile] = new CoverageInfo();
            coverage[sourceFile].TestCount++;
        }
    }
    
    // 3. Calculate coverage percentage
    var coveredFiles = coverage.Count;
    var totalFiles = sourceFiles.Count();
    
    return Math.Round((double)coveredFiles / totalFiles * 100, 2);
}

// Pattern-based test identification:
private async Task<IEnumerable<RepositoryFile>> IdentifyTestFilesAsync(int repositoryId)
{
    var testPatterns = new[]
    {
        @".*[Tt]est.*\.(cs|js|ts)$",
        @".*\.test\.(js|ts)$",
        @".*\.spec\.(js|ts)$",
        @".*/tests?/.*\.(cs|js|ts)$",
        @".*/specs?/.*\.(cs|js|ts)$"
    };
    
    // Apply patterns to file paths
}
```

**Dependencies:**
- File pattern matching
- Import/reference analysis for different languages
- AST parsing to find test method references

**Estimated Effort:** 8-12 days

### **PHASE 2: Advanced Analytics (Priority: MEDIUM)**

#### **Action Item 2.1: Security Vulnerability Detection**
**Current State:** Hardcoded count of 2
**Implementation Required:**
```csharp
public async Task<SecurityAnalysisResult> AnalyzeSecurityVulnerabilitiesAsync(int repositoryId)
{
    var vulnerabilities = new List<SecurityIssue>();
    var files = await _fileRepository.GetByRepositoryIdAsync(repositoryId);
    
    foreach (var file in files)
    {
        var content = await GetFileContentAsync(file);
        
        // 1. Hardcoded secrets detection
        vulnerabilities.AddRange(await DetectHardcodedSecretsAsync(file, content));
        
        // 2. SQL injection patterns
        vulnerabilities.AddRange(await DetectSqlInjectionAsync(file, content));
        
        // 3. XSS vulnerabilities (for web files)
        if (IsWebFile(file.FilePath))
            vulnerabilities.AddRange(await DetectXssVulnerabilitiesAsync(file, content));
        
        // 4. Insecure cryptography
        vulnerabilities.AddRange(await DetectCryptographyIssuesAsync(file, content));
    }
    
    return new SecurityAnalysisResult
    {
        TotalVulnerabilities = vulnerabilities.Count,
        CriticalCount = vulnerabilities.Count(v => v.Severity == "Critical"),
        HighCount = vulnerabilities.Count(v => v.Severity == "High"),
        MediumCount = vulnerabilities.Count(v => v.Severity == "Medium"),
        LowCount = vulnerabilities.Count(v => v.Severity == "Low"),
        Issues = vulnerabilities
    };
}

// Pattern-based detection methods:
private async Task<IEnumerable<SecurityIssue>> DetectHardcodedSecretsAsync(RepositoryFile file, string content)
{
    var secretPatterns = new Dictionary<string, string>
    {
        ["AWS Access Key"] = @"AKIA[0-9A-Z]{16}",
        ["API Key"] = @"api[_-]?key['\""]*\s*[:=]\s*['\""]*[a-zA-Z0-9]{20,}",
        ["Password"] = @"password['\""]*\s*[:=]\s*['\""]*[^'\""]{8,}",
        ["Private Key"] = @"-----BEGIN (RSA )?PRIVATE KEY-----"
    };
    
    var issues = new List<SecurityIssue>();
    foreach (var pattern in secretPatterns)
    {
        var matches = Regex.Matches(content, pattern.Value, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            issues.Add(new SecurityIssue
            {
                Type = pattern.Key,
                Severity = "High",
                FilePath = file.FilePath,
                LineNumber = GetLineNumber(content, match.Index),
                Description = $"Potential {pattern.Key} found in source code"
            });
        }
    }
    return issues;
}
```

**Dependencies:**
- Regex pattern libraries for security detection
- Integration with security scanning tools (Bandit, ESLint security, etc.)
- OWASP vulnerability database

**Estimated Effort:** 12-20 days

#### **Action Item 2.2: Build Success Rate Tracking**
**Current State:** Hardcoded 96.8%
**Implementation Required:**
```csharp
public async Task<BuildMetrics> CalculateBuildSuccessRateAsync(int repositoryId)
{
    // Integration with CI/CD systems
    var repository = await _repositoryRepository.GetByIdAsync(repositoryId);
    var buildData = new List<BuildResult>();
    
    // 1. GitHub Actions integration
    if (IsGitHubRepository(repository.Url))
    {
        buildData.AddRange(await GetGitHubActionResultsAsync(repository));
    }
    
    // 2. Azure DevOps integration
    else if (IsAzureDevOpsRepository(repository.Url))
    {
        buildData.AddRange(await GetAzureDevOpsResultsAsync(repository));
    }
    
    // 3. Jenkins integration (if configured)
    else if (HasJenkinsConfiguration(repository))
    {
        buildData.AddRange(await GetJenkinsResultsAsync(repository));
    }
    
    // 4. Local build simulation (fallback)
    else
    {
        buildData.AddRange(await SimulateBuildFromProjectFiles(repository));
    }
    
    return CalculateBuildMetrics(buildData);
}

// CI/CD API Integration methods:
private async Task<IEnumerable<BuildResult>> GetGitHubActionResultsAsync(Repository repository)
{
    // GitHub REST API: GET /repos/{owner}/{repo}/actions/runs
    var client = _httpClientFactory.CreateClient("GitHubAPI");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _gitHubToken);
    
    var response = await client.GetAsync($"repos/{owner}/{repo}/actions/runs");
    // Parse workflow runs and extract build results
}
```

**Dependencies:**
- CI/CD system APIs (GitHub Actions, Azure DevOps, Jenkins)
- HTTP clients with authentication
- Build file parsers (package.json, *.csproj, Makefile)

**Estimated Effort:** 8-15 days

### **PHASE 3: Contributor Analytics (Priority: MEDIUM)**

#### **Action Item 3.1: Real Contributor Analysis**
**Current State:** Hardcoded contributor list with fake names
**Implementation Required:**
```csharp
public async Task<List<ContributorInfo>> AnalyzeRealContributorsAsync(int repositoryId)
{
    // 1. Get commit history from Git
    var commits = await _commitRepository.GetByRepositoryIdAsync(repositoryId);
    
    // 2. Aggregate contributor statistics
    var contributorStats = commits
        .GroupBy(c => new { c.AuthorEmail, c.AuthorName })
        .Select(g => new ContributorAnalysis
        {
            Name = g.Key.AuthorName,
            Email = g.Key.AuthorEmail,
            CommitCount = g.Count(),
            FirstCommit = g.Min(c => c.CommitDate),
            LastCommit = g.Max(c => c.CommitDate),
            LinesAdded = g.Sum(c => c.LinesAdded),
            LinesDeleted = g.Sum(c => c.LinesDeleted),
            FilesModified = g.SelectMany(c => c.FilesChanged).Distinct().Count()
        })
        .OrderByDescending(c => c.CommitCount)
        .ToList();
    
    // 3. Calculate advanced metrics
    var totalCommits = contributorStats.Sum(c => c.CommitCount);
    var result = new List<ContributorInfo>();
    
    foreach (var contributor in contributorStats)
    {
        result.Add(new ContributorInfo
        {
            Name = contributor.Name,
            Email = contributor.Email,
            Commits = contributor.CommitCount,
            Percentage = Math.Round((double)contributor.CommitCount / totalCommits * 100, 1),
            LinesAdded = contributor.LinesAdded,
            LinesDeleted = contributor.LinesDeleted,
            FilesModified = contributor.FilesModified,
            ActivityLevel = CalculateActivityLevel(contributor),
            ExpertiseAreas = await IdentifyExpertiseAreasAsync(contributor, repositoryId),
            Tenure = CalculateTenure(contributor.FirstCommit),
            RecentActivity = CalculateRecentActivity(contributor.LastCommit)
        });
    }
    
    return result;
}

// Advanced contributor analysis:
private ActivityLevel CalculateActivityLevel(ContributorAnalysis contributor)
{
    var daysBetween = (contributor.LastCommit - contributor.FirstCommit).TotalDays;
    if (daysBetween == 0) daysBetween = 1;
    
    var commitsPerDay = contributor.CommitCount / daysBetween;
    
    return commitsPerDay switch
    {
        >= 1.0 => ActivityLevel.VeryActive,
        >= 0.5 => ActivityLevel.Active,
        >= 0.1 => ActivityLevel.Moderate,
        _ => ActivityLevel.Occasional
    };
}

private async Task<List<string>> IdentifyExpertiseAreasAsync(ContributorAnalysis contributor, int repositoryId)
{
    // Analyze file types and directories most commonly modified
    var commits = await _commitRepository.GetByAuthorEmailAsync(contributor.Email, repositoryId);
    var fileChanges = commits.SelectMany(c => c.FilesChanged).ToList();
    
    var expertiseAreas = fileChanges
        .GroupBy(fc => GetFileCategory(fc.FilePath))
        .OrderByDescending(g => g.Count())
        .Take(3)
        .Select(g => g.Key)
        .ToList();
    
    return expertiseAreas;
}
```

**Dependencies:**
- Git history parsing (LibGit2Sharp)
- Commit data enrichment with file change details
- Domain expertise categorization algorithms

**Estimated Effort:** 10-14 days

#### **Action Item 3.2: Bus Factor Calculation**
**Current State:** Simple approximation
**Implementation Required:**
```csharp
public async Task<BusFactorAnalysis> CalculateBusFactorAsync(int repositoryId)
{
    var contributors = await AnalyzeRealContributorsAsync(repositoryId);
    var fileChanges = await _commitRepository.GetFileChangesByRepositoryAsync(repositoryId);
    
    // 1. Calculate knowledge distribution by file
    var fileOwnership = new Dictionary<string, Dictionary<string, int>>();
    
    foreach (var change in fileChanges)
    {
        if (!fileOwnership.ContainsKey(change.FilePath))
            fileOwnership[change.FilePath] = new Dictionary<string, int>();
        
        if (!fileOwnership[change.FilePath].ContainsKey(change.AuthorEmail))
            fileOwnership[change.FilePath][change.AuthorEmail] = 0;
        
        fileOwnership[change.FilePath][change.AuthorEmail] += change.LinesChanged;
    }
    
    // 2. Calculate bus factor for each file
    var fileBusFactors = new List<FileBusFactor>();
    
    foreach (var file in fileOwnership)
    {
        var totalChanges = file.Value.Values.Sum();
        var sortedContributors = file.Value
            .OrderByDescending(kvp => kvp.Value)
            .ToList();
        
        // Find minimum contributors that represent 50% of changes
        var runningTotal = 0;
        var busFactor = 0;
        var threshold = totalChanges * 0.5;
        
        foreach (var contributor in sortedContributors)
        {
            busFactor++;
            runningTotal += contributor.Value;
            if (runningTotal >= threshold) break;
        }
        
        fileBusFactors.Add(new FileBusFactor
        {
            FilePath = file.Key,
            BusFactor = busFactor,
            PrimaryOwner = sortedContributors.First().Key,
            OwnershipPercentage = Math.Round((double)sortedContributors.First().Value / totalChanges * 100, 1)
        });
    }
    
    // 3. Calculate overall repository bus factor
    var overallBusFactor = Math.Round(fileBusFactors.Average(f => f.BusFactor), 1);
    var criticalFiles = fileBusFactors.Where(f => f.BusFactor == 1).ToList();
    
    return new BusFactorAnalysis
    {
        OverallBusFactor = overallBusFactor,
        CriticalFiles = criticalFiles,
        RiskLevel = CalculateRiskLevel(overallBusFactor, criticalFiles.Count),
        Recommendations = GenerateBusFactorRecommendations(criticalFiles)
    };
}
```

**Estimated Effort:** 6-10 days

### **PHASE 4: Activity Pattern Analysis (Priority: LOW)**

#### **Action Item 4.1: Real Activity Patterns**
**Current State:** Hardcoded weekly pattern array
**Implementation Required:**
```csharp
public async Task<ActivityPatternAnalysis> AnalyzeActivityPatternsAsync(int repositoryId)
{
    var commits = await _commitRepository.GetByRepositoryIdAsync(repositoryId);
    
    // 1. Hourly patterns
    var hourlyActivity = commits
        .GroupBy(c => c.CommitDate.Hour)
        .ToDictionary(g => $"Hour{g.Key:D2}", g => g.Count());
    
    // 2. Daily patterns (day of week)
    var dailyActivity = commits
        .GroupBy(c => c.CommitDate.DayOfWeek)
        .ToDictionary(g => g.Key.ToString(), g => g.Count());
    
    // 3. Monthly trends
    var monthlyActivity = commits
        .Where(c => c.CommitDate > DateTime.UtcNow.AddMonths(-12))
        .GroupBy(c => new { c.CommitDate.Year, c.CommitDate.Month })
        .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Count());
    
    // 4. Seasonal analysis
    var seasonalTrends = AnalyzeSeasonalTrends(commits);
    
    return new ActivityPatternAnalysis
    {
        HourlyDistribution = hourlyActivity,
        DailyDistribution = dailyActivity,
        MonthlyTrends = monthlyActivity,
        SeasonalTrends = seasonalTrends,
        PeakActivityHour = GetPeakActivityHour(hourlyActivity),
        MostActiveDay = GetMostActiveDay(dailyActivity),
        ActivityConsistency = CalculateActivityConsistency(commits)
    };
}
```

**Estimated Effort:** 4-6 days

### **PHASE 5: Dependency and Maintenance Analysis (Priority: LOW)**

#### **Action Item 5.1: Outdated Dependencies Detection**
**Current State:** Hardcoded count of 12
**Implementation Required:**
```csharp
public async Task<DependencyAnalysis> AnalyzeOutdatedDependenciesAsync(int repositoryId)
{
    var repository = await _repositoryRepository.GetByIdAsync(repositoryId);
    var dependencyFiles = await FindDependencyFilesAsync(repository);
    var outdatedDependencies = new List<OutdatedDependency>();
    
    foreach (var file in dependencyFiles)
    {
        switch (Path.GetFileName(file.FilePath).ToLower())
        {
            case "package.json":
                outdatedDependencies.AddRange(await AnalyzeNpmDependenciesAsync(file));
                break;
            case "requirements.txt":
                outdatedDependencies.AddRange(await AnalyzePythonDependenciesAsync(file));
                break;
            case var name when name.EndsWith(".csproj"):
                outdatedDependencies.AddRange(await AnalyzeNuGetDependenciesAsync(file));
                break;
            case "composer.json":
                outdatedDependencies.AddRange(await AnalyzeComposerDependenciesAsync(file));
                break;
        }
    }
    
    return new DependencyAnalysis
    {
        TotalDependencies = await CountTotalDependenciesAsync(dependencyFiles),
        OutdatedCount = outdatedDependencies.Count,
        SecurityVulnerabilities = outdatedDependencies.Count(d => d.HasSecurityIssues),
        MajorUpdatesAvailable = outdatedDependencies.Count(d => d.UpdateType == UpdateType.Major),
        OutdatedDependencies = outdatedDependencies
    };
}

// Package manager specific analysis:
private async Task<IEnumerable<OutdatedDependency>> AnalyzeNpmDependenciesAsync(RepositoryFile packageJsonFile)
{
    var content = await GetFileContentAsync(packageJsonFile);
    var packageJson = JsonSerializer.Deserialize<PackageJsonModel>(content);
    var outdated = new List<OutdatedDependency>();
    
    foreach (var dependency in packageJson.Dependencies)
    {
        var latestVersion = await GetLatestNpmVersionAsync(dependency.Key);
        var currentVersion = CleanVersionString(dependency.Value);
        
        if (IsOutdated(currentVersion, latestVersion))
        {
            var vulnerabilities = await CheckNpmVulnerabilitiesAsync(dependency.Key, currentVersion);
            
            outdated.Add(new OutdatedDependency
            {
                PackageName = dependency.Key,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                UpdateType = DetermineUpdateType(currentVersion, latestVersion),
                HasSecurityIssues = vulnerabilities.Any(),
                SecurityIssues = vulnerabilities
            });
        }
    }
    
    return outdated;
}
```

**Dependencies:**
- Package manager APIs (npm, NuGet, PyPI, Packagist)
- Vulnerability databases (Snyk, GitHub Security Advisory)
- Version comparison algorithms

**Estimated Effort:** 10-15 days

## 🏗️ IMPLEMENTATION ROADMAP

### **Sprint 1 (Weeks 1-2): Foundation**
- Set up AST parsing infrastructure
- Implement language detection service
- Create file content reading abstraction
- Basic metrics calculation framework

### **Sprint 2 (Weeks 3-4): Core Quality Metrics**
- Language distribution calculation
- Code complexity analysis
- Basic test coverage detection
- Code duplication detection

### **Sprint 3 (Weeks 5-6): Security & Build Integration**
- Security vulnerability scanning
- CI/CD system integrations
- Build success rate tracking
- Documentation coverage analysis

### **Sprint 4 (Weeks 7-8): Advanced Analytics**
- Real contributor analysis
- Bus factor calculation
- Activity pattern analysis
- Performance optimization

### **Sprint 5 (Weeks 9-10): Dependencies & Polish**
- Dependency analysis
- Outdated package detection
- Security vulnerability database integration
- Performance optimization and testing

## 📊 CURRENT HARDCODED VALUES TO REPLACE

| Metric | Current Hardcoded Value | Real Calculation Status | Priority |
|--------|------------------------|------------------------|----------|
| **Language Distribution** | Fixed percentages | ❌ Not implemented | HIGH |
| **Code Quality Score** | 85.5 | ❌ Not implemented | HIGH |
| **Test Coverage** | 78.4% | ❌ Not implemented | HIGH |
| **Build Success Rate** | 96.8% | ❌ Not implemented | MEDIUM |
| **Security Vulnerabilities** | 2 | ❌ Not implemented | MEDIUM |
| **Outdated Dependencies** | 12 | ❌ Not implemented | LOW |
| **Code Duplication** | 2.1% | ❌ Not implemented | MEDIUM |
| **Technical Debt** | File count * 0.1 | ❌ Not implemented | MEDIUM |
| **Contributor Names** | "Alex Johnson", "Sarah Chen" | ❌ Not implemented | HIGH |
| **Activity Patterns** | Fixed week pattern | ❌ Not implemented | LOW |
| **Bus Factor** | Simple approximation | ❌ Not implemented | MEDIUM |

## 🔧 REQUIRED DEPENDENCIES

### **NuGet Packages:**
```xml
<!-- Code Analysis -->
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
<PackageReference Include="Microsoft.CodeAnalysis.VisualBasic" Version="4.8.0" />

<!-- Git Integration -->
<PackageReference Include="LibGit2Sharp" Version="0.29.0" />

<!-- HTTP Clients for API Integration -->
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />

<!-- JSON Processing -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- Language Detection -->
<PackageReference Include="GitHubLinguist" Version="1.0.0" />

<!-- Security Scanning -->
<PackageReference Include="SecurityCodeScan" Version="5.6.7" />
```

### **External APIs Required:**
- GitHub REST API (for GitHub repositories)
- Azure DevOps REST API (for Azure repos)
- NPM Registry API (for dependency analysis)
- NuGet API (for .NET dependency analysis)
- PyPI API (for Python dependency analysis)
- CVE Database API (for security vulnerabilities)

### **Infrastructure Dependencies:**
- File system access for repository cloning
- Temporary storage for analysis workspaces
- Background job processing (Hangfire or similar)
- Caching layer (Redis) for API responses

## 🎯 SUCCESS CRITERIA

### **Phase 1 Complete:**
- ✅ Language distribution shows real file analysis results
- ✅ Code quality scores based on actual complexity metrics
- ✅ Test coverage reflects actual test file presence
- ✅ No more hardcoded language percentages

### **Phase 2 Complete:**
- ✅ Security vulnerabilities show real pattern detection
- ✅ Build success rates integrate with actual CI/CD systems
- ✅ Contributor data shows real Git commit authors
- ✅ All mock contributor names removed

### **Phase 3 Complete:**
- ✅ Activity patterns reflect real commit timing
- ✅ Bus factor based on actual code ownership analysis
- ✅ Dependency analysis shows real package versions
- ✅ All placeholder values replaced with calculations

### **Quality Gates:**
- **Accuracy**: Metrics must match manual verification within 95%
- **Performance**: Analysis must complete within 5 minutes for repos under 1000 files
- **Reliability**: System must handle analysis failures gracefully
- **Scalability**: Must support concurrent analysis of multiple repositories

## 📋 TEAM ASSIGNMENTS

### **Backend Team (4 developers):**
- **Developer 1**: Language detection & file analysis
- **Developer 2**: Code quality metrics & complexity analysis  
- **Developer 3**: Security scanning & vulnerability detection
- **Developer 4**: CI/CD integration & build metrics

### **DevOps Team (1 developer):**
- Infrastructure setup for analysis environments
- API integrations configuration
- Performance monitoring and optimization

### **QA Team (2 testers):**
- Manual verification of calculated metrics
- Integration testing with real repositories
- Performance and load testing

## 📈 ESTIMATED TIMELINE

| Phase | Duration | Deliverables |
|-------|----------|-------------|
| **Planning & Setup** | 1 week | Infrastructure, dependencies, architecture |
| **Phase 1: Core Metrics** | 2 weeks | Language distribution, code quality basics |
| **Phase 2: Security & CI** | 2 weeks | Security scanning, build integration |
| **Phase 3: Contributors** | 2 weeks | Real contributor analysis, bus factor |
| **Phase 4: Polish** | 1 week | Activity patterns, dependencies |
| **Testing & Deployment** | 2 weeks | Integration testing, performance optimization |
| **TOTAL** | **10 weeks** | Complete real metrics implementation |

## 💡 CONCLUSION

The RepoLens system currently uses extensive hardcoded values to demonstrate UI functionality and API structure. This comprehensive action plan provides the roadmap to replace all placeholder data with real, calculated metrics that provide genuine insights into repository health, code quality, and team dynamics.

The implementation will transform RepoLens from a UI prototype into a production-ready code intelligence platform that rivals tools like SonarQube, CodeClimate, and GitHub Insights.

**Priority order for maximum business impact:**
1. **Language Distribution & Code Quality** (immediate user value)
2. **Real Contributor Analysis** (team insights)
3. **Security Vulnerability Detection** (compliance requirements)
4. **CI/CD Integration** (DevOps workflows)
5. **Advanced Analytics** (strategic insights)

Each phase builds upon the previous one, allowing for incremental deployment and user feedback integration throughout the development process.
