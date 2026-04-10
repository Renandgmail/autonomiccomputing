# RepoLens Project Structure Guide

## 🎯 Overview

This guide explains the new comprehensive folder structure for the RepoLens project. The reorganization maintains all existing functionality while creating clear boundaries for documentation, agent workflows, and future development.

## 📁 Quick Navigation

```
repolens-enterprise/
├── 📚 docs/                     # Master documentation hub
├── 🤖 agents/                   # Agentic workflow management  
├── 🏠 src/                      # Source code (to be created)
├── 🧪 tests/                    # Test suite (to be created)
├── 🚀 deployment/               # Deployment & infrastructure
├── 📦 tools/                    # Development tools
├── 📋 project-management/       # Project management artifacts
└── 📚 legacy/                   # Legacy files during transition
```

## 🎯 How to Use This Structure

### 📚 Documentation (`docs/`)

**Purpose**: Centralized documentation hub for all project knowledge

**Key Folders**:
- `docs/project-management/` - Requirements, specifications, planning
- `docs/architecture/` - Technical architecture and design
- `docs/design/` - UX/UI design documentation
- `docs/security/` - Security policies and implementations
- `docs/operations/` - DevOps and operational procedures
- `docs/testing/` - Testing strategies and documentation
- `docs/api/` - API documentation

**Usage Guidelines**:
- All master documentation goes here
- Use placeholder files to identify missing documentation
- Cross-reference with agent outputs but maintain as authoritative source
- Follow established templates and naming conventions

### 🤖 Agents (`agents/`)

**Purpose**: Agentic workflow management and automation

**Key Folders**:
- `agents/workflows/` - Agent workflow definitions
- `agents/templates/` - Document templates for agents
- `agents/tools/` - Agent utility tools and scripts
- `agents/outputs/` - Agent-generated outputs and reports
- `agents/workspace/` - Temporary agent workspace

**Usage Guidelines**:
- All agent activities must follow `agents/workflows/AGENT_WORKFLOW_RULES.md`
- Agents create reports using templates from `agents/templates/`
- Agent outputs complement but never replace master documentation
- Use workspace for temporary files with automatic cleanup

### 🏠 Source Code (`src/` - To Be Created)

**Future Structure**:
```
src/
├── backend/          # Backend applications (RepoLens.*)
├── frontend/         # Frontend applications (repolens-ui)
├── shared/           # Shared libraries and utilities
└── integrations/     # External integrations
```

**Migration Note**: Source code will be moved here during Phase 2 of migration

### 🚀 Deployment (`deployment/`)

**Purpose**: All deployment and infrastructure configurations

**Key Folders**:
- `deployment/docker/` - Docker configurations by environment
- `deployment/kubernetes/` - K8s manifests and helm charts
- `deployment/infrastructure/` - Infrastructure as Code
- `deployment/scripts/` - Deployment and maintenance scripts
- `deployment/environments/` - Environment-specific configurations

**Usage Guidelines**:
- Organize by environment (development, staging, production)
- Keep environment-specific configurations separate
- Document all deployment procedures

### 📦 Tools (`tools/`)

**Purpose**: Development tools and utilities

**Key Folders**:
- `tools/build/` - Build tools and scripts
- `tools/dev-tools/` - Development utilities
- `tools/generators/` - Code generators
- `tools/analyzers/` - Static analysis tools
- `tools/migration/` - Migration utilities

## 🤖 Agent Workflow Integration

### Agent Success Rules

When an agent completes a task successfully, it MUST:

1. **Update Progress Documentation**
2. **Create Completion Report** in `agents/outputs/reports/`
3. **Update Relevant Documentation** in `docs/`
4. **Archive Working Files** to `agents/workspace/archive/`

### Agent Quality Gates

Before marking any task complete, agents must:
- [ ] Verify all documentation is up-to-date
- [ ] Run quality checks on generated content
- [ ] Cross-reference with existing documentation
- [ ] Generate summary report
- [ ] Archive working files

### Agent Workspace Management

```
agents/workspace/
├── drafts/          # Work in progress (auto-clean after 7 days)
├── scratch/         # Temporary calculations (auto-clean after 1 day)  
├── processing/      # Active tasks (auto-clean after 3 days)
└── archive/         # Completed work (retain 30 days)
```

## 📋 Migration Process

### Current Status
✅ **COMPLETED**:
- New folder structure created
- Agent workflow rules defined
- Documentation templates created
- Migration guide prepared

⏳ **NEXT STEPS**:
- Copy existing files to new structure (Phase 1)
- Move source code to `src/` folder (Phase 2)
- Set up agent automation (Phase 3)
- Clean up old structure (Phase 4)

### Migration Commands

Follow the detailed commands in `MIGRATION_GUIDE.md` for step-by-step migration instructions.

## 🎯 Key Benefits

1. **Clear Separation of Concerns**: Code, docs, tests, deployment are clearly separated
2. **Agent-Friendly**: Dedicated spaces for agent workflows and outputs  
3. **Scalable**: Structure supports growth and additional components
4. **Maintainable**: Logical grouping makes finding and updating files easier
5. **Professional**: Enterprise-grade organization suitable for large teams
6. **Documentation-Driven**: Comprehensive documentation structure with placeholders
7. **Automation-Ready**: Structure supports CI/CD and automated workflows

## 📚 Key Documents

- `PROJECT_STRUCTURE_REORGANIZATION.md` - Comprehensive reorganization plan
- `MIGRATION_GUIDE.md` - Step-by-step migration instructions
- `agents/workflows/AGENT_WORKFLOW_RULES.md` - Agent workflow rules and guidelines
- `agents/templates/agent-completion-report-template.md` - Agent report template
- `docs/project-management/requirements/business-requirements.md` - Business requirements placeholder

## 🚀 Getting Started

### For Developers
1. **Read**: `PROJECT_STRUCTURE_REORGANIZATION.md` for full context
2. **Follow**: `MIGRATION_GUIDE.md` for migration steps
3. **Use**: New folder structure for all new development
4. **Reference**: `docs/` folder for all project documentation

### For Agents
1. **Follow**: `agents/workflows/AGENT_WORKFLOW_RULES.md` for all activities
2. **Use**: Templates from `agents/templates/` for all outputs
3. **Report**: Completion using `agent-completion-report-template.md`
4. **Archive**: Working files to `agents/workspace/archive/`

### For Project Management
1. **Track**: Progress in `project-management/tracking/`
2. **Document**: Requirements in `docs/project-management/`
3. **Manage**: Governance in `project-management/governance/`
4. **Monitor**: Agent outputs in `agents/outputs/reports/`

## 🔄 Continuous Improvement

This structure is designed to evolve. Suggestions for improvements should be:

1. **Documented** in agent completion reports
2. **Discussed** in project management meetings
3. **Implemented** through controlled changes
4. **Validated** with team feedback

---

## 📞 Support

For questions about this structure:
- **Documentation Issues**: Create placeholder in appropriate `docs/` folder
- **Agent Workflow Questions**: Refer to `agents/workflows/AGENT_WORKFLOW_RULES.md`
- **Migration Issues**: Follow `MIGRATION_GUIDE.md` troubleshooting section
- **Structure Improvements**: Document in agent completion reports

---

**Status**: Structure Ready for Migration  
**Next Action**: Begin Phase 1 migration following `MIGRATION_GUIDE.md`  
**Documentation Coverage**: Comprehensive placeholders created  
**Agent Readiness**: Workflows and templates prepared  

*This structure represents a significant improvement in project organization and prepares the RepoLens codebase for scalable, maintainable future development with full agentic workflow support.*
