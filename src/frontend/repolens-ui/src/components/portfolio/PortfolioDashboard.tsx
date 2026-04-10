import React, { useState, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  Chip,
  Alert,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField
} from '@mui/material';
import {
  Star,
  StarBorder,
  Refresh,
  Warning,
  TrendingUp,
  Code,
  People,
  Assessment,
  FilterList
} from '@mui/icons-material';
import apiService from '../../services/apiService';

interface PortfolioStats {
  totalRepositories: number;
  activeRepositories: number;
  totalContributors: number;
  totalIssues: number;
  averageHealth: number;
  languageDistribution: Record<string, number>;
  lastUpdated: string;
}

interface Repository {
  id: number;
  name: string;
  url: string;
  health: number;
  lastCommit: string;
  contributors: number;
  issues: number;
  language: string;
  status: 'Active' | 'Inactive' | 'Archived';
  starred: boolean;
}

interface CriticalIssue {
  id: string;
  repositoryName: string;
  type: 'Security' | 'Quality' | 'Performance' | 'Dependency';
  severity: 'High' | 'Medium' | 'Low';
  message: string;
  fileAffected: string;
}

const PortfolioDashboard: React.FC = () => {
  const [portfolioStats, setPortfolioStats] = useState<PortfolioStats | null>(null);
  const [repositories, setRepositories] = useState<Repository[]>([]);
  const [criticalIssues, setCriticalIssues] = useState<CriticalIssue[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState({
    status: 'all',
    sortBy: 'health',
    sortOrder: 'desc',
    search: ''
  });

  useEffect(() => {
    loadPortfolioData();
  }, [filters]);

  const loadPortfolioData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Load portfolio data in parallel
      const [summaryData, repositoriesData, issuesData] = await Promise.all([
        apiService.getPortfolioSummary().catch(() => generateMockSummary()),
        apiService.getPortfolioRepositories({
          status: filters.status === 'all' ? undefined : filters.status,
          sortBy: filters.sortBy,
          sortOrder: filters.sortOrder,
          page: 1,
          pageSize: 50
        }).catch(() => generateMockRepositories()),
        apiService.getCriticalIssues().catch(() => generateMockIssues())
      ]);

      setPortfolioStats(summaryData);
      setRepositories(repositoriesData.repositories || repositoriesData);
      setCriticalIssues(issuesData.issues || issuesData);
    } catch (err: any) {
      console.error('Portfolio data loading failed:', err);
      setError('Failed to load portfolio data. Using demo data.');
      
      // Fallback to mock data
      setPortfolioStats(generateMockSummary());
      setRepositories(generateMockRepositories());
      setCriticalIssues(generateMockIssues());
    } finally {
      setLoading(false);
    }
  };

  const handleStarToggle = async (repositoryId: number) => {
    try {
      await apiService.toggleRepositoryStar(repositoryId);
      
      // Update local state
      setRepositories(repos =>
        repos.map(repo =>
          repo.id === repositoryId
            ? { ...repo, starred: !repo.starred }
            : repo
        )
      );
    } catch (err: any) {
      console.error('Failed to toggle star:', err);
      // Still update UI optimistically
      setRepositories(repos =>
        repos.map(repo =>
          repo.id === repositoryId
            ? { ...repo, starred: !repo.starred }
            : repo
        )
      );
    }
  };

  const handleRefresh = () => {
    loadPortfolioData();
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High': return 'error';
      case 'Medium': return 'warning';
      case 'Low': return 'info';
      default: return 'default';
    }
  };

  const getHealthColor = (health: number) => {
    if (health >= 80) return '#4caf50';
    if (health >= 60) return '#ff9800';
    return '#f44336';
  };

  // Mock data generators for fallback
  const generateMockSummary = (): PortfolioStats => ({
    totalRepositories: 12,
    activeRepositories: 8,
    totalContributors: 45,
    totalIssues: 23,
    averageHealth: 76,
    languageDistribution: {
      'TypeScript': 35,
      'C#': 28,
      'JavaScript': 20,
      'Python': 12,
      'CSS': 5
    },
    lastUpdated: new Date().toISOString()
  });

  const generateMockRepositories = (): Repository[] => [
    {
      id: 1,
      name: 'RepoLens-Frontend',
      url: 'https://github.com/company/repolens-frontend',
      health: 85,
      lastCommit: '2 hours ago',
      contributors: 8,
      issues: 3,
      language: 'TypeScript',
      status: 'Active',
      starred: true
    },
    {
      id: 2,
      name: 'RepoLens-API',
      url: 'https://github.com/company/repolens-api',
      health: 92,
      lastCommit: '4 hours ago',
      contributors: 12,
      issues: 1,
      language: 'C#',
      status: 'Active',
      starred: true
    },
    {
      id: 3,
      name: 'Data-Pipeline',
      url: 'https://github.com/company/data-pipeline',
      health: 68,
      lastCommit: '1 day ago',
      contributors: 5,
      issues: 8,
      language: 'Python',
      status: 'Active',
      starred: false
    },
    {
      id: 4,
      name: 'Legacy-System',
      url: 'https://github.com/company/legacy-system',
      health: 45,
      lastCommit: '2 weeks ago',
      contributors: 3,
      issues: 15,
      language: 'JavaScript',
      status: 'Inactive',
      starred: false
    }
  ];

  const generateMockIssues = (): CriticalIssue[] => [
    {
      id: '1',
      repositoryName: 'Data-Pipeline',
      type: 'Security',
      severity: 'High',
      message: 'Hardcoded API key detected in configuration',
      fileAffected: 'src/config/database.py'
    },
    {
      id: '2',
      repositoryName: 'Legacy-System',
      type: 'Quality',
      severity: 'Medium',
      message: 'High cyclomatic complexity detected',
      fileAffected: 'src/controllers/UserController.js'
    },
    {
      id: '3',
      repositoryName: 'RepoLens-Frontend',
      type: 'Dependency',
      severity: 'Medium',
      message: 'Vulnerable dependency: lodash@4.17.15',
      fileAffected: 'package.json'
    }
  ];

  const filteredRepositories = repositories.filter(repo => {
    const matchesSearch = repo.name.toLowerCase().includes(filters.search.toLowerCase());
    const matchesStatus = filters.status === 'all' || repo.status.toLowerCase() === filters.status.toLowerCase();
    return matchesSearch && matchesStatus;
  });

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
        <Typography variant="h6" sx={{ ml: 2 }}>
          Loading Portfolio Data...
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Portfolio Dashboard
        </Typography>
        <Button
          variant="outlined"
          startIcon={<Refresh />}
          onClick={handleRefresh}
          disabled={loading}
        >
          Refresh
        </Button>
      </Box>

      {error && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Portfolio Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center">
                <Code color="primary" sx={{ mr: 2 }} />
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Total Repositories
                  </Typography>
                  <Typography variant="h4">
                    {portfolioStats?.totalRepositories || 0}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    {portfolioStats?.activeRepositories || 0} active
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center">
                <People color="primary" sx={{ mr: 2 }} />
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Contributors
                  </Typography>
                  <Typography variant="h4">
                    {portfolioStats?.totalContributors || 0}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Across all repos
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center">
                <Assessment color="primary" sx={{ mr: 2 }} />
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Portfolio Health
                  </Typography>
                  <Typography variant="h4" sx={{ color: getHealthColor(portfolioStats?.averageHealth || 0) }}>
                    {portfolioStats?.averageHealth || 0}%
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Average health score
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center">
                <Warning color="warning" sx={{ mr: 2 }} />
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Critical Issues
                  </Typography>
                  <Typography variant="h4">
                    {criticalIssues.length}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    Need attention
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box display="flex" alignItems="center" gap={2} flexWrap="wrap">
            <FilterList color="primary" />
            <TextField
              size="small"
              placeholder="Search repositories..."
              value={filters.search}
              onChange={(e) => setFilters(prev => ({ ...prev, search: e.target.value }))}
              sx={{ minWidth: 200 }}
            />
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Status</InputLabel>
              <Select
                value={filters.status}
                onChange={(e) => setFilters(prev => ({ ...prev, status: e.target.value }))}
                label="Status"
              >
                <MenuItem value="all">All</MenuItem>
                <MenuItem value="active">Active</MenuItem>
                <MenuItem value="inactive">Inactive</MenuItem>
                <MenuItem value="archived">Archived</MenuItem>
              </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 120 }}>
              <InputLabel>Sort By</InputLabel>
              <Select
                value={filters.sortBy}
                onChange={(e) => setFilters(prev => ({ ...prev, sortBy: e.target.value }))}
                label="Sort By"
              >
                <MenuItem value="health">Health</MenuItem>
                <MenuItem value="name">Name</MenuItem>
                <MenuItem value="lastCommit">Last Commit</MenuItem>
                <MenuItem value="contributors">Contributors</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Repositories Table */}
        <Grid item xs={12} lg={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Repositories ({filteredRepositories.length})
              </Typography>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Repository</TableCell>
                      <TableCell>Health</TableCell>
                      <TableCell>Last Commit</TableCell>
                      <TableCell>Contributors</TableCell>
                      <TableCell>Issues</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filteredRepositories.map((repo) => (
                      <TableRow key={repo.id}>
                        <TableCell>
                          <Box>
                            <Typography variant="subtitle2">{repo.name}</Typography>
                            <Chip 
                              size="small" 
                              label={repo.language} 
                              variant="outlined" 
                            />
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Box display="flex" alignItems="center">
                            <Box
                              sx={{
                                width: 60,
                                height: 6,
                                borderRadius: 3,
                                backgroundColor: '#e0e0e0',
                                mr: 1
                              }}
                            >
                              <Box
                                sx={{
                                  width: `${repo.health}%`,
                                  height: '100%',
                                  borderRadius: 3,
                                  backgroundColor: getHealthColor(repo.health)
                                }}
                              />
                            </Box>
                            <Typography variant="body2">
                              {repo.health}%
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">{repo.lastCommit}</Typography>
                        </TableCell>
                        <TableCell>{repo.contributors}</TableCell>
                        <TableCell>{repo.issues}</TableCell>
                        <TableCell>
                          <Chip 
                            size="small"
                            label={repo.status}
                            color={repo.status === 'Active' ? 'success' : 'default'}
                          />
                        </TableCell>
                        <TableCell>
                          <Tooltip title={repo.starred ? 'Unstar' : 'Star'}>
                            <IconButton 
                              size="small"
                              onClick={() => handleStarToggle(repo.id)}
                            >
                              {repo.starred ? <Star color="warning" /> : <StarBorder />}
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>

        {/* Critical Issues */}
        <Grid item xs={12} lg={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Critical Issues
              </Typography>
              {criticalIssues.length === 0 ? (
                <Typography color="textSecondary">
                  No critical issues found
                </Typography>
              ) : (
                <Box>
                  {criticalIssues.map((issue) => (
                    <Box key={issue.id} sx={{ mb: 2, p: 2, border: 1, borderColor: 'grey.200', borderRadius: 1 }}>
                      <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
                        <Typography variant="subtitle2">{issue.repositoryName}</Typography>
                        <Chip 
                          size="small"
                          label={issue.severity}
                          color={getSeverityColor(issue.severity) as any}
                        />
                      </Box>
                      <Typography variant="body2" color="textSecondary" mb={1}>
                        {issue.type}: {issue.message}
                      </Typography>
                      <Typography variant="caption" color="textSecondary">
                        {issue.fileAffected}
                      </Typography>
                    </Box>
                  ))}
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Language Distribution */}
      {portfolioStats?.languageDistribution && (
        <Card sx={{ mt: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Language Distribution
            </Typography>
            <Grid container spacing={2}>
              {Object.entries(portfolioStats.languageDistribution).map(([language, percentage]) => (
                <Grid item xs={6} sm={4} md={2} key={language}>
                  <Box textAlign="center">
                    <Typography variant="h4" color="primary">
                      {percentage}%
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      {language}
                    </Typography>
                  </Box>
                </Grid>
              ))}
            </Grid>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default PortfolioDashboard;
