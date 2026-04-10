import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  Card,
  CardContent,
  Grid,
  Chip,
  List,
  ListItem,
  ListItemText,
  Button,
  Alert,
  CircularProgress,
  Divider,
  IconButton,
  Tooltip,
  Select,
  MenuItem,
  FormControl
} from '@mui/material';
import {
  TrendingUp,
  TrendingDown,
  Remove as TrendingFlat,
  Security,
  BugReport,
  Warning,
  Info,
  CheckCircle,
  OpenInNew,
  Refresh,
  ArrowForward,
  Schedule
} from '@mui/icons-material';
import apiService from '../../services/apiService';

// Interfaces for L4 File Detail
interface FileMetrics {
  qualityScore: number;
  qualityTrend: 'up' | 'down' | 'flat';
  complexity: number;
  technicalDebt: number; // hours
  testCoverage: number;
  securityIssues: number;
  lastChanged: string;
  changeFrequency: number; // changes in last 30 days
  dependenciesOut: FileDependency[];
  dependenciesIn: FileDependency[];
}

interface FileDependency {
  id: string;
  filePath: string;
  fileName: string;
  type: 'import' | 'call' | 'extend' | 'implement';
  count: number;
}

interface FileIssue {
  id: string;
  severity: 'critical' | 'high' | 'medium' | 'low';
  type: string;
  description: string;
  lineNumber: number;
  recommendation: string;
  status: 'open' | 'acknowledged' | 'resolved';
}

interface FileDetails {
  id: string;
  fileName: string;
  filePath: string;
  repositoryName: string;
  metrics: FileMetrics;
  issues: FileIssue[];
}

interface L4FileDetailProps {
  repositoryId?: number;
  fileId?: string;
}

