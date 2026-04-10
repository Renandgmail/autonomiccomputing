/**
 * L2 Repository Dashboard - Main Container Component
 * Provides repository-specific insights for Engineering Managers
 * 4-zone layout: Summary | Hotspots | Activity | Quick Actions
 * Target: 60-second decision time for repository actions
 */

import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Alert, CircularProgress, Box, Typography, Grid, Container } from '@mui/material';
import { styled } from '@mui/material/styles';
import RepositorySummaryCards from './RepositorySummaryCards';
import QualityHotspotsPanel from './QualityHotspotsPanel';
import ActivityFeedPanel from './ActivityFeedPanel';
import QuickActionsPanel from './QuickActionsPanel';
import repositoryApiService, {
  RepositorySummary,
  QualityHotspotsResponse,
  RepositoryActivityResponse,
  RepositoryQuickActions,
  RepositoryContext,
} from '../../services/repositoryApiService';

// Styled components for 4-zone layout
const DashboardContainer = styled(Container)(({ theme }) => ({
  maxWidth: '1440px',
  padding: theme.spacing(3),
  '& .MuiGrid-container': {
    gap: theme.spacing(3),
  },
}));

const Zone1Container = styled(Box)(({ theme }) => ({
  marginBottom: theme.spacing(3),
}));

const Zone234Container = styled(Box)(({ theme }) => ({
  display: 'grid',
  gridTemplateColumns: '1fr 300px',
  gap: theme.spacing(3),
  alignItems: 'start',
  
  // Mobile responsive
  [theme.breakpoints.down('lg')]: {
    gridTemplateColumns: '1fr',
    gap: theme.spacing(2),
  },
}));

const PrimaryPanel = styled(Box)(({ theme }) => ({
  display: 'grid',
  gridTemplateColumns: '1fr',
  gap: theme.spacing(3),
  
  // Desktop: 2 panels side by side
  [theme.breakpoints.up('lg')]: {
    gridTemplateColumns: '2fr 1fr',
  },
  
  // Tablet: 2 panels side by side
  [theme.breakpoints.between('md', 'lg')]: {
    gridTemplateColumns: '1fr 1fr',
  },
}));

const SecondaryPanel = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing(2),
  
  [theme.breakpoints.down('lg')]: {
    order: -1, // Show quick actions first on mobile
  },
}));

const ErrorAlert = styled(Alert)(({ theme }) => ({
  marginBottom: theme.spacing(3),
}));

const LoadingContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  minHeight: '400px',
  flexDirection: 'column',
  gap: theme.spacing(2),
}));

interface L2RepositoryDashboardProps {
  repositoryId?: number;
}

/**
 * L2 Repository Dashboard - Engineering Manager repository insights
 * Shows 4 zones: Summary Cards | Quality Hotspots | Activity Feed | Quick Actions
 */
