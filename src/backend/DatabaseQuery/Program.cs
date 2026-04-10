using System;
using System.Threading.Tasks;
using Npgsql;

class Program
{
    private const string ConnectionString = "Host=localhost;Database=repolens_db;Username=postgres;Password=TCEP;Port=5432";
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("🔍 Direct Database Query for Autonomic Computing Repository");
        Console.WriteLine("Database: repolens_db (Main UI Database)");
        Console.WriteLine();

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("✅ Connected to database successfully");
            Console.WriteLine();

            // Query 1: Search for autonomiccomputing repository
            Console.WriteLine("🔍 Repository Search Results:");
            var repoQuery = @"
                SELECT id, name, url, status, type, created_at, last_sync_at, is_local, local_path
                FROM repositories 
                WHERE url LIKE '%autonomiccomputing%' 
                   OR name ILIKE '%autonomic%' 
                   OR description ILIKE '%autonomic%'
                ORDER BY created_at DESC";

            using var repoCommand = new NpgsqlCommand(repoQuery, connection);
            using var repoReader = await repoCommand.ExecuteReaderAsync();
            
            bool foundRepository = false;
            int repositoryId = 0;
            
            while (await repoReader.ReadAsync())
            {
                foundRepository = true;
                repositoryId = Convert.ToInt32(repoReader["id"]);
                
                Console.WriteLine("✅ REPOSITORY FOUND:");
                Console.WriteLine($"   • ID: {repoReader["id"]}");
                Console.WriteLine($"   • Name: {repoReader["name"]}");
                Console.WriteLine($"   • URL: {repoReader["url"]}");
                Console.WriteLine($"   • Status: {repoReader["status"]}");
                Console.WriteLine($"   • Type: {repoReader["type"]}");
                Console.WriteLine($"   • Created: {repoReader["created_at"]}");
                Console.WriteLine($"   • Last Sync: {(repoReader["last_sync_at"] == DBNull.Value ? "Never" : repoReader["last_sync_at"].ToString())}");
                Console.WriteLine($"   • Is Local: {repoReader["is_local"]}");
                Console.WriteLine($"   • Local Path: {(repoReader["local_path"] == DBNull.Value ? "Not specified" : repoReader["local_path"].ToString())}");
                Console.WriteLine();
            }
            
            repoReader.Close();
            
