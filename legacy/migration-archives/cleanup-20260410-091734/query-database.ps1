# Direct PostgreSQL database query for autonomiccomputing repository
# This script connects directly to the database to check repository status

$connectionString = "Host=localhost;Database=repolens_db;Username=postgres;Password=TCEP;Port=5432"
$autonomicRepoUrl = "https://github.com/Renandgmail/autonomiccomputing.git"
$autonomicRepoUrlAlt = "https://github.com/Renandgmail/autonomiccomputing"

Write-Host "🔍 Direct PostgreSQL Database Query for Autonomic Computing Repository" -ForegroundColor Yellow
Write-Host "Target URL: $autonomicRepoUrl" -ForegroundColor Gray
Write-Host "Database: repolens_db (Main UI Database)" -ForegroundColor Gray
Write-Host ""

try {
    # Load PostgreSQL .NET library
    Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\*\Npgsql.dll" -ErrorAction SilentlyContinue
    
    # Try alternative method using psql if available
    $psqlPath = Get-Command psql -ErrorAction SilentlyContinue
    
    if ($psqlPath) {
        Write-Host "📋 === DIRECT DATABASE QUERY RESULTS ===" -ForegroundColor Green
        Write-Host ""
        
        # Query 1: Search for autonomiccomputing repository
        Write-Host "🔍 Repository Search Results:" -ForegroundColor Cyan
        
        $query1 = @"
SELECT id, name, url, status, type, created_at, last_sync_at, is_local, local_path
FROM repositories 
WHERE url LIKE '%autonomiccomputing%' 
   OR name ILIKE '%autonomic%' 
   OR description ILIKE '%autonomic%'
ORDER BY created_at DESC;
"@

        Write-Host "Searching repositories table..." -ForegroundColor Gray
        $env:PGPASSWORD = "TCEP"
        $repoResult = & psql -h localhost -U postgres -d repolens_db -c $query1 -t -A -F"|"
        
        if ($repoResult -and $repoResult.Length -gt 0 -and $repoResult -ne "") {
            Write-Host "✅ REPOSITORY FOUND:" -ForegroundColor Green
            $lines = $repoResult -split "`n" | Where-Object { $_.Trim() -ne "" }
            foreach ($line in $lines) {
                $parts = $line -split '\|'
                if ($parts.Length -ge 6) {
                    Write-Host "   • ID: $($parts[0])" -ForegroundColor White
                    Write-Host "   • Name: $($parts[1])" -ForegroundColor White
                    Write-Host "   • URL: $($parts[2])" -ForegroundColor White
                    Write-Host "   • Status: $($parts[3])" -ForegroundColor White
                    Write-Host "   • Type: $($parts[4])" -ForegroundColor White
                    Write-Host "   • Created: $($parts[5])" -ForegroundColor White
                    Write-Host "   • Last Sync: $($parts[6])" -ForegroundColor White
                    Write-Host "   • Is Local: $($parts[7])" -ForegroundColor White
                    Write-Host "   • Local Path: $($parts[8])" -ForegroundColor White
                    Write-Host ""
                    
                    $repoId = $parts[0]
                    
                    # Query metrics for this repository
                    Write-Host "📈 Repository Metrics:" -ForegroundColor Cyan
                    $metricsQuery = @"
SELECT total_files, total_lines, total_size, total_commits, total_contributors, 
       quality_score, security_score, maintainability_index, technical_debt,
       language_distribution, created_at
FROM repository_metrics 
WHERE repository_id = $repoId 
ORDER BY created_at DESC 
LIMIT 1;
"@
                    
                    $metricsResult = & psql -h localhost -U postgres -d repolens_db -c $metricsQuery -t -A -F"|"
                    if ($metricsResult -and $metricsResult.Trim() -ne "") {
                        $metricsParts = $metricsResult -split '\|'
                        if ($metricsParts.Length -ge 9) {
                            Write-Host "   • Total Files: $($metricsParts[0])" -ForegroundColor White
                            Write-Host "   • Total Lines: $($metricsParts[1])" -ForegroundColor White
                            Write-Host "   • Total Size: $($metricsParts[2]) bytes" -ForegroundColor White
                            Write-Host "   • Total Commits: $($metricsParts[3])" -ForegroundColor White
                            Write-Host "   • Contributors: $($metricsParts[4])" -ForegroundColor White
                            Write-Host "   • Quality Score: $($metricsParts[5])" -ForegroundColor White
                            Write-Host "   • Security Score: $($metricsParts[6])" -ForegroundColor White
                            Write-Host "   • Maintainability: $($metricsParts[7])" -ForegroundColor White
                            Write-Host "   • Technical Debt: $($metricsParts[8])%" -ForegroundColor White
                            Write-Host "   • Languages: $($metricsParts[9])" -ForegroundColor Gray
                            Write-Host ""
                        }
                    } else {
                        Write-Host "   ❌ No metrics found for this repository" -ForegroundColor Red
                    }
                    
                    # Query vocabulary terms
                    Write-Host "🧠 Vocabulary Terms:" -ForegroundColor Cyan
                    $vocabQuery = @"
SELECT COUNT(*) as total_terms,
       COUNT(*) FILTER (WHERE term_type = 0) as business_terms,
       COUNT(*) FILTER (WHERE term_type = 1) as technical_terms,
       COUNT(*) FILTER (WHERE term_type = 2) as domain_terms
FROM vocabulary_terms 
WHERE repository_id = $repoId;
"@
                    
                    $vocabResult = & psql -h localhost -U postgres -d repolens_db -c $vocabQuery -t -A -F"|"
                    if ($vocabResult -and $vocabResult.Trim() -ne "") {
                        $vocabParts = $vocabResult -split '\|'
                        if ($vocabParts.Length -ge 4) {
                            Write-Host "   • Total Terms: $($vocabParts[0])" -ForegroundColor White
                            Write-Host "   • Business Terms: $($vocabParts[1])" -ForegroundColor White
                            Write-Host "   • Technical Terms: $($vocabParts[2])" -ForegroundColor White
                            Write-Host "   • Domain Terms: $($vocabParts[3])" -ForegroundColor White
                            Write-Host ""
                        }
                    }
                    
                    # Query repository files
                    Write-Host "📁 Repository Files:" -ForegroundColor Cyan
                    $filesQuery = @"
SELECT COUNT(*) as file_count,
       COUNT(DISTINCT language) as language_count
FROM repository_files 
WHERE repository_id = $repoId;
"@
                    
                    $filesResult = & psql -h localhost -U postgres -d repolens_db -c $filesQuery -t -A -F"|"
                    if ($filesResult -and $filesResult.Trim() -ne "") {
                        $filesParts = $filesResult -split '\|'
                        if ($filesParts.Length -ge 2) {
                            Write-Host "   • Total Files: $($filesParts[0])" -ForegroundColor White
                            Write-Host "   • Languages: $($filesParts[1])" -ForegroundColor White
                            Write-Host ""
                        }
                    }
                    
                    # Query code elements
                    Write-Host "🔧 Code Elements:" -ForegroundColor Cyan
                    $codeQuery = @"
SELECT COUNT(*) as element_count,
       COUNT(*) FILTER (WHERE element_type = 0) as class_count,
       COUNT(*) FILTER (WHERE element_type = 1) as method_count,
       COUNT(*) FILTER (WHERE element_type = 2) as interface_count
FROM code_elements ce
JOIN repository_files rf ON ce.file_id = rf.id
WHERE rf.repository_id = $repoId;
"@
                    
                    $codeResult = & psql -h localhost -U postgres -d repolens_db -c $codeQuery -t -A -F"|"
                    if ($codeResult -and $codeResult.Trim() -ne "") {
                        $codeParts = $codeResult -split '\|'
                        if ($codeParts.Length -ge 4) {
                            Write-Host "   • Total Elements: $($codeParts[0])" -ForegroundColor White
                            Write-Host "   • Classes: $($codeParts[1])" -ForegroundColor White
                            Write-Host "   • Methods: $($codeParts[2])" -ForegroundColor White
                            Write-Host "   • Interfaces: $($codeParts[3])" -ForegroundColor White
                            Write-Host ""
                        }
                    }
                }
            }
        } else {
            Write-Host "❌ REPOSITORY NOT FOUND" -ForegroundColor Red
            Write-Host ""
            
            # Show all repositories in database
            Write-Host "📋 All Repositories in Database:" -ForegroundColor Cyan
            $allReposQuery = @"
SELECT id, name, url, status, created_at
FROM repositories 
ORDER BY name;
"@
            
            $allReposResult = & psql -h localhost -U postgres -d repolens_db -c $allReposQuery -t -A -F"|"
            if ($allReposResult -and $allReposResult.Length -gt 0) {
                $allLines = $allReposResult -split "`n" | Where-Object { $_.Trim() -ne "" }
                foreach ($line in $allLines) {
                    $parts = $line -split '\|'
                    if ($parts.Length -ge 5) {
                        Write-Host "   • ID: $($parts[0]) | Name: $($parts[1])" -ForegroundColor White
                        Write-Host "     URL: $($parts[2])" -ForegroundColor Gray
                        Write-Host "     Status: $($parts[3]) | Created: $($parts[4])" -ForegroundColor Gray
                        Write-Host ""
                    }
                }
            } else {
                Write-Host "   ❌ No repositories found in database" -ForegroundColor Red
            }
        }
        
        # Overall database statistics
        Write-Host "📊 === DATABASE SUMMARY ===" -ForegroundColor Green
        
        $statsQuery = @"
SELECT 
    (SELECT COUNT(*) FROM repositories) as total_repos,
    (SELECT COUNT(*) FROM repositories WHERE status = 0) as active_repos,
    (SELECT COUNT(*) FROM repository_metrics) as total_metrics,
    (SELECT COUNT(*) FROM vocabulary_terms) as total_vocab,
    (SELECT COUNT(*) FROM repository_files) as total_files,
    (SELECT COUNT(*) FROM code_elements) as total_code_elements,
    (SELECT COUNT(*) FROM contributor_metrics) as total_contributors,
    (SELECT COUNT(*) FROM users) as total_users;
"@
        
        $statsResult = & psql -h localhost -U postgres -d repolens_db -c $statsQuery -t -A -F"|"
        if ($statsResult -and $statsResult.Trim() -ne "") {
            $statsParts = $statsResult -split '\|'
            if ($statsParts.Length -ge 8) {
                Write-Host "Overall Database Status:" -ForegroundColor Yellow
                Write-Host "   • Repositories: $($statsParts[0]) total, $($statsParts[1]) active" -ForegroundColor White
                Write-Host "   • Metrics: $($statsParts[2]) records" -ForegroundColor White
                Write-Host "   • Vocabulary: $($statsParts[3]) terms" -ForegroundColor White
                Write-Host "   • Files: $($statsParts[4]) indexed" -ForegroundColor White
                Write-Host "   • Code Elements: $($statsParts[5]) parsed" -ForegroundColor White
                Write-Host "   • Contributors: $($statsParts[6]) tracked" -ForegroundColor White
                Write-Host "   • Users: $($statsParts[7]) registered" -ForegroundColor White
                Write-Host ""
                
                $totalRepos = [int]$statsParts[0]
                if ($totalRepos -ge 10) {
                    Write-Host "🎯 Dashboard Display Analysis:" -ForegroundColor Yellow
                    Write-Host "   • Expected UI Count: $totalRepos repositories" -ForegroundColor White
                    Write-Host "   • If UI shows only 10: Check pagination/filtering logic" -ForegroundColor White
                    Write-Host "   • If autonomic repo missing: Check repository status and visibility" -ForegroundColor White
                }
            }
        }
        
    } else {
        Write-Host "❌ psql command not found. Please ensure PostgreSQL client tools are installed." -ForegroundColor Red
        Write-Host "You can install them from: https://www.postgresql.org/download/" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Database query failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Alternative: You can manually query the database using:" -ForegroundColor Yellow
    Write-Host "psql -h localhost -U postgres -d repolens_db" -ForegroundColor Gray
    Write-Host "Password: TCEP" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Then run:" -ForegroundColor Yellow
    Write-Host "SELECT * FROM repositories WHERE url LIKE '%autonomiccomputing%';" -ForegroundColor Gray
}

Write-Host ""
Write-Host "✅ Database query completed" -ForegroundColor Green