const L2RepositoryDashboard: React.FC<L2RepositoryDashboardProps> = ({ repositoryId: propRepositoryId }) => {
  const { repositoryId: paramRepositoryId } = useParams<{ repositoryId: string }>();
  const navigate = useNavigate();
  
  const repositoryId = propRepositoryId || (paramRepositoryId ? parseInt(paramRepositoryId, 10) : null);

  // State management for all 4 zones
  const [summary, setSummary] = useState<RepositorySummary | null>(null);
  const [hotspots, setHotspots] = useState<QualityHotspotsResponse | null>(null);
  const [activity, setActivity] = useState<RepositoryActivityResponse | null>(null);
  const [quickActions, setQuickActions] = useState<RepositoryQuickActions | null>(null);
  const [repositoryContext, setRepositoryContext] = useState<RepositoryContext | null>(null);
  
  // Loading and error states
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  // Load all dashboard data
  const loadDashboardData = async (repoId: number) => {
    try {
      setError(null);
      setLoading(true);

      // Load all 4 zones in parallel for optimal performance
      const [summaryData, hotspotsData, activityData, quickActionsData, contextData] = await Promise.allSettled([
        repositoryApiService.getRepositorySummary(repoId),
        repositoryApiService.getQualityHotspots(repoId, { limit: 5 }),
        repositoryApiService.getRepositoryActivity(repoId, { limit: 10 }),
        repositoryApiService.getQuickActions(repoId),
        repositoryApiService.getRepositoryContext(repoId),
      ]);

      // Handle results - show partial data if some zones fail
      if (summaryData.status === 'fulfilled') {
        setSummary(summaryData.value);
      } else {
        console.error('Failed to load summary:', summaryData.reason);
      }

      if (hotspotsData.status === 'fulfilled') {
        setHotspots(hotspotsData.value);
      } else {
        console.error('Failed to load hotspots:', hotspotsData.reason);
      }

      if (activityData.status === 'fulfilled') {
        setActivity(activityData.value);
      } else {
        console.error('Failed to load activity:', activityData.reason);
      }

      if (quickActionsData.status === 'fulfilled') {
        setQuickActions(quickActionsData.value);
      } else {
        console.error('Failed to load quick actions:', quickActionsData.reason);
      }

      if (contextData.status === 'fulfilled') {
        setRepositoryContext(contextData.value);
      } else {
        console.error('Failed to load repository context:', contextData.reason);
      }

      // Show error only if all zones failed
      const failedCount = [summaryData, hotspotsData, activityData, quickActionsData, contextData]
        .filter(result => result.status === 'rejected').length;
      
      if (failedCount >= 4) {
        setError('Failed to load repository dashboard. Please try again.');
      }

    } catch (err) {
      console.error('Dashboard loading error:', err);
      setError('Failed to load repository dashboard. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Manual refresh handler
  const handleRefresh = async () => {
    if (!repositoryId) return;
    
    try {
      setRefreshing(true);
      setError(null);

      // Trigger backend refresh
      await repositoryApiService.refreshRepository(repositoryId);
      
      // Reload dashboard data
      await loadDashboardData(repositoryId);
    } catch (err) {
      console.error('Refresh error:', err);
      setError('Failed to refresh repository data. Please try again.');
    } finally {
      setRefreshing(false);
    }
  };

  // Handle navigation to L3/L4 screens
  const handleNavigate = (route: string) => {
    navigate(route);
  };

  // Handle hotspot click - navigate to L4 file detail
  const handleHotspotClick = (hotspot: any) => {
    navigate(`/repositories/${repositoryId}/files/${hotspot.fileId}`);
  };

  // Load data on mount and when repository changes
  useEffect(() => {
    if (repositoryId) {
      loadDashboardData(repositoryId);
    } else {
      setError('Repository ID is required');
      setLoading(false);
    }
  }, [repositoryId]);

  // Validation
  if (!repositoryId) {
    return (
      <DashboardContainer>
        <ErrorAlert severity="error">
          Repository ID is required to display the dashboard.
        </ErrorAlert>
      </DashboardContainer>
    );
  }

  // Loading state
  if (loading) {
    return (
      <DashboardContainer>
        <LoadingContainer>
          <CircularProgress size={48} />
          <Typography variant="body1" color="textSecondary">
            Loading repository dashboard...
          </Typography>
          {repositoryContext && (
            <Typography variant="body2" color="textSecondary">
              {repositoryContext.name}
            </Typography>
          )}
        </LoadingContainer>
      </DashboardContainer>
    );
  }

  return (
    <DashboardContainer>
      {/* Error Alert */}
      {error && (
        <ErrorAlert severity="error">
          {error}
        </ErrorAlert>
      )}

      {/* Zone 1: Repository Summary Cards */}
      <Zone1Container>
        <RepositorySummaryCards
          repositoryId={repositoryId}
          summary={summary}
          loading={refreshing}
          onRefresh={handleRefresh}
          onNavigate={handleNavigate}
        />
      </Zone1Container>

      {/* Zones 2, 3, 4: Main Content Layout */}
      <Zone234Container>
        {/* Primary Panel: Zones 2 & 3 */}
        <PrimaryPanel>
          {/* Zone 2: Quality Hotspots (Primary - Most Important) */}
          <QualityHotspotsPanel
            repositoryId={repositoryId}
            hotspots={hotspots}
            loading={refreshing}
            onHotspotClick={handleHotspotClick}
            onSeeAll={() => handleNavigate(`/repositories/${repositoryId}/analytics?tab=files`)}
          />

          {/* Zone 3: Recent Activity Feed (Secondary) */}
          <ActivityFeedPanel
            repositoryId={repositoryId}
            activity={activity}
            loading={refreshing}
          />
        </PrimaryPanel>

        {/* Secondary Panel: Zone 4 */}
        <SecondaryPanel>
          {/* Zone 4: Quick Actions (Tertiary) */}
          <QuickActionsPanel
            repositoryId={repositoryId}
            quickActions={quickActions}
            loading={refreshing}
            onNavigate={handleNavigate}
          />
        </SecondaryPanel>
      </Zone234Container>
    </DashboardContainer>
  );
};

export default L2RepositoryDashboard;
