# Code and Document Migration Mapping

## 📋 Overview

This document provides the exact mapping from your current RepoLens structure to the new organized structure, including all files that need to be moved and empty documents that need to be created.

## 🔄 File Movement Mapping

### **Backend Code Migration**
```powershell
# Current Location → New Location

# .NET Projects
RepoLens.Api/                           → src/backend/RepoLens.Api/
RepoLens.Core/                          → src/backend/RepoLens.Core/
RepoLens.Infrastructure/                → src/backend/RepoLens.Infrastructure/
RepoLens.Worker/                        → src/backend/RepoLens.Worker/
DatabaseQuery/                          → src/backend/DatabaseQuery/
SearchApiDemo/                          → src/backend/SearchApiDemo/

# Tests
RepoLens.Tests/                         → tests/unit/backend/RepoLens.Tests/

# Solution File
RepoLens.sln                            → RepoLens.sln (update paths)
CodeLens.slnx                           → CodeLens.slnx (update paths)
```

### **Frontend Code Migration**
```powershell
# React Application
repolens-ui/                            → src/frontend/repolens-ui/
```

### **Documentation Migration**
```powershell
# Existing Documentation
README.md                               → docs/project-management/README.md
BUILD-AND-START-GUIDE.md               → docs/operations/deployment/build-and-start-guide.md
DEPLOYMENT-GUIDE.md                     → docs/operations/deployment/deployment-guide.md
SystemAudit.md                          → docs/operations/maintenance/system-audit.md
CLINE-STANDING-INSTRUCTIONS.md         → docs/operations/maintenance/cline-instructions.md
CONSOLIDATED_INTEGRATION_ACTION_ITEMS.md → docs/project-management/planning/integration-action-items.md
COMPLETE-BUILD-COMMANDS.md             → docs/operations/deployment/complete-build-commands.md
PULL_REQUEST_TEMPLATE.md               → docs/project-management/planning/pull-request-template.md
deployment-options.md                   → docs/operations/deployment/deployment-options.md

# Existing Design Documentation
repolens-docs/00-architecture/          → docs/architecture/system-design/
repolens-docs/01-screens/               → docs/design/user-interface/screens/
repolens-docs/02-components/            → docs/design/user-interface/components/
repolens-docs/03-interactions/          → docs/design/user-interface/interactions/
repolens-docs/04-design-system/         → docs/design/user-interface/design-system/
repolens-docs/05-compliance/            → docs/security/compliance/
repolens-docs/06-implementation/        → docs/project-management/planning/implementation/

# Operations Documentation
docs/operations/backup-restore.md      → docs/operations/maintenance/backup-restore.md
```

### **Deployment and Infrastructure Migration**
```powershell
# Docker Configuration
docker-compose.yml                      → deployment/docker/production/docker-compose.yml
docker-compose.dev.yml                 → deployment/docker/development/docker-compose.yml
docker-compose.simple.yml              → deployment/docker/development/docker-compose.simple.yml
nginx.conf                              → deployment/docker/production/nginx.conf

# Kubernetes
kong-gateway-setup.yaml                 → deployment/kubernetes/manifests/kong-gateway.yaml
deployment-config.yaml                  → deployment/kubernetes/manifests/deployment-config.yaml

# Scripts
deploy-infrastructure.ps1               → deployment/scripts/deployment/deploy-infrastructure.ps1
start-codelens.ps1                      → deployment/scripts/deployment/start-codelens.ps1
start-codelens-simple.ps1              → deployment/scripts/deployment/start-codelens-simple.ps1
start-dev-services.bat                 → deployment/scripts/development/start-dev-services.bat
start-services.bat                     → deployment/scripts/deployment/start-services.bat
git-status.ps1                         → tools/dev-tools/git-status.ps1
git-upload.ps1                         → tools/dev-tools/git-upload.ps1
query-database.ps1                     → tools/dev-tools/query-database.ps1
ManualDataIngestion.ps1                → tools/dev-tools/manual-data-ingestion.ps1
Monitor-Services.ps1                   → deployment/scripts/monitoring/monitor-services.ps1
```

### **Database and Configuration Migration**
```powershell
# Database
database-schema.sql                     → docs/architecture/database-design/schema.sql
package-lock.json                       → src/frontend/repolens-ui/package-lock.json (already there)

# Configuration Files
.gitignore                              → .gitignore (stays at root)
```

### **Legacy and Archive Migration**
```powershell
# System Audit
system-audit/                           → legacy/original-structure/system-audit/

# Build Outputs (Clean These)
bin/                                    → (delete - regenerated)
obj/                                    → (delete - regenerated)
```

