/**
 * Zone 1: Repository Summary Cards Component
 * Displays exactly 4 metrics for Engineering Managers
 * Health Score | Active Contributors | Critical Issues | Technical Debt Hours
 */

import React from 'react';
import { Box, Grid, IconButton, Tooltip, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import { Refresh as RefreshIcon, TrendingUp, TrendingDown, TrendingFlat } from '@mui/icons-material';
import MetricCard from '../MetricCard';
import { RepositorySummary, TrendIndicator } from '../../services/repositoryApiService';

const SummaryContainer = styled(Box)(({ theme }) => ({
  marginBottom: theme.spacing(3),
}));

const SummaryHeader = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  marginBottom: theme.spacing(2),
}));

const SummaryTitle = styled(Typography)(({ theme }) => ({
  fontSize: '1.125rem',
  fontWeight: 600,
  color: theme.palette.text.primary,
}));

const RefreshButton = styled(IconButton)(({ theme }) => ({
  padding: theme.spacing(1),
  '&:hover': {
    backgroundColor: theme.palette.action.hover,
  },
}));

const CardsGrid = styled(Grid)(({ theme }) => ({
  gap: theme.spacing(2),
  
  // Responsive grid layout
  [theme.breakpoints.down('md')]: {
    '& .MuiGrid-item': {
      flex: '1 1 100%', // Stack on mobile
    },
  },
  
  [theme.breakpoints.between('md', 'lg')]: {
    '& .MuiGrid-item': {
      flex: '1 1 48%', // 2x2 grid on tablet
    },
  },
  
  [theme.breakpoints.up('lg')]: {
    '& .MuiGrid-item': {
      flex: '1 1 23%', // 4 cards in a row on desktop
    },
  },
}));

interface RepositorySummaryCardsProps {
  repositoryId: number;
  summary: RepositorySummary | null;
  loading?: boolean;
  onRefresh?: () => void;
  onNavigate?: (route: string) => void;
}

/**
 * Get trend icon based on direction
 */
const getTrendIcon = (trend: TrendIndicator) => {
  switch (trend.direction) {
    case 'up':
      return <TrendingUp fontSize="small" sx={{ color: trend.positiveDirection === 'up' ? '#16A34A' : '#DC2626' }} />;
    case 'down':
      return <TrendingDown fontSize="small" sx={{ color: trend.positiveDirection === 'up' ? '#DC2626' : '#16A34A' }} />;
    default:
      return <TrendingFlat fontSize="small" sx={{ color: '#6B7280' }} />;
  }
};

/**
 * Zone 1: Repository Summary Cards
 * Shows 4 exact metrics that Engineering Managers need for quick assessment
 */
const RepositorySummaryCards: React.FC<RepositorySummaryCardsProps> = ({
  repositoryId,
  summary,
  loading = false,
  onRefresh,
  onNavigate,
}) => {
  // Handle metric card clicks for navigation
  const handleMetricClick = (metric: string) => {
    if (!onNavigate) return;

    const routes = {
      'health': `/repositories/${repositoryId}/analytics?tab=trends`,
      'contributors': `/repositories/${repositoryId}/analytics?tab=team`,
      'issues': `/repositories/${repositoryId}/analytics?tab=security`,
      'debt': `/repositories/${repositoryId}/analytics?tab=files`,
    };

    const route = routes[metric as keyof typeof routes];
    if (route) {
      onNavigate(route);
    }
  };

  return (
    <SummaryContainer>
      {/* Header with refresh button */}
      <SummaryHeader>
        <SummaryTitle>Repository Summary</SummaryTitle>
        {onRefresh && (
          <Tooltip title="Refresh data">
            <RefreshButton 
              onClick={onRefresh}
              disabled={loading}
              size="small"
            >
              <RefreshIcon fontSize="small" />
            </RefreshButton>
          </Tooltip>
        )}
      </SummaryHeader>

      {/* 4 Summary Cards Grid */}
      <CardsGrid container spacing={2}>
        {/* Card 1: Health Score */}
        <Grid item>
          <MetricCard
            label="Health Score"
            value={summary ? summary.healthScore.toFixed(1) : '--'}
            unit="%"
            trend={summary?.healthTrend ? {
              direction: summary.healthTrend.direction,
              delta: summary.healthTrend.delta,
              context: summary.healthTrend.context,
              positive: summary.healthTrend.positiveDirection as 'up' | 'down',
            } : undefined}
            healthScore={summary?.healthScore}
            loading={loading}
            onClick={onNavigate ? () => handleMetricClick('health') : undefined}
            aria-label="Repository health score - overall repository quality"
          />
        </Grid>

        {/* Card 2: Active Contributors */}
        <Grid item>
          <MetricCard
            label="Active Contributors"
            value={summary ? summary.activeContributors : '--'}
            loading={loading}
            onClick={onNavigate ? () => handleMetricClick('contributors') : undefined}
            aria-label="Active contributors in last 30 days"
          />
        </Grid>

        {/* Card 3: Critical Issues */}
        <Grid item>
          <MetricCard
            label="Critical Issues"
            value={summary ? summary.criticalIssues : '--'}
            loading={loading}
            onClick={onNavigate ? () => handleMetricClick('issues') : undefined}
            aria-label="Critical issues requiring attention"
          />
        </Grid>

        {/* Card 4: Technical Debt */}
        <Grid item>
          <MetricCard
            label="Technical Debt"
            value={summary ? summary.technicalDebtHours.toFixed(1) : '--'}
            unit="h"
            loading={loading}
            onClick={onNavigate ? () => handleMetricClick('debt') : undefined}
            aria-label="Technical debt estimated fix time in hours"
          />
        </Grid>
      </CardsGrid>

      {/* Last updated indicator */}
      {summary && (
        <Box sx={{ mt: 1, textAlign: 'center' }}>
          <Typography variant="caption" color="textSecondary">
            Last calculated: {new Date(summary.lastCalculated).toLocaleString()}
          </Typography>
        </Box>
      )}
    </SummaryContainer>
  );
};

export default RepositorySummaryCards;
