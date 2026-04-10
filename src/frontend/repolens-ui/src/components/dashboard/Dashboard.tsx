import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  LinearProgress,
  Chip,
  Stack,
  Alert,
  CircularProgress
} from '@mui/material';
import {
  Folder,
  Code,
  TrendingUp,
  Security,
  Add,
  Refresh
} from '@mui/icons-material';
import apiService from '../../services/apiService';
import { DashboardStats, ProcessingStatus } from '../../types/api';
import MetricCard from '../MetricCard';
import RepositoryHealthChip from '../RepositoryHealthChip';
import QualityHotspotRow, { QualityHotspot } from '../QualityHotspotRow';

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [dashboardStats, setDashboardStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [syncing, setSyncing] = useState(false);

  // Mock data for component demonstration
  const mockQualityHotspots: QualityHotspot[] = [
    {
      id: '1',
      filePath: 'src/services/apiService.ts',
      complexityScore: 92,
      churnRate: 78,
      qualityDeficit: 85,
      urgencyScore: 95,
      language: 'TypeScript',
      lineCount: 850,
      lastModified: '2024-03-25T10:30:00Z',
      issues: { security: 2, bugs: 3, codeSmells: 5 }
    },
    {
      id: '2', 
      filePath: 'components/dashboard/Dashboard.tsx',
      complexityScore: 76,
      churnRate: 45,
      qualityDeficit: 62,
      urgencyScore: 68,
      language: 'TypeScript',
      lineCount: 420,
      lastModified: '2024-03-20T14:15:00Z',
      issues: { security: 0, bugs: 1, codeSmells: 3 }
    },
    {
      id: '3',
      filePath: 'theme/design-system.ts',
      complexityScore: 45,
      churnRate: 25,
      qualityDeficit: 30,
      urgencyScore: 35,
      language: 'TypeScript', 
      lineCount: 280,
      lastModified: '2024-03-28T09:45:00Z',
      issues: { security: 0, bugs: 0, codeSmells: 1 }
    }
  ];

  const formatBytes = (bytes: number) => {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
  };

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);
      const stats = await apiService.getDashboardStats();
      setDashboardStats(stats);
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    setSyncing(true);
    await loadDashboardData();
    setSyncing(false);
  };

  const getStatusColor = (status: ProcessingStatus): 'success' | 'warning' | 'error' | 'info' => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'success';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'warning';
      case ProcessingStatus.Failed:
        return 'error';
      default:
        return 'info';
    }
  };

  const getActivityStatusLabel = (status: ProcessingStatus): string => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'Complete';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'Processing';
      case ProcessingStatus.Failed:
        return 'Error';
      case ProcessingStatus.Pending:
        return 'Pending';
      default:
        return 'Unknown';
    }
  };

  const getActivityStatusColor = (status: ProcessingStatus): 'success' | 'warning' | 'error' | 'info' => {
    return getStatusColor(status);
  };

  const getProcessingPercentage = (status: ProcessingStatus): number => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 100;
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 60;
      case ProcessingStatus.Failed:
        return 0;
      case ProcessingStatus.Pending:
        return 10;
      default:
        return 0;
    }
  };

  const getProcessingColor = (status: ProcessingStatus): 'success' | 'warning' | 'error' => {
    switch (status) {
      case ProcessingStatus.Completed:
        return 'success';
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return 'warning';
      case ProcessingStatus.Failed:
        return 'error';
      default:
        return 'warning';
    }
  };

  const getProcessingStatusMessage = (status: ProcessingStatus, totalRepos: number): string => {
    switch (status) {
      case ProcessingStatus.Completed:
        return `All ${totalRepos} repositories are up to date. Last sync: Recently`;
      case ProcessingStatus.InProgress:
      case ProcessingStatus.Processing:
        return `Processing repositories... ${totalRepos} total`;
      case ProcessingStatus.Failed:
        return 'Some repositories failed to process. Check logs for details.';
      case ProcessingStatus.Pending:
        return `${totalRepos} repositories waiting to be processed`;
      default:
        return 'System status unknown';
    }
  };

  useEffect(() => {
    loadDashboardData();
  }, []);

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
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
        <Button variant="outlined" onClick={loadDashboardData}>
          Retry
        </Button>
      </Box>
    );
  }

  if (!dashboardStats) {
    return (
      <Alert severity="warning">
        No dashboard data available
      </Alert>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Dashboard
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Overview of your repository analytics and system status
        </Typography>
      </Box>

      {/* Stats Cards - Enhanced with MetricCard components */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm: '1fr 1fr',
            md: '1fr 1fr 1fr 1fr'
          },
          gap: 3,
          mb: 4,
          justifyItems: 'center', // Center the 220px fixed-width cards
        }}
      >
        <MetricCard
          label="Repositories"
          value={dashboardStats.totalRepositories}
          trend={{
            direction: 'up',
            delta: '+2',
            context: 'this month',
            positive: 'up'
          }}
          onClick={() => navigate('/repositories')}
          loading={loading}
          aria-label="Total repositories with navigation to repositories page"
        />

        <MetricCard
          label="Code Files"
          value={dashboardStats.totalArtifacts.toLocaleString()}
          trend={{
            direction: 'up',
            delta: '+15%',
            context: 'vs last week',
            positive: 'up'
          }}
          onClick={() => navigate('/analytics')}
          loading={loading}
          aria-label="Total code files analyzed with navigation to analytics"
        />

        <MetricCard
          label="Storage"
          value={formatBytes(dashboardStats.totalStorageBytes)}
          trend={{
            direction: 'up',
            delta: '+8%',
            context: 'this month',
            positive: 'up'
          }}
          loading={loading}
          aria-label="Total data storage indexed"
        />

        <MetricCard
          label="System Health"
          value={getProcessingPercentage(dashboardStats.processingStatus)}
          unit="%"
          healthScore={getProcessingPercentage(dashboardStats.processingStatus)}
          trend={{
            direction: dashboardStats.processingStatus === ProcessingStatus.Completed ? 'up' : 'down',
            delta: dashboardStats.processingStatus === ProcessingStatus.Completed ? 'Stable' : 'Processing',
            context: 'current status',
            positive: 'up'
          }}
          loading={loading}
          aria-label="System processing health score"
        />
      </Box>

      {/* Main Content */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            md: '1fr 2fr'
          },
          gap: 3,
          mb: 3
        }}
      >
        {/* Quick Actions */}
        <Card sx={{ height: 'fit-content' }}>
          <CardContent>
            <Typography variant="h6" component="h2" gutterBottom>
              Quick Actions
            </Typography>
            
            <Stack spacing={2} sx={{ mt: 2 }}>
              <Button
                variant="contained"
                startIcon={<Add />}
                fullWidth
                onClick={() => navigate('/repositories')}
              >
                Add Repository
              </Button>
              
              <Button
                variant="outlined"
                startIcon={<Refresh />}
                fullWidth
                onClick={handleRefresh}
                disabled={syncing}
              >
                {syncing ? 'Syncing...' : 'Refresh Data'}
              </Button>
              
              <Button
                variant="outlined"
                startIcon={<TrendingUp />}
                fullWidth
                onClick={() => navigate('/analytics')}
              >
                View Analytics
              </Button>
            </Stack>
          </CardContent>
        </Card>

        {/* Recent Activity */}
        <Card>
          <CardContent>
            <Typography variant="h6" component="h2" gutterBottom>
              Recent Activity
            </Typography>
            
            <Box sx={{ mt: 2 }}>
              {dashboardStats.recentActivity && dashboardStats.recentActivity.length > 0 ? (
                dashboardStats.recentActivity.map((activity) => (
                  <Box
                    key={activity.id}
                    sx={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      py: 2,
                      borderBottom: '1px solid',
                      borderColor: 'divider',
                      '&:last-child': {
                        borderBottom: 'none'
                      }
                    }}
                  >
                    <Box sx={{ flexGrow: 1 }}>
                      <Typography variant="body2">
                        {activity.message}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(activity.timestamp).toLocaleString()}
                      </Typography>
                    </Box>
                    <Chip
                      label={getActivityStatusLabel(activity.status)}
                      size="small"
                      color={getActivityStatusColor(activity.status)}
                      variant="outlined"
                    />
                  </Box>
                ))
              ) : (
                <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                  No recent activity
                </Typography>
              )}
            </Box>
          </CardContent>
        </Card>
      </Box>

      {/* Repository Health Overview - NEW COMPONENT DEMO */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" component="h2" gutterBottom>
            Repository Health Overview
          </Typography>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Quick health assessment across your repository portfolio
          </Typography>
          
          <Box sx={{ 
            display: 'flex', 
            gap: 3, 
            flexWrap: 'wrap',
            mt: 2,
            alignItems: 'center'
          }}>
            <RepositoryHealthChip 
              healthScore={92} 
              trend={{ direction: 'up', delta: 3 }}
              showLabel
              onClick={() => navigate('/repositories/1')}
              aria-label="RepoLens API - Excellent health, trending up"
            />
            <RepositoryHealthChip 
              healthScore={76} 
              trend={{ direction: 'flat', delta: 0 }}
              showLabel
              onClick={() => navigate('/repositories/2')}
              aria-label="RepoLens UI - Good health, stable"
            />
            <RepositoryHealthChip 
              healthScore={45} 
              trend={{ direction: 'down', delta: -5 }}
              showLabel
              onClick={() => navigate('/repositories/3')}
              aria-label="Legacy System - Poor health, trending down"
            />
            <RepositoryHealthChip 
              healthScore={23} 
              trend={{ direction: 'down', delta: -8 }}
              showLabel
              onClick={() => navigate('/repositories/4')}
              aria-label="Old Codebase - Critical health, needs attention"
            />
          </Box>
        </CardContent>
      </Card>

      {/* Quality Hotspots - NEW COMPONENT DEMO */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" component="h2" gutterBottom>
            Top Quality Hotspots
          </Typography>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Files ranked by complexity × churn × quality deficit - requiring immediate attention
          </Typography>
          
          <Box sx={{ mt: 2, border: '1px solid', borderColor: 'divider', borderRadius: 1 }}>
            {mockQualityHotspots.map((hotspot, index) => (
              <QualityHotspotRow
                key={hotspot.id}
                hotspot={hotspot}
                repositoryId={1}
                rank={index + 1}
                onViewFile={(filePath) => {
                  console.log('Navigate to file:', filePath);
                  navigate(`/repos/1/files/${encodeURIComponent(filePath)}`);
                }}
              />
            ))}
          </Box>
          
          <Box sx={{ mt: 2, textAlign: 'center' }}>
            <Button 
              variant="outlined" 
              onClick={() => navigate('/analytics')}
              size="small"
            >
              View All Quality Issues
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Processing Status */}
      <Card>
        <CardContent>
          <Typography variant="h6" component="h2" gutterBottom>
            System Status
          </Typography>
          
          <Box sx={{ mt: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2">
                Repository Analysis
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {getProcessingPercentage(dashboardStats.processingStatus)}%
              </Typography>
            </Box>
            <LinearProgress 
              variant="determinate" 
              value={getProcessingPercentage(dashboardStats.processingStatus)} 
              color={getProcessingColor(dashboardStats.processingStatus)} 
              sx={{ mb: 2 }} 
            />
            
            <Typography variant="body2" color="text.secondary">
              {getProcessingStatusMessage(dashboardStats.processingStatus, dashboardStats.totalRepositories)}
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Dashboard;