## 📁 Missing Documents to Create

### **Project Management Documents**
```
docs/project-management/requirements/
├── technical-requirements.md           # EMPTY - To be populated
├── user-stories.md                     # EMPTY - To be populated  
└── acceptance-criteria.md              # EMPTY - To be populated

docs/project-management/specifications/
├── api-specifications.md               # EMPTY - To be populated
├── database-schema.md                  # EMPTY - To be populated
├── integration-specs.md                # EMPTY - To be populated
└── security-requirements.md            # EMPTY - To be populated

docs/project-management/planning/
├── roadmap.md                          # EMPTY - To be populated
├── milestones.md                       # EMPTY - To be populated
├── sprint-planning.md                  # EMPTY - To be populated
└── release-notes/                      # EMPTY FOLDER

docs/project-management/decisions/
├── 001-architecture-overview.md        # EMPTY - To be populated
├── 002-technology-stack.md             # EMPTY - To be populated
├── 003-database-design.md              # EMPTY - To be populated
└── 004-api-design.md                   # EMPTY - To be populated
```

### **Architecture Documents**
```
docs/architecture/system-design/
├── component-diagrams.md               # EMPTY - To be populated
├── data-flow-diagrams.md               # EMPTY - To be populated
└── deployment-architecture.md          # EMPTY - To be populated

docs/architecture/api-design/
├── rest-api-design.md                  # EMPTY - To be populated
├── signalr-hubs.md                     # EMPTY - To be populated
├── authentication-flow.md              # EMPTY - To be populated
└── rate-limiting.md                    # EMPTY - To be populated

docs/architecture/database-design/
├── entity-relationships.md             # EMPTY - To be populated
├── indexing-strategy.md                # EMPTY - To be populated
├── migration-strategy.md               # EMPTY - To be populated
└── performance-optimization.md         # EMPTY - To be populated

docs/architecture/integration-patterns/
├── git-provider-integration.md         # EMPTY - To be populated
├── elastic-search-integration.md       # EMPTY - To be populated
├── ast-analysis-pipeline.md            # EMPTY - To be populated
└── real-time-notifications.md          # EMPTY - To be populated
```

### **Design Documents**
```
docs/design/user-experience/
├── user-personas.md                    # EMPTY - To be populated
├── user-journey-maps.md                # EMPTY - To be populated
├── wireframes/                         # EMPTY FOLDER
└── usability-testing.md                # EMPTY - To be populated

docs/design/user-interface/
├── responsive-design.md                # EMPTY - To be populated

docs/design/branding/
├── brand-guidelines.md                 # EMPTY - To be populated
├── color-palette.md                    # EMPTY - To be populated
├── typography.md                       # EMPTY - To be populated
└── iconography.md                      # EMPTY - To be populated
```

### **Security Documents**
```
docs/security/security-policies/
├── data-protection-policy.md           # EMPTY - To be populated
├── access-control-policy.md            # EMPTY - To be populated
├── incident-response-plan.md           # EMPTY - To be populated
└── compliance-requirements.md          # EMPTY - To be populated

docs/security/vulnerability-assessments/
├── security-audit-reports/             # EMPTY FOLDER
├── penetration-testing/                # EMPTY FOLDER
└── security-scanning-results/          # EMPTY FOLDER

docs/security/security-implementations/
├── authentication-implementation.md    # EMPTY - To be populated
├── authorization-matrix.md             # EMPTY - To be populated
├── data-encryption.md                  # EMPTY - To be populated
└── secure-coding-practices.md          # EMPTY - To be populated
```

### **Operations Documents**
```
docs/operations/deployment/
├── environment-setup.md                # EMPTY - To be populated
├── configuration-management.md         # EMPTY - To be populated
└── scaling-strategies.md               # EMPTY - To be populated

docs/operations/monitoring/
├── monitoring-strategy.md              # EMPTY - To be populated
├── alerting-rules.md                   # EMPTY - To be populated
├── performance-metrics.md              # EMPTY - To be populated
└── log-management.md                   # EMPTY - To be populated

docs/operations/maintenance/
├── database-maintenance.md             # EMPTY - To be populated
├── system-updates.md                   # EMPTY - To be populated
└── disaster-recovery.md                # EMPTY - To be populated

docs/operations/troubleshooting/
├── common-issues.md                    # EMPTY - To be populated
├── error-handling.md                   # EMPTY - To be populated
├── performance-issues.md               # EMPTY - To be populated
└── debugging-guide.md                  # EMPTY - To be populated
```

