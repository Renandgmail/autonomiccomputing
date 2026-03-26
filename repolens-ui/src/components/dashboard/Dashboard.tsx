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

const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [dashboardStats, setDashboardStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [syncing, setSyncing] = useState(false);

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

      {/* Stats Cards */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm: '1fr 1fr',
            md: '1fr 1fr 1fr 1fr'
          },
          gap: 3,
          mb: 4
        }}
      >
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Folder sx={{ color: 'primary.main', mr: 1 }} />
              <Typography variant="h6" component="div">
                Repositories
              </Typography>
            </Box>
            <Typography variant="h3" component="div" color="primary.main">
              {dashboardStats.totalRepositories}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Total tracked repositories
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Code sx={{ color: 'success.main', mr: 1 }} />
              <Typography variant="h6" component="div">
                Code Files
              </Typography>
            </Box>
            <Typography variant="h3" component="div" color="success.main">
              {dashboardStats.totalArtifacts.toLocaleString()}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Files analyzed
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <TrendingUp sx={{ color: 'warning.main', mr: 1 }} />
              <Typography variant="h6" component="div">
                Storage
              </Typography>
            </Box>
            <Typography variant="h3" component="div" color="warning.main">
              {formatBytes(dashboardStats.totalStorageBytes)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Data indexed
            </Typography>
          </CardContent>
        </Card>

        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Security sx={{ color: 'error.main', mr: 1 }} />
              <Typography variant="h6" component="div">
                Status
              </Typography>
            </Box>
            <Chip
              label={dashboardStats.processingStatus}
              color={getStatusColor(dashboardStats.processingStatus)}
              variant="outlined"
              sx={{ fontSize: '1rem', height: 32 }}
            />
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              System health
            </Typography>
          </CardContent>
        </Card>
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
