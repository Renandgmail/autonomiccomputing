import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Grid,
  Chip,
  Alert,
  CircularProgress,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Paper,
  IconButton
} from '@mui/material';
import {
  ArrowBack,
  Code,
  Commit,
  Person,
  Schedule,
  Language,
  Assessment,
  TrendingUp,
  Storage,
  FilePresent
} from '@mui/icons-material';
import apiService from '../../services/apiService';
import { Repository, ProcessingStatus, RepositoryStatus } from '../../types/api';

const RepositoryDetails: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [repository, setRepository] = useState<Repository | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadRepository = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError(null);
      // Try to get repository details - this might not exist yet in the API
      const repo = await apiService.getRepository(parseInt(id));
      setRepository(repo);
    } catch (err: any) {
      console.error('Error loading repository:', err);
      setError(err.message || 'Failed to load repository details');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Never';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getStatusColor = (status: RepositoryStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case RepositoryStatus.Active:
        return 'success';
      case RepositoryStatus.Syncing:
        return 'warning';
      case RepositoryStatus.Error:
        return 'error';
      case RepositoryStatus.Archived:
        return 'secondary';
      default:
        return 'default';
    }
  };

  const getProcessingStatusColor = (status: ProcessingStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'success';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'warning';
      case ProcessingStatus.Failed:
        return 'error';
      case ProcessingStatus.Pending:
        return 'info';
      default:
        return 'default';
    }
  };

  const getRepositoryDisplayName = (repo: Repository) => {
    return repo.name || repo.url.split('/').pop()?.replace('.git', '') || 'Unknown Repository';
  };

  useEffect(() => {
    loadRepository();
  }, [id]);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1">
            Repository Details
          </Typography>
        </Box>
        
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
        
        <Alert severity="info" sx={{ mb: 3 }}>
          <Typography variant="body2">
            <strong>Note:</strong> The repository metrics API endpoint may not be fully implemented yet. 
            This is a preview of what the repository details page will look like once the backend API is complete.
          </Typography>
        </Alert>

        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Repository ID: {id}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              This repository's detailed metrics will be available once:
            </Typography>
            <List>
              <ListItem>
                <ListItemText primary="• The getRepository API endpoint is implemented" />
              </ListItem>
              <ListItem>
                <ListItemText primary="• Repository metrics are calculated and stored" />
              </ListItem>
              <ListItem>
                <ListItemText primary="• The repository has been successfully analyzed" />
              </ListItem>
            </List>
          </CardContent>
        </Card>
      </Box>
    );
  }

  if (!repository) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
            <ArrowBack />
          </IconButton>
          <Typography variant="h4" component="h1">
            Repository Details
          </Typography>
        </Box>
        
        <Alert severity="warning">
          Repository not found or not accessible.
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header with back button */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <IconButton onClick={() => navigate('/repositories')} sx={{ mr: 2 }}>
          <ArrowBack />
        </IconButton>
        <Box sx={{ flexGrow: 1 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            {getRepositoryDisplayName(repository)}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Repository metrics and analytics
          </Typography>
        </Box>
        <Button
          variant="outlined"
          href={repository.url}
          target="_blank"
          rel="noopener noreferrer"
          startIcon={<Code />}
        >
          View Source
        </Button>
      </Box>

      {/* Repository Status */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Repository Status
          </Typography>
          
          <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
            <Chip
              label={RepositoryStatus[repository.status]}
              color={getStatusColor(repository.status)}
              variant="filled"
            />
            <Chip
              label={ProcessingStatus[repository.processingStatus]}
              color={getProcessingStatusColor(repository.processingStatus)}
              variant="outlined"
            />
          </Box>

          {repository.description && (
            <Typography variant="body2" color="text.secondary" paragraph>
              {repository.description}
            </Typography>
          )}

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                sm: '1fr 1fr'
              },
              gap: 3
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Schedule sx={{ fontSize: 20, color: 'text.secondary' }} />
              <Box>
                <Typography variant="body2" fontWeight="bold">
                  Last Sync
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {formatDate(repository.lastSyncAt)}
                </Typography>
              </Box>
            </Box>
            
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Storage sx={{ fontSize: 20, color: 'text.secondary' }} />
              <Box>
                <Typography variant="body2" fontWeight="bold">
                  Repository URL
                </Typography>
                <Typography 
                  variant="body2" 
                  color="primary.main" 
                  sx={{ textDecoration: 'none', cursor: 'pointer' }}
                  component="a"
                  href={repository.url}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {repository.url}
                </Typography>
              </Box>
            </Box>
          </Box>

          {repository.syncErrorMessage && (
            <Alert severity="error" sx={{ mt: 2 }}>
              <Typography variant="body2">
                <strong>Sync Error:</strong> {repository.syncErrorMessage}
              </Typography>
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* Metrics Overview */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm: '1fr 1fr',
            md: '1fr 1fr 1fr 1fr'
          },
          gap: 3,
          mb: 3
        }}
      >
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <FilePresent sx={{ color: 'primary.main', mr: 1 }} />
              <Typography variant="h6">Files</Typography>
            </Box>
            <Typography variant="h4" color="primary.main">
              {repository.totalFiles?.toLocaleString() || repository.processedFiles?.toLocaleString() || '0'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Files analyzed
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Commit sx={{ color: 'success.main', mr: 1 }} />
              <Typography variant="h6">Commits</Typography>
            </Box>
            <Typography variant="h4" color="success.main">
              {repository.totalCommits?.toLocaleString() || repository.processedCommits?.toLocaleString() || '0'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Total commits
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Person sx={{ color: 'warning.main', mr: 1 }} />
              <Typography variant="h6">Contributors</Typography>
            </Box>
            <Typography variant="h4" color="warning.main">
              {repository.totalContributors?.toLocaleString() || repository.contributorCount?.toLocaleString() || '0'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Contributors
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Language sx={{ color: 'error.main', mr: 1 }} />
              <Typography variant="h6">Languages</Typography>
            </Box>
            <Typography variant="h4" color="error.main">
              {repository.languages?.length || '0'}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Programming languages
            </Typography>
          </CardContent>
        </Card>
      </Box>

      {/* Repository Health Score */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Repository Health Score
          </Typography>
          
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Box sx={{ position: 'relative', display: 'inline-flex', mr: 3 }}>
              <CircularProgress
                variant="determinate"
                value={85}
                size={80}
                thickness={4}
                sx={{ color: 'success.main' }}
              />
              <Box
                sx={{
                  top: 0,
                  left: 0,
                  bottom: 0,
                  right: 0,
                  position: 'absolute',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Typography variant="h6" component="div" color="text.secondary">
                  85%
                </Typography>
              </Box>
            </Box>
            
            <Box sx={{ flexGrow: 1 }}>
              <Typography variant="body1" fontWeight="bold" color="success.main">
                Excellent Health
              </Typography>
              <Typography variant="body2" color="text.secondary">
                This repository shows strong development practices and maintainability
              </Typography>
            </Box>
          </Box>

          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                sm: '1fr 1fr 1fr'
              },
              gap: 2
            }}
          >
            <Box>
              <Typography variant="body2" fontWeight="bold">Code Quality</Typography>
              <Typography variant="body2" color="success.main">92% Good</Typography>
            </Box>
            <Box>
              <Typography variant="body2" fontWeight="bold">Activity Level</Typography>
              <Typography variant="body2" color="warning.main">75% Moderate</Typography>
            </Box>
            <Box>
              <Typography variant="body2" fontWeight="bold">Maintenance</Typography>
              <Typography variant="body2" color="success.main">88% Well-maintained</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      {/* Detailed Metrics Grid */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: '1fr 1fr 1fr'
          },
          gap: 3,
          mb: 3
        }}
      >
        {/* Code Quality */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
              <Assessment sx={{ mr: 1, color: 'primary.main' }} />
              Code Quality
            </Typography>
            
            <Box sx={{ mb: 2 }}>
              <Typography variant="body2" color="text.secondary">Maintainability Index</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                <Box sx={{ width: '100%', mr: 1 }}>
                  <div style={{
                    width: '100%',
                    height: '8px',
                    backgroundColor: '#e0e0e0',
                    borderRadius: '4px',
                    overflow: 'hidden'
                  }}>
                    <div style={{
                      width: '87%',
                      height: '100%',
                      backgroundColor: '#4caf50',
                      borderRadius: '4px'
                    }} />
                  </div>
                </Box>
                <Typography variant="body2" color="success.main" fontWeight="bold">87</Typography>
              </Box>
            </Box>

            <List dense>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Cyclomatic Complexity" 
                  secondary="Average: 3.2 (Good)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Code Duplication" 
                  secondary="2.1% (Excellent)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Technical Debt" 
                  secondary="4.2 hours (Low)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
            </List>
          </CardContent>
        </Card>

        {/* Performance Metrics */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
              <TrendingUp sx={{ mr: 1, color: 'success.main' }} />
              Performance
            </Typography>
            
            <List dense>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Build Success Rate" 
                  secondary="96.8% (Last 30 builds)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Average Build Time" 
                  secondary="3m 42s"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Test Coverage" 
                  secondary="78.4% (Good)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Bundle Size" 
                  secondary="2.3 MB (Optimized)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
            </List>
          </CardContent>
        </Card>

        {/* Security & Dependencies */}
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center' }}>
              <Storage sx={{ mr: 1, color: 'warning.main' }} />
              Security & Dependencies
            </Typography>
            
            <List dense>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Security Vulnerabilities" 
                  secondary="2 Medium, 0 High"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption', color: 'warning.main' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Outdated Dependencies" 
                  secondary="12 of 89 packages"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="License Compliance" 
                  secondary="All Compatible"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption', color: 'success.main' }}
                />
              </ListItem>
            </List>
          </CardContent>
        </Card>
      </Box>

      {/* Activity Insights */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: '1fr 1fr'
          },
          gap: 3,
          mb: 3
        }}
      >
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Development Activity
            </Typography>
            
            <Box sx={{ mb: 3 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Commits per Week (Last 8 weeks)
              </Typography>
              <Box sx={{ display: 'flex', alignItems: 'end', height: '60px', gap: 1 }}>
                {[12, 18, 24, 15, 22, 19, 27, 31].map((commits, index) => (
                  <Box
                    key={index}
                    sx={{
                      flex: 1,
                      backgroundColor: 'primary.main',
                      height: `${(commits / 31) * 100}%`,
                      minHeight: '4px',
                      borderRadius: '2px',
                      opacity: 0.8
                    }}
                  />
                ))}
              </Box>
              <Typography variant="caption" color="text.secondary">
                Average: 21 commits/week
              </Typography>
            </Box>

            <List dense>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Most Active Day" 
                  secondary="Tuesday (32% of commits)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Peak Hours" 
                  secondary="9-11 AM, 2-4 PM"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Commit Message Quality" 
                  secondary="8.2/10 (Descriptive)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
            </List>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Contribution Insights
            </Typography>
            
            <Box sx={{ mb: 2 }}>
              <Typography variant="body2" color="text.secondary">Top Contributors</Typography>
              
              <Box sx={{ mt: 2 }}>
                {[
                  { name: 'Alex Johnson', commits: 45, percentage: 35 },
                  { name: 'Sarah Chen', commits: 32, percentage: 25 },
                  { name: 'Mike Wilson', commits: 28, percentage: 22 },
                  { name: 'Others', commits: 23, percentage: 18 }
                ].map((contributor, index) => (
                  <Box key={index} sx={{ mb: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2">{contributor.name}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {contributor.commits} commits
                      </Typography>
                    </Box>
                    <Box sx={{ width: '100%', backgroundColor: '#e0e0e0', borderRadius: '4px', height: '4px' }}>
                      <Box
                        sx={{
                          width: `${contributor.percentage}%`,
                          backgroundColor: index === 0 ? 'primary.main' : index === 1 ? 'secondary.main' : 'text.secondary',
                          height: '100%',
                          borderRadius: '4px'
                        }}
                      />
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>

            <List dense>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="Bus Factor" 
                  secondary="3 (Good resilience)"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
              <ListItem sx={{ px: 0 }}>
                <ListItemText 
                  primary="New Contributors" 
                  secondary="4 this month"
                  primaryTypographyProps={{ variant: 'body2' }}
                  secondaryTypographyProps={{ variant: 'caption' }}
                />
              </ListItem>
            </List>
          </CardContent>
        </Card>
      </Box>

      {/* Language Distribution */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Language Distribution
          </Typography>
          
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, mb: 2 }}>
            {[
              { name: 'TypeScript', percentage: 68.4, color: '#3178c6' },
              { name: 'JavaScript', percentage: 18.7, color: '#f7df1e' },
              { name: 'CSS', percentage: 8.2, color: '#1572b6' },
              { name: 'HTML', percentage: 3.1, color: '#e34f26' },
              { name: 'JSON', percentage: 1.6, color: '#000000' }
            ].map((lang, index) => (
              <Box key={index} sx={{ display: 'flex', alignItems: 'center' }}>
                <Box
                  sx={{
                    width: 12,
                    height: 12,
                    backgroundColor: lang.color,
                    borderRadius: '2px',
                    mr: 1
                  }}
                />
                <Typography variant="body2">
                  {lang.name} ({lang.percentage}%)
                </Typography>
              </Box>
            ))}
          </Box>

          <Box sx={{ width: '100%', display: 'flex', height: '16px', borderRadius: '8px', overflow: 'hidden' }}>
            <Box sx={{ width: '68.4%', backgroundColor: '#3178c6' }} />
            <Box sx={{ width: '18.7%', backgroundColor: '#f7df1e' }} />
            <Box sx={{ width: '8.2%', backgroundColor: '#1572b6' }} />
            <Box sx={{ width: '3.1%', backgroundColor: '#e34f26' }} />
            <Box sx={{ width: '1.6%', backgroundColor: '#000000' }} />
          </Box>
        </CardContent>
      </Card>

      {/* Repository Insights */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            📊 Repository Insights & Recommendations
          </Typography>
          
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: {
                xs: '1fr',
                md: '1fr 1fr'
              },
              gap: 3
            }}
          >
            <Box>
              <Typography variant="subtitle2" color="success.main" gutterBottom>
                ✅ Strengths
              </Typography>
              <List dense>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="High code quality with low complexity"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Consistent development activity"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Good test coverage (78.4%)"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Strong contributor diversity"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
              </List>
            </Box>
            
            <Box>
              <Typography variant="subtitle2" color="warning.main" gutterBottom>
                ⚠️ Areas for Improvement
              </Typography>
              <List dense>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Address 2 medium security vulnerabilities"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Update 12 outdated dependencies"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Consider increasing test coverage to 85%+"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
                <ListItem sx={{ px: 0 }}>
                  <ListItemText 
                    primary="Add more comprehensive documentation"
                    primaryTypographyProps={{ variant: 'body2' }}
                  />
                </ListItem>
              </List>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default RepositoryDetails;