### **Testing Documents**
```
docs/testing/test-strategy/
├── testing-approach.md                 # EMPTY - To be populated
├── test-automation.md                  # EMPTY - To be populated
├── performance-testing.md              # EMPTY - To be populated
└── security-testing.md                 # EMPTY - To be populated

docs/testing/test-cases/
├── functional-tests/                   # EMPTY FOLDER
├── integration-tests/                  # EMPTY FOLDER
├── e2e-tests/                          # EMPTY FOLDER
└── load-tests/                         # EMPTY FOLDER

docs/testing/test-reports/
├── test-execution-reports/             # EMPTY FOLDER
├── coverage-reports/                   # EMPTY FOLDER
└── quality-metrics/                    # EMPTY FOLDER
```

### **API Documents**
```
docs/api/rest-endpoints/
├── portfolio-api.md                    # EMPTY - To be populated
├── repository-api.md                   # EMPTY - To be populated
├── analytics-api.md                    # EMPTY - To be populated
└── search-api.md                       # EMPTY - To be populated

docs/api/realtime-apis/
├── signalr-hubs.md                     # EMPTY - To be populated
├── websocket-events.md                 # EMPTY - To be populated
└── notification-system.md              # EMPTY - To be populated

docs/api/integration-guides/
├── client-sdk.md                       # EMPTY - To be populated
├── webhook-integration.md              # EMPTY - To be populated
└── third-party-apis.md                 # EMPTY - To be populated
```

### **Story Documents**
```
docs/user-experience/stories/feature-stories/
├── dashboard-navigation.md             # EMPTY - To be populated
├── code-quality-metrics.md             # EMPTY - To be populated
├── search-functionality.md             # EMPTY - To be populated
└── notification-system.md              # EMPTY - To be populated

docs/user-experience/stories/user-journey-stories/
├── engineering-manager-workflow.md     # EMPTY - To be populated
├── developer-onboarding.md             # EMPTY - To be populated
├── quality-assessment-flow.md          # EMPTY - To be populated
└── issue-investigation.md              # EMPTY - To be populated

docs/user-experience/stories/acceptance-stories/
├── performance-acceptance.md           # EMPTY - To be populated
├── security-acceptance.md              # EMPTY - To be populated
├── usability-acceptance.md             # EMPTY - To be populated
└── compliance-acceptance.md            # EMPTY - To be populated
```

### **Knowledge Base Documents**
```
docs/knowledge-base/patterns/
├── code-patterns.md                    # EMPTY - To be populated
├── design-patterns.md                  # EMPTY - To be populated
├── integration-patterns.md             # EMPTY - To be populated
└── testing-patterns.md                 # EMPTY - To be populated

docs/knowledge-base/troubleshooting/
├── common-build-errors.md              # EMPTY - To be populated
├── runtime-issues.md                   # EMPTY - To be populated
├── performance-problems.md             # EMPTY - To be populated
└── integration-failures.md             # EMPTY - To be populated

docs/knowledge-base/best-practices/
├── coding-standards.md                 # EMPTY - To be populated
├── git-workflow.md                     # EMPTY - To be populated
├── deployment-practices.md             # EMPTY - To be populated
└── monitoring-guidelines.md            # EMPTY - To be populated

docs/knowledge-base/lessons-learned/
├── architecture-decisions.md           # EMPTY - To be populated
├── refactoring-outcomes.md             # EMPTY - To be populated
├── performance-optimizations.md        # EMPTY - To be populated
└── security-improvements.md            # EMPTY - To be populated
```

## 🚀 Migration Execution Commands

### **Step 1: Create Source Structure**
```powershell
# Create source directories first
mkdir src\backend src\frontend tests\unit\backend
```

### **Step 2: Move Backend Code**
```powershell
# Move .NET projects
Move-Item "RepoLens.Api" "src\backend\"
Move-Item "RepoLens.Core" "src\backend\"  
Move-Item "RepoLens.Infrastructure" "src\backend\"
Move-Item "RepoLens.Worker" "src\backend\"
Move-Item "DatabaseQuery" "src\backend\"
Move-Item "SearchApiDemo" "src\backend\"
Move-Item "RepoLens.Tests" "tests\unit\backend\"
```

### **Step 3: Move Frontend Code**
```powershell
# Move React application
Move-Item "repolens-ui" "src\frontend\"
```