            if (foundRepository)
            {
                // Query metrics for this repository
                Console.WriteLine("📈 Repository Metrics:");
                var metricsQuery = @"
                    SELECT total_files, total_lines, total_size, total_commits, total_contributors, 
                           quality_score, security_score, maintainability_index, technical_debt,
                           language_distribution, created_at
                    FROM repository_metrics 
                    WHERE repository_id = @repoId 
                    ORDER BY created_at DESC 
                    LIMIT 1";

                using var metricsCommand = new NpgsqlCommand(metricsQuery, connection);
                metricsCommand.Parameters.AddWithValue("repoId", repositoryId);
                using var metricsReader = await metricsCommand.ExecuteReaderAsync();
                
                if (await metricsReader.ReadAsync())
                {
                    Console.WriteLine($"   • Total Files: {metricsReader["total_files"]}");
                    Console.WriteLine($"   • Total Lines: {metricsReader["total_lines"]}");
                    Console.WriteLine($"   • Total Size: {metricsReader["total_size"]} bytes ({Convert.ToInt64(metricsReader["total_size"]) / 1024.0 / 1024.0:F1} MB)");
                    Console.WriteLine($"   • Total Commits: {metricsReader["total_commits"]}");
                    Console.WriteLine($"   • Contributors: {metricsReader["total_contributors"]}");
                    Console.WriteLine($"   • Quality Score: {Convert.ToDouble(metricsReader["quality_score"]):F1}");
                    Console.WriteLine($"   • Security Score: {Convert.ToDouble(metricsReader["security_score"]):F1}");
                    Console.WriteLine($"   • Maintainability: {Convert.ToDouble(metricsReader["maintainability_index"]):F1}");
                    Console.WriteLine($"   • Technical Debt: {Convert.ToDouble(metricsReader["technical_debt"]):F1}%");
                    Console.WriteLine($"   • Languages: {(metricsReader["language_distribution"] == DBNull.Value ? "Not specified" : metricsReader["language_distribution"].ToString())}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("   ❌ No metrics found for this repository");
                }
                
                metricsReader.Close();
                
                // Query vocabulary terms
                Console.WriteLine("🧠 Vocabulary Terms:");
                var vocabQuery = @"
                    SELECT COUNT(*) as total_terms,
                           COUNT(*) FILTER (WHERE term_type = 0) as business_terms,
                           COUNT(*) FILTER (WHERE term_type = 1) as technical_terms,
                           COUNT(*) FILTER (WHERE term_type = 2) as domain_terms
                    FROM vocabulary_terms 
                    WHERE repository_id = @repoId";

                using var vocabCommand = new NpgsqlCommand(vocabQuery, connection);
                vocabCommand.Parameters.AddWithValue("repoId", repositoryId);
                using var vocabReader = await vocabCommand.ExecuteReaderAsync();
                
                if (await vocabReader.ReadAsync())
                {
                    Console.WriteLine($"   • Total Terms: {vocabReader["total_terms"]}");
                    Console.WriteLine($"   • Business Terms: {vocabReader["business_terms"]}");
                    Console.WriteLine($"   • Technical Terms: {vocabReader["technical_terms"]}");
                    Console.WriteLine($"   • Domain Terms: {vocabReader["domain_terms"]}");
                    Console.WriteLine();
                }
                
                vocabReader.Close();
                
                // Query repository files
                Console.WriteLine("📁 Repository Files:");
                var filesQuery = @"
                    SELECT COUNT(*) as file_count,
                           COUNT(DISTINCT language) as language_count
                    FROM repository_files 
                    WHERE repository_id = @repoId";

                using var filesCommand = new NpgsqlCommand(filesQuery, connection);
                filesCommand.Parameters.AddWithValue("repoId", repositoryId);
                using var filesReader = await filesCommand.ExecuteReaderAsync();
                
                if (await filesReader.ReadAsync())
                {
                    Console.WriteLine($"   • Total Files: {filesReader["file_count"]}");
                    Console.WriteLine($"   • Languages: {filesReader["language_count"]}");
                    Console.WriteLine();
                }
                
                filesReader.Close();
                
                // Query code elements
                Console.WriteLine("🔧 Code Elements:");
                var codeQuery = @"
                    SELECT COUNT(*) as element_count,
                           COUNT(*) FILTER (WHERE element_type = 0) as class_count,
                           COUNT(*) FILTER (WHERE element_type = 1) as method_count,
                           COUNT(*) FILTER (WHERE element_type = 2) as interface_count
                    FROM code_elements ce
                    JOIN repository_files rf ON ce.file_id = rf.id
                    WHERE rf.repository_id = @repoId";

                using var codeCommand = new NpgsqlCommand(codeQuery, connection);
                codeCommand.Parameters.AddWithValue("repoId", repositoryId);
                using var codeReader = await codeCommand.ExecuteReaderAsync();
                
                if (await codeReader.ReadAsync())
                {
                    Console.WriteLine($"   • Total Elements: {codeReader["element_count"]}");
                    Console.WriteLine($"   • Classes: {codeReader["class_count"]}");
                    Console.WriteLine($"   • Methods: {codeReader["method_count"]}");
                    Console.WriteLine($"   • Interfaces: {codeReader["interface_count"]}");
                    Console.WriteLine();
                }
                
                codeReader.Close();
            }
            else
            {
                Console.WriteLine("❌ REPOSITORY NOT FOUND");
                Console.WriteLine();
                
                // Show all repositories in database
                Console.WriteLine("📋 All Repositories in Database:");
                var allReposQuery = @"
                    SELECT id, name, url, status, created_at
                    FROM repositories 
                    ORDER BY name";

                using var allReposCommand = new NpgsqlCommand(allReposQuery, connection);
                using var allReposReader = await allReposCommand.ExecuteReaderAsync();
                
                bool hasRepositories = false;
                while (await allReposReader.ReadAsync())
                {
                    hasRepositories = true;
                    Console.WriteLine($"   • ID: {allReposReader["id"]} | Name: {allReposReader["name"]}");
                    Console.WriteLine($"     URL: {allReposReader["url"]}");
                    Console.WriteLine($"     Status: {allReposReader["status"]} | Created: {allReposReader["created_at"]}");
                    Console.WriteLine();
                }
                
                if (!hasRepositories)
                {
                    Console.WriteLine("   ❌ No repositories found in database");
                }
                
                allReposReader.Close();
            }
            
            // Overall database statistics
            Console.WriteLine("📊 === DATABASE SUMMARY ===");
            
            var statsQuery = @"
                SELECT 
                    (SELECT COUNT(*) FROM repositories) as total_repos,
                    (SELECT COUNT(*) FROM repositories WHERE status = 0) as active_repos,
                    (SELECT COUNT(*) FROM repository_metrics) as total_metrics,
                    (SELECT COUNT(*) FROM vocabulary_terms) as total_vocab,
                    (SELECT COUNT(*) FROM repository_files) as total_files,
                    (SELECT COUNT(*) FROM code_elements) as total_code_elements,
                    (SELECT COUNT(*) FROM contributor_metrics) as total_contributors,
                    (SELECT COUNT(*) FROM users) as total_users";

            using var statsCommand = new NpgsqlCommand(statsQuery, connection);
            using var statsReader = await statsCommand.ExecuteReaderAsync();
            
            if (await statsReader.ReadAsync())
            {
                var totalRepos = Convert.ToInt64(statsReader["total_repos"]);
                var activeRepos = Convert.ToInt64(statsReader["active_repos"]);
                var totalMetrics = Convert.ToInt64(statsReader["total_metrics"]);
                var totalVocab = Convert.ToInt64(statsReader["total_vocab"]);
                var totalFiles = Convert.ToInt64(statsReader["total_files"]);
                var totalCodeElements = Convert.ToInt64(statsReader["total_code_elements"]);
                var totalContributors = Convert.ToInt64(statsReader["total_contributors"]);
                var totalUsers = Convert.ToInt64(statsReader["total_users"]);
                
                Console.WriteLine("Overall Database Status:");
                Console.WriteLine($"   • Repositories: {totalRepos} total, {activeRepos} active");
                Console.WriteLine($"   • Metrics: {totalMetrics} records");
                Console.WriteLine($"   • Vocabulary: {totalVocab} terms");
                Console.WriteLine($"   • Files: {totalFiles} indexed");
                Console.WriteLine($"   • Code Elements: {totalCodeElements} parsed");
                Console.WriteLine($"   • Contributors: {totalContributors} tracked");
                Console.WriteLine($"   • Users: {totalUsers} registered");
                Console.WriteLine();
                
                if (totalRepos >= 10)
                {
                    Console.WriteLine("🎯 Dashboard Display Analysis:");
                    Console.WriteLine($"   • Expected UI Count: {totalRepos} repositories");
                    Console.WriteLine("   • If UI shows only 10: Check pagination/filtering logic");
                    Console.WriteLine("   • If autonomic repo missing: Check repository status and visibility");
                }
            }
            
            statsReader.Close();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database query failed: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("💡 Possible issues:");
            Console.WriteLine("   • PostgreSQL server not running");
            Console.WriteLine("   • Database 'repolens_db' doesn't exist");
            Console.WriteLine("   • Incorrect credentials (postgres/TCEP)");
            Console.WriteLine("   • Connection blocked by firewall");
        }

        Console.WriteLine();
        Console.WriteLine("✅ Database query completed");
    }
}
