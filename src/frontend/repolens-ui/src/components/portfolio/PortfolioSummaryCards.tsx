/**
 * Portfolio Summary Cards - Zone 1 of L1 Dashboard
 * Exactly 4 metric cards as specified in L1_PORTFOLIO_DASHBOARD.md
 * Reuses existing MetricCard component for consistency
 */

import React from 'react';
import { Grid, Box, Skeleton } from '@mui/material';
import { TrendingUp, TrendingDown, TrendingFlat, Warning } from '@mui/icons-material';
import { PortfolioSummary } from '../../services/portfolioApiService';
import MetricCard from '../MetricCard';

interface PortfolioSummaryCardsProps {
  summary: PortfolioSummary | null;
  onCriticalIssuesClick?: () => void;
  loading?: boolean;
}

export const PortfolioSummaryCards: React.FC<PortfolioSummaryCardsProps> = ({
  summary,
  onCriticalIssuesClick,
  loading = false
}) => {
  // Loading skeleton when data is not available
  if (!summary) {
    return (
      <Grid container spacing={3}>
        {[1, 2, 3, 4].map((index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <Box sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
              <Skeleton variant="text" width="60%" height={24} />
              <Skeleton variant="text" width="40%" height={40} sx={{ mt: 1 }} />
              <Skeleton variant="text" width="80%" height={16} sx={{ mt: 1 }} />
            </Box>
          </Grid>
        ))}
      </Grid>
    );
  }

  // Helper function to get trend icon based on direction
  const getTrendIcon = (direction: string) => {
    switch (direction) {
      case 'up':
        return <TrendingUp fontSize="small" />;
      case 'down':
        return <TrendingDown fontSize="small" />;
      default:
        return <TrendingFlat fontSize="small" />;
    }
  };

  // Helper function to determine trend color
  const getTrendColor = (direction: string, isPositiveTrend: boolean) => {
    if (direction === 'flat') return 'text.secondary';
    
    // For health score, up is good. For critical issues, down is good.
    const isGood = isPositiveTrend ? direction === 'up' : direction === 'down';
    return isGood ? 'success.main' : 'error.main';
  };

  return (
    <Grid container spacing={3}>
      {/* Metric 1: Total Repositories */}
      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Repositories"
          value={summary.totalRepositories.toString()}
          loading={loading}
        />
      </Grid>

      {/* Metric 2: Average Health Score with Trend */}
      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Average Health"
          value={summary.averageHealthScore.toFixed(1)}
          unit="%"
          trend={{
            direction: summary.healthScoreTrend.direction as 'up' | 'down' | 'flat',
            delta: summary.healthScoreTrend.delta,
            context: summary.healthScoreTrend.context,
            positive: 'up'
          }}
          healthScore={summary.averageHealthScore}
          loading={loading}
        />
      </Grid>

      {/* Metric 3: Critical Issues Count - Clickable */}
      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Critical Issues"
          value={summary.criticalIssuesCount.toString()}
          loading={loading}
          onClick={summary.criticalIssuesCount > 0 ? onCriticalIssuesClick : undefined}
          healthScore={summary.criticalIssuesCount > 0 ? 25 : 95} // Red if issues, green if none
          aria-label={`${summary.criticalIssuesCount} critical issues${summary.criticalIssuesCount > 0 ? ' requiring attention' : ' - portfolio healthy'}`}
        />
      </Grid>

      {/* Metric 4: Active Teams */}
      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Active Teams"
          value={summary.activeTeamsCount.toString()}
          loading={loading}
        />
      </Grid>
    </Grid>
  );
};

export default PortfolioSummaryCards;