### **Step 4: Move Documentation**
```powershell
# Move existing documentation
Move-Item "README.md" "docs\project-management\"
Move-Item "BUILD-AND-START-GUIDE.md" "docs\operations\deployment\build-and-start-guide.md"
Move-Item "DEPLOYMENT-GUIDE.md" "docs\operations\deployment\deployment-guide.md"
Move-Item "SystemAudit.md" "docs\operations\maintenance\system-audit.md"
Move-Item "CLINE-STANDING-INSTRUCTIONS.md" "docs\operations\maintenance\cline-instructions.md"
Move-Item "CONSOLIDATED_INTEGRATION_ACTION_ITEMS.md" "docs\project-management\planning\integration-action-items.md"
Move-Item "COMPLETE-BUILD-COMMANDS.md" "docs\operations\deployment\complete-build-commands.md"
Move-Item "PULL_REQUEST_TEMPLATE.md" "docs\project-management\planning\pull-request-template.md"
Move-Item "deployment-options.md" "docs\operations\deployment\deployment-options.md"

# Move design documentation
Move-Item "repolens-docs\00-architecture" "docs\architecture\system-design\original"
Move-Item "repolens-docs\01-screens" "docs\design\user-interface\screens"
Move-Item "repolens-docs\02-components" "docs\design\user-interface\components"
Move-Item "repolens-docs\03-interactions" "docs\design\user-interface\interactions" 
Move-Item "repolens-docs\04-design-system" "docs\design\user-interface\design-system"
Move-Item "repolens-docs\05-compliance" "docs\security\compliance"
Move-Item "repolens-docs\06-implementation" "docs\project-management\planning\implementation"

# Move operations documentation
Move-Item "docs\operations\backup-restore.md" "docs\operations\maintenance\backup-restore.md"
```

### **Step 5: Move Deployment Files**
```powershell
# Move Docker configurations
Move-Item "docker-compose.yml" "deployment\docker\production\docker-compose.yml"
Move-Item "docker-compose.dev.yml" "deployment\docker\development\docker-compose.yml"  
Move-Item "docker-compose.simple.yml" "deployment\docker\development\docker-compose.simple.yml"
Move-Item "nginx.conf" "deployment\docker\production\nginx.conf"

# Move Kubernetes configurations
Move-Item "kong-gateway-setup.yaml" "deployment\kubernetes\manifests\kong-gateway.yaml"
Move-Item "deployment-config.yaml" "deployment\kubernetes\manifests\deployment-config.yaml"

# Move deployment scripts
Move-Item "deploy-infrastructure.ps1" "deployment\scripts\deployment\deploy-infrastructure.ps1"
Move-Item "start-codelens.ps1" "deployment\scripts\deployment\start-codelens.ps1"
Move-Item "start-codelens-simple.ps1" "deployment\scripts\deployment\start-codelens-simple.ps1"
Move-Item "start-dev-services.bat" "deployment\scripts\development\start-dev-services.bat"
Move-Item "start-services.bat" "deployment\scripts\deployment\start-services.bat"
Move-Item "Monitor-Services.ps1" "deployment\scripts\monitoring\monitor-services.ps1"

# Move development tools
Move-Item "git-status.ps1" "tools\dev-tools\git-status.ps1"
Move-Item "git-upload.ps1" "tools\dev-tools\git-upload.ps1" 
Move-Item "query-database.ps1" "tools\dev-tools\query-database.ps1"
Move-Item "ManualDataIngestion.ps1" "tools\dev-tools\manual-data-ingestion.ps1"
```

### **Step 6: Move Database Files**
```powershell
# Move database schema
Move-Item "database-schema.sql" "docs\architecture\database-design\schema.sql"
```

### **Step 7: Archive Legacy Files**
```powershell
# Move system audit to legacy
Move-Item "system-audit" "legacy\original-structure\system-audit"

# Archive repolens-docs folder after moving content
Move-Item "repolens-docs" "legacy\original-structure\repolens-docs"
```

### **Step 8: Update Solution File**
```powershell
# Update .sln file to point to new locations
(Get-Content "RepoLens.sln") -replace "RepoLens\.Api\\", "src\backend\RepoLens.Api\" | Set-Content "RepoLens.sln"
(Get-Content "RepoLens.sln") -replace "RepoLens\.Core\\", "src\backend\RepoLens.Core\" | Set-Content "RepoLens.sln"  
(Get-Content "RepoLens.sln") -replace "RepoLens\.Infrastructure\\", "src\backend\RepoLens.Infrastructure\" | Set-Content "RepoLens.sln"
(Get-Content "RepoLens.sln") -replace "RepoLens\.Worker\\", "src\backend\RepoLens.Worker\" | Set-Content "RepoLens.sln"
(Get-Content "RepoLens.sln") -replace "RepoLens\.Tests\\", "tests\unit\backend\RepoLens.Tests\" | Set-Content "RepoLens.sln"
```

Now I'll create all the missing empty documents that need to be populated later.
