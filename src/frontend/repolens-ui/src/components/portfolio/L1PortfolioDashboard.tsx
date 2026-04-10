/**
 * L1 Portfolio Dashboard - Engineering Manager Focus
 * 3-Zone Layout: Summary Cards | Repository List | Critical Issues Panel
 * Optimized for 90-second decision time as per requirements
 */

import React, { useState, useEffect } from 'react';
import { Box, Container, Grid, Typography, Alert, CircularProgress } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import portfolioApiService, { 
  PortfolioSummary, 
  PortfolioRepositoryListResponse, 
  CriticalIssue,
  PortfolioFilters,
  RepositoryHealthBand
} from '../../services/portfolioApiService';
import PortfolioSummaryCards from './PortfolioSummaryCards';
import RepositoryList from './RepositoryList';
import CriticalIssuesPanel from './CriticalIssuesPanel';

interface L1PortfolioDashboardProps {
  className?: string;
}

export const L1PortfolioDashboard: React.FC<L1PortfolioDashboardProps> = ({
  className
}) => {
  const navigate = useNavigate();
  
  // State management for 3 zones
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [repositoryData, setRepositoryData] = useState<PortfolioRepositoryListResponse | null>(null);
  const [criticalIssues, setCriticalIssues] = useState<CriticalIssue[]>([]);
  
  // Loading and error states
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Filters for Zone 2 (Repository List)
  const [filters, setFilters] = useState<Partial<PortfolioFilters>>({});
  
  // Auto-refresh timer
  const [lastRefresh, setLastRefresh] = useState<Date>(new Date());

  /**
   * Load all dashboard data - optimized for Engineering Manager workflow
   */
  const loadDashboardData = async (currentFilters: Partial<PortfolioFilters> = {}) => {
    try {
      setError(null);
      
      // Parallel loading for optimal performance (90-second goal)
      const [summaryData, repoListData, criticalIssuesData] = await Promise.all([
        portfolioApiService.getPortfolioSummary(),
        portfolioApiService.getRepositoryList(currentFilters),
        portfolioApiService.getCriticalIssues()
      ]);

      setSummary(summaryData);
      setRepositoryData(repoListData);
      setCriticalIssues(criticalIssuesData);
      setLastRefresh(new Date());
      
      console.log('📊 Portfolio dashboard loaded successfully', {
        repositories: repoListData.totalCount,
        criticalIssues: criticalIssuesData.length,
        avgHealth: summaryData.averageHealthScore
      });
      
    } catch (err: any) {
      console.error('Failed to load portfolio dashboard:', err);
      setError(err.message || 'Failed to load dashboard data. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Initial load
  useEffect(() => {
    loadDashboardData(filters);
  }, []);

  /**
   * Handle filter changes from Zone 2 repository list
   */
  const handleFiltersChange = (newFilters: Partial<PortfolioFilters>) => {
    setFilters(newFilters);
    setLoading(true);
    loadDashboardData(newFilters);
  };

  /**
   * Handle repository star/unstar from Zone 2
   */
  const handleRepositoryStarToggle = async (repositoryId: number, isStarred: boolean) => {
    try {
      await portfolioApiService.toggleRepositoryStar(repositoryId, isStarred);
      // Refresh repository list to update star status and sorting
      const updatedData = await portfolioApiService.getRepositoryList(filters);
      setRepositoryData(updatedData);
    } catch (err: any) {
      console.error('Failed to toggle repository star:', err);
      // Show error notification or handle gracefully
    }
  };

  /**
   * Handle critical issues filter - apply "critical issues only" filter
   */
  const handleCriticalIssuesFilter = () => {
    const criticalFilters: Partial<PortfolioFilters> = {
      ...filters,
      hasCriticalIssuesOnly: true
    };
    handleFiltersChange(criticalFilters);
  };

  /**
   * Handle repository click - navigate to L2 Repository Dashboard
   */
  const handleRepositoryClick = (repositoryId: number) => {
    navigate(`/repos/${repositoryId}`);
  };

  /**
   * Handle critical issue click - navigate directly to issue location
   */
  const handleCriticalIssueClick = (issue: CriticalIssue) => {
    if (issue.navigationRoute) {
      navigate(issue.navigationRoute);
    } else {
      // Fallback: navigate to repository dashboard
      navigate(`/repositories/${issue.repositoryId}`);
    }
  };

  /**
   * Manual refresh for Engineering Manager control
   */
  const handleRefresh = () => {
    setLoading(true);
    loadDashboardData(filters);
  };

  // Loading state for initial load
  if (loading && !summary) {
    return (
      <Container maxWidth="xl" sx={{ py: 3 }}>
        <Box 
          display="flex" 
          justifyContent="center" 
          alignItems="center" 
          minHeight="400px"
          flexDirection="column"
          gap={2}
        >
          <CircularProgress size={48} />
          <Typography variant="h6" color="text.secondary">
            Loading Portfolio Dashboard...
          </Typography>
        </Box>
      </Container>
    );
  }

  // Error state
  if (error) {
    return (
      <Container maxWidth="xl" sx={{ py: 3 }}>
        <Alert 
          severity="error" 
          action={
            <button onClick={handleRefresh}>
              Retry
            </button>
          }
          sx={{ mb: 3 }}
        >
          {error}
        </Alert>
      </Container>
    );
  }

  const hasCriticalIssues = criticalIssues.length > 0;

  return (
    <Container 
      maxWidth="xl" 
      sx={{ py: 3 }}
      className={className}
    >
      {/* Engineering Manager Dashboard Title */}
      <Box sx={{ mb: 4 }}>
        <Typography 
          variant="h4" 
          component="h1" 
          sx={{ 
            fontWeight: 600,
            color: 'text.primary',
            mb: 1
          }}
        >
          Portfolio Dashboard
        </Typography>
        <Typography 
          variant="body1" 
          color="text.secondary"
          sx={{ mb: 2 }}
        >
          Engineering team health overview • Last updated {lastRefresh.toLocaleTimeString()}
        </Typography>
      </Box>

      <Grid container spacing={3}>
        {/* Zone 1: Summary Cards (4 metrics exactly) */}
        <Grid item xs={12}>
          <PortfolioSummaryCards 
            summary={summary}
            onCriticalIssuesClick={handleCriticalIssuesFilter}
            loading={loading && !!summary} // Show loading overlay during refresh
          />
        </Grid>

        {/* Zone 2: Repository List (Main decision-making area) */}
        <Grid item xs={12} lg={hasCriticalIssues ? 8 : 12}>
          <RepositoryList
            data={repositoryData}
            filters={filters}
            onFiltersChange={handleFiltersChange}
            onRepositoryClick={handleRepositoryClick}
            onStarToggle={handleRepositoryStarToggle}
            loading={loading && !!repositoryData}
          />
        </Grid>

        {/* Zone 3: Critical Issues Panel (Conditional display) */}
        {hasCriticalIssues && (
          <Grid item xs={12} lg={4}>
            <CriticalIssuesPanel
              issues={criticalIssues}
              onIssueClick={handleCriticalIssueClick}
              onViewAllClick={handleCriticalIssuesFilter}
              totalCriticalRepositories={summary?.criticalIssuesCount || 0}
            />
          </Grid>
        )}
      </Grid>

      {/* Engineering Manager Success Message - No Critical Issues */}
      {!hasCriticalIssues && summary && (
        <Box sx={{ mt: 4, textAlign: 'center' }}>
          <Alert 
            severity="success" 
            sx={{ 
              maxWidth: 600, 
              mx: 'auto',
              backgroundColor: 'rgba(76, 175, 80, 0.1)',
              border: '1px solid rgba(76, 175, 80, 0.3)'
            }}
          >
            <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
              🎉 No Critical Issues Detected
            </Typography>
            <Typography variant="body2">
              Your portfolio is in good health. All repositories are meeting quality standards.
            </Typography>
          </Alert>
        </Box>
      )}

      {/* Debug Info for Development */}
      {process.env.NODE_ENV === 'development' && (
        <Box sx={{ mt: 4, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
          <Typography variant="caption" display="block">
            <strong>Debug Info:</strong> {repositoryData?.totalCount} repositories, 
            {criticalIssues.length} critical issues, 
            {summary?.averageHealthScore.toFixed(1)}% avg health
          </Typography>
        </Box>
      )}
    </Container>
  );
};

export default L1PortfolioDashboard;
