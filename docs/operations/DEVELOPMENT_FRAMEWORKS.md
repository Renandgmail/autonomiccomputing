# Development Frameworks

This document describes the development frameworks available for the RepoLens project.

## Overview

The RepoLens project includes several automated frameworks to streamline development, compilation, and git repository management processes:

1. **Service Compilation and Launch Framework** - `start-services-fixed.bat`
2. **ReadyForMerge Framework** - `readyformerge.bat`

## Service Compilation and Launch Framework

### Purpose
Automates the complete compilation and launch of both backend and frontend services with comprehensive validation and health checks.

### Location
`tools/dev-tools/start-services-fixed.bat`

### Features
- Prerequisites validation (.NET SDK, Node.js, npm)
- Project structure validation
- Systematic compilation of all 6 backend projects:
  - RepoLens.Core
  - RepoLens.Infrastructure
  - RepoLens.Api
  - RepoLens.Worker
  - DatabaseQuery
  - SearchApiDemo
- Frontend dependency installation and compilation
- Automated service startup with health checks
- Service monitoring and status reporting

### Usage
```batch
cd tools/dev-tools
start-services-fixed.bat
```

### Service Endpoints
After successful execution:
- **Backend API**: http://localhost:5000
- **Frontend UI**: http://localhost:3000
- **API Health Check**: http://localhost:5000/health
- **API Documentation**: http://localhost:5000/swagger

### Configuration
The framework uses the following default configuration:
- API Port: 5000
- Frontend Port: 3000
- Startup Timeout: 45 seconds

### Process Flow
1. **Prerequisites Check**: Validates .NET SDK, Node.js, and npm availability
2. **Structure Validation**: Ensures all required directories and files exist
3. **Process Cleanup**: Terminates any existing dotnet/node processes
4. **Backend Compilation**: Builds all 6 backend projects in Release configuration
5. **Backend Service Start**: Launches the API service with health monitoring
6. **Frontend Setup**: Installs dependencies and builds the React application
7. **Frontend Service Start**: Launches the development server with monitoring
8. **Health Verification**: Confirms both services are responding on their ports
9. **Status Report**: Provides summary and access information

## ReadyForMerge Framework

### Purpose
Comprehensive git repository preparation and validation framework that ensures the codebase is ready for merge operations while creating backups and detailed reports.

### Location
`tools/dev-tools/readyformerge.bat`

### Features

#### Phase 1: Pre-Merge Validation
- Git repository status validation
- Full project compilation verification
- Frontend build validation
- Pre-merge state archiving

#### Phase 2: Code Quality Checks
- File structure validation
- Common issues detection (large files, sensitive patterns, debug files)
- Dependency analysis and vulnerability scanning

#### Phase 3: Git Preparation
- Detailed git status analysis
- Branch information review
- .gitignore validation

#### Phase 4: Merge Readiness
- Final validation checks
- Comprehensive readiness summary
- Next steps recommendations

### Usage
```batch
cd tools/dev-tools
readyformerge.bat
```

### Outputs
The framework generates:
- **Pre-merge backup**: `legacy/pre-merge-archives/pre-merge-backup-[timestamp]/`
- **Detailed report**: `legacy/pre-merge-archives/pre-merge-report-[timestamp].txt`

### Validation Checklist
The framework validates:
- [x] All projects compile successfully
- [x] Git repository is in good state
- [x] Pre-merge backup created
- [x] Code quality checks passed
- [x] Dependencies analyzed
- [x] File structure validated

### Report Contents
The generated report includes:
- Git status and recent commits
- Branch information
- Project summary
- Compilation results
- Quality check results
- Recommendations

## Best Practices

### Before Starting Development
1. Run the **ReadyForMerge Framework** to ensure repository health
2. Address any warnings or issues identified
3. Create a feature branch: `git checkout -b feature/your-feature`

### During Development
1. Use the **Service Compilation and Launch Framework** for testing
2. Monitor service health through the provided endpoints
3. Regularly commit changes with descriptive messages

### Before Merging
1. Run the **ReadyForMerge Framework** again
2. Review the generated report
3. Ensure all validations pass
4. Create pull request with the report attached

## Framework Integration

Both frameworks are designed to work together:

1. **Initial Setup**: Use ReadyForMerge to validate repository state
2. **Development**: Use Service Framework for active development
3. **Pre-Merge**: Use ReadyForMerge again to prepare for repository updates

## Troubleshooting

### Common Issues

#### Service Framework
- **Port conflicts**: Change ports in the batch file configuration section
- **Compilation failures**: Check individual project build logs
- **Service startup failures**: Verify prerequisites are installed

#### ReadyForMerge Framework
- **Git not found**: Ensure git is installed and in PATH
- **Archive creation fails**: Check disk space and permissions
- **Build failures**: Resolve compilation errors before proceeding

### Error Codes
Both frameworks use standard exit codes:
- `0`: Success
- `1`: Error occurred, check output for details

## Framework Architecture

```
tools/dev-tools/
├── start-services-fixed.bat     # Service compilation and launch
├── readyformerge.bat           # Git repository preparation
├── start-services.bat          # Legacy service launcher (paths need fixing)
└── start-dev-services.bat      # Legacy dev service launcher (paths need fixing)

legacy/pre-merge-archives/       # ReadyForMerge outputs
├── pre-merge-backup-[timestamp]/
└── pre-merge-report-[timestamp].txt
```

## Future Enhancements

### Planned Features
- [ ] PowerShell versions for enhanced functionality
- [ ] Configuration file support
- [ ] Integration with CI/CD pipelines
- [ ] Automated testing framework integration
- [ ] Performance benchmarking integration
- [ ] Docker containerization support

### Extensibility
The frameworks are designed to be easily extensible:
- Add new projects by updating the compilation sections
- Extend validation rules in the quality check phases
- Customize reporting format and content
- Integrate with external tools and services

## Support

For issues or enhancement requests related to these frameworks, please:
1. Review this documentation
2. Check the troubleshooting section
3. Examine the generated reports and logs
4. Create an issue with detailed reproduction steps

---

*Last Updated: April 10, 2026*
*Framework Version: 1.0*