const L4FileDetail: React.FC<L4FileDetailProps> = ({ 
  repositoryId: propRepositoryId, 
  fileId: propFileId 
}) => {
  const { repoId, fileId } = useParams<{ repoId: string; fileId: string }>();
  const navigate = useNavigate();

  const repositoryId = propRepositoryId || (repoId ? Number(repoId) : undefined);
  const currentFileId = propFileId || fileId;

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fileDetails, setFileDetails] = useState<FileDetails | null>(null);

  useEffect(() => {
    if (repositoryId && currentFileId) {
      loadFileDetails();
    }
  }, [repositoryId, currentFileId]);

  const loadFileDetails = async () => {
    try {
      setLoading(true);
      setError(null);

      // Try to load from API first (mock for now)
      const data = null; // await apiService.getFileDetails(repositoryId!, currentFileId!);
      
      if (data) {
        setFileDetails(data);
      } else {
        // Generate mock data for demonstration
        generateMockFileDetails();
      }
    } catch (err) {
      console.error('Error loading file details:', err);
      generateMockFileDetails(); // Fallback to mock data
    } finally {
      setLoading(false);
    }
  };

  const generateMockFileDetails = () => {
    const filePath = decodeURIComponent(currentFileId || 'src/auth/AuthService.js');
    const fileName = filePath.split('/').pop() || 'AuthService.js';

    const mockData: FileDetails = {
      id: currentFileId || 'auth-service-1',
      fileName,
      filePath,
      repositoryName: 'frontend-app',
      metrics: {
        qualityScore: 92,
        qualityTrend: 'up',
        complexity: 7.2,
        technicalDebt: 2.4,
        testCoverage: 88,
        securityIssues: 2,
        lastChanged: '2 days ago',
        changeFrequency: 8,
        dependenciesOut: [
          { id: 'user-service', filePath: 'src/services/UserService.ts', fileName: 'UserService.ts', type: 'import', count: 3 },
          { id: 'config-service', filePath: 'src/config/ConfigService.ts', fileName: 'ConfigService.ts', type: 'import', count: 1 },
          { id: 'validation-utils', filePath: 'src/utils/ValidationUtils.ts', fileName: 'ValidationUtils.ts', type: 'import', count: 2 }
        ],
        dependenciesIn: [
          { id: 'payment-api', filePath: 'src/api/PaymentAPI.ts', fileName: 'PaymentAPI.ts', type: 'import', count: 1 },
          { id: 'dashboard-component', filePath: 'src/components/Dashboard.tsx', fileName: 'Dashboard.tsx', type: 'import', count: 2 }
        ]
      },
      issues: [
        {
          id: 'sql-injection-47',
          severity: 'critical',
          type: 'SQL Injection',
          description: 'Direct SQL query concatenation detected',
          lineNumber: 47,
          recommendation: 'Use parameterized queries',
          status: 'open'
        },
        {
          id: 'weak-password-89',
          severity: 'high',
          type: 'Weak Password Validation',
          description: 'Password complexity requirements too low',
          lineNumber: 89,
          recommendation: 'Add regex validation',
          status: 'acknowledged'
        },
        {
          id: 'missing-logs-123',
          severity: 'medium',
          type: 'Missing Error Logs',
          description: 'Exception handling without logging',
          lineNumber: 123,
          recommendation: 'Add try-catch logging',
          status: 'open'
        }
      ]
    };

    setFileDetails(mockData);
  };

  const handleDependencyClick = (dependency: FileDependency) => {
    navigate(`/repos/${repositoryId}/files/${encodeURIComponent(dependency.filePath)}`);
  };

  const handleIssueStatusChange = async (issueId: string, newStatus: string) => {
    if (!fileDetails) return;

    // Optimistic update
    const updatedIssues = fileDetails.issues.map(issue => 
      issue.id === issueId ? { ...issue, status: newStatus as any } : issue
    );
    setFileDetails({ ...fileDetails, issues: updatedIssues });

    try {
      // API call to update issue status (mock for now)
      // await apiService.updateIssueStatus(repositoryId!, issueId, newStatus);
      console.log('Issue status updated:', issueId, newStatus);
    } catch (err) {
      console.error('Failed to update issue status:', err);
      // Revert on error
      setFileDetails(fileDetails);
    }
  };

  const getTrendIcon = (trend: 'up' | 'down' | 'flat') => {
    switch (trend) {
      case 'up': return <TrendingUp color="success" />;
      case 'down': return <TrendingDown color="error" />;
      default: return <TrendingFlat color="action" />;
    }
  };

  const getComplexityColor = (complexity: number) => {
    if (complexity >= 8) return 'error';
    if (complexity >= 6) return 'warning';
    if (complexity >= 4) return 'info';
    return 'success';
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'critical': return <Security sx={{ color: '#d32f2f' }} />;
      case 'high': return <BugReport sx={{ color: '#ff9800' }} />;
      case 'medium': return <Warning sx={{ color: '#ffc107' }} />;
      case 'low': return <Info sx={{ color: '#2196f3' }} />;
      default: return <Info />;
    }
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'critical': return '#d32f2f';
      case 'high': return '#ff9800';
      case 'medium': return '#ffc107';
      case 'low': return '#2196f3';
      default: return '#757575';
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !fileDetails) {
    return (
      <Alert severity="error" action={
        <Button color="inherit" size="small" onClick={loadFileDetails} startIcon={<Refresh />}>
          Retry
        </Button>
      }>
        {error || 'Failed to load file details'}
      </Alert>
    );
  }

  return (
    <Box>
      {/* Extended Breadcrumb - will be handled by ContextBar in implementation */}
      <Box sx={{ mb: 3 }}>
        <Typography variant="body2" color="text.secondary">
          Portfolio {'>'} {fileDetails.repositoryName} {'>'} {fileDetails.filePath}
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* Metrics Panel (Left) */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3, height: 'fit-content' }}>
            <Typography variant="h6" gutterBottom>
              File Metrics
            </Typography>
            
            {/* Quality Score */}
            <Box display="flex" alignItems="center" mb={2}>
              <Box>
                <Typography variant="h3" color="primary.main">
                  {fileDetails.metrics.qualityScore}%
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Quality Score
                </Typography>
              </Box>
              <Box sx={{ ml: 2 }}>
                {getTrendIcon(fileDetails.metrics.qualityTrend)}
              </Box>
            </Box>

            <Divider sx={{ my: 2 }} />

            {/* Other Metrics */}
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="h4" color={`${getComplexityColor(fileDetails.metrics.complexity)}.main`}>
                  {fileDetails.metrics.complexity}/10
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Complexity
                </Typography>
              </Grid>
              
              <Grid item xs={6}>
                <Typography variant="h4" color="warning.main">
                  {fileDetails.metrics.technicalDebt}h
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Tech Debt
                </Typography>
              </Grid>

              <Grid item xs={6}>
                <Typography variant="h4" color="success.main">
                  {fileDetails.metrics.testCoverage}%
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Test Coverage
                </Typography>
              </Grid>

              <Grid item xs={6}>
                <Typography variant="h4" color="error.main">
                  {fileDetails.metrics.securityIssues}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Security Issues
                </Typography>
              </Grid>
            </Grid>

            <Divider sx={{ my: 2 }} />

            {/* Change Information */}
            <Box mb={2}>
              <Typography variant="body2" gutterBottom>
                <Schedule fontSize="small" sx={{ mr: 1, verticalAlign: 'middle' }} />
                Last changed: {fileDetails.metrics.lastChanged}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Changed {fileDetails.metrics.changeFrequency}× this month
              </Typography>
            </Box>

            <Divider sx={{ my: 2 }} />

            {/* Dependencies */}
            <Typography variant="subtitle2" gutterBottom>
              Dependencies
            </Typography>
            
            {fileDetails.metrics.dependenciesOut.length > 0 && (
              <Box mb={2}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  → Uses ({fileDetails.metrics.dependenciesOut.length})
                </Typography>
                {fileDetails.metrics.dependenciesOut.map((dep) => (
                  <Chip
                    key={dep.id}
                    label={`${dep.fileName} (${dep.count})`}
                    size="small"
                    onClick={() => handleDependencyClick(dep)}
                    sx={{ mr: 0.5, mb: 0.5 }}
                    icon={<ArrowForward />}
                  />
                ))}
              </Box>
            )}

            {fileDetails.metrics.dependenciesIn.length > 0 && (
              <Box>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  ← Used by ({fileDetails.metrics.dependenciesIn.length})
                </Typography>
                {fileDetails.metrics.dependenciesIn.map((dep) => (
                  <Chip
                    key={dep.id}
                    label={`${dep.fileName} (${dep.count})`}
                    size="small"
                    onClick={() => handleDependencyClick(dep)}
                    sx={{ mr: 0.5, mb: 0.5 }}
                    variant="outlined"
                  />
                ))}
              </Box>
            )}
          </Paper>
        </Grid>

        {/* Issues List (Right) */}
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Issues ({fileDetails.issues.length})
            </Typography>

            {fileDetails.issues.length === 0 ? (
              <Alert severity="success" icon={<CheckCircle />}>
                No issues detected in this file
              </Alert>
            ) : (
              <List disablePadding>
                {fileDetails.issues.map((issue, index) => (
                  <React.Fragment key={issue.id}>
                    {index > 0 && <Divider />}
                    <ListItem sx={{ px: 0, py: 2, flexDirection: 'column', alignItems: 'flex-start' }}>
                      <Box display="flex" alignItems="center" width="100%" mb={1}>
                        <Box display="flex" alignItems="center" gap={1}>
                          {getSeverityIcon(issue.severity)}
                          <Typography variant="subtitle2" fontWeight="bold">
                            {issue.type} · Line {issue.lineNumber}
                          </Typography>
                        </Box>
                        <Box sx={{ ml: 'auto' }}>
                          <FormControl size="small" variant="outlined">
                            <Select
                              value={issue.status}
                              onChange={(e) => handleIssueStatusChange(issue.id, e.target.value)}
                              sx={{ minWidth: 120 }}
                            >
                              <MenuItem value="open">Open</MenuItem>
                              <MenuItem value="acknowledged">Acknowledged</MenuItem>
                              <MenuItem value="resolved">Resolved</MenuItem>
                            </Select>
                          </FormControl>
                        </Box>
                      </Box>

                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        {issue.description}
                      </Typography>

                      <Typography variant="body2" color="primary.main">
                        💡 {issue.recommendation}
                      </Typography>
                    </ListItem>
                  </React.Fragment>
                ))}
              </List>
            )}

            {/* Action Buttons */}
            <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid #eee' }}>
              <Button
                variant="outlined"
                startIcon={<OpenInNew />}
                onClick={() => {
                  // Open in editor (VS Code protocol or similar)
                  window.open(`vscode://file/${fileDetails.filePath}`, '_blank');
                }}
                sx={{ mr: 2 }}
              >
                Open in Editor
              </Button>
              <Button
                variant="outlined"
                onClick={() => {
                  navigate(`/repos/${repositoryId}/analytics/files?file=${encodeURIComponent(fileDetails.filePath)}`);
                }}
              >
                View in Analytics
              </Button>
            </Box>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default L4FileDetail;
