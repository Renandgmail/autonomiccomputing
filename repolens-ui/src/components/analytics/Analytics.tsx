import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Chip,
  LinearProgress,
  Skeleton,
  FormControlLabel,
  Switch
} from '@mui/material';
import {
  TrendingUp,
  Assessment,
  Code,
  Timeline,
  BugReport,
  Security,
  Speed
} from '@mui/icons-material';
import TrendChart from '../charts/TrendChart';
import apiService from '../../services/apiService';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface TrendData {
  date: string;
  value: number;
}

interface AnalyticsData {
  repositoryTrends?: {
    chartData: {
      linesOfCode: TrendData[];
      commits: TrendData[];
      contributors: TrendData[];
      qualityScore: TrendData[];
      techDebt: TrendData[];
      coverage: TrendData[];
    };
    trends: {
      linesOfCodeTrend: number;
      commitsTrend: number;
      contributorsTrend: number;
      qualityTrend: number;
    };
  };
  languageTrends?: {
    languages: { [key: string]: number };
    trends: { [key: string]: number };
  };
  activityPatterns?: {
    patterns: {
      hourly: { [hour: string]: number };
      daily: { [day: string]: number };
    };
  };
  history?: {
    dataPoints: TrendData[];
    data: any;
  };
}

const Analytics: React.FC = () => {
  const [repositories, setRepositories] = useState<Repository[]>([]);
  const [selectedRepository, setSelectedRepository] = useState<number | null>(null);
  const [timeRange, setTimeRange] = useState<number>(30);
  const [loading, setLoading] = useState(true);
  const [dataLoading, setDataLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [analyticsData, setAnalyticsData] = useState<AnalyticsData>({});
  const [autoRefresh, setAutoRefresh] = useState(false);

  // Load repositories on component mount
  useEffect(() => {
    loadRepositories();
  }, []);

  // Load analytics data when repository or time range changes
  useEffect(() => {
    if (selectedRepository) {
      loadAnalyticsData();
    }
  }, [selectedRepository, timeRange]);

  // Auto-refresh functionality
  useEffect(() => {
    if (autoRefresh && selectedRepository) {
      const interval = setInterval(() => {
        loadAnalyticsData();
      }, 30000); // Refresh every 30 seconds

      return () => clearInterval(interval);
    }
  }, [autoRefresh, selectedRepository]);

  const loadRepositories = async () => {
    try {
      setLoading(true);
      setError(null);
      const repos = await apiService.getRepositories();
      setRepositories(repos);
      
      // Auto-select first repository if available
      if (repos.length > 0 && !selectedRepository) {
        setSelectedRepository(repos[0].id);
      }
    } catch (err) {
      setError('Failed to load repositories. Please try again.');
      console.error('Error loading repositories:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadAnalyticsData = async () => {
    if (!selectedRepository) return;

    try {
      setDataLoading(true);
      setError(null);

      // Load all analytics data in parallel
      const [trendsData, languageData, activityData, historyData] = await Promise.allSettled([
        apiService.getRepositoryTrends(selectedRepository, timeRange),
        apiService.getLanguageTrends(selectedRepository, timeRange),
        apiService.getActivityPatterns(selectedRepository),
        apiService.getRepositoryHistory(selectedRepository, timeRange)
      ]);

      const analytics: AnalyticsData = {};

      if (trendsData.status === 'fulfilled') {
        analytics.repositoryTrends = trendsData.value;
      }

      if (languageData.status === 'fulfilled') {
        analytics.languageTrends = languageData.value;
      }

      if (activityData.status === 'fulfilled') {
        analytics.activityPatterns = activityData.value;
      }

      if (historyData.status === 'fulfilled') {
        analytics.history = historyData.value;
      }

      setAnalyticsData(analytics);

      // Log any failed requests
      [trendsData, languageData, activityData, historyData].forEach((result, index) => {
        if (result.status === 'rejected') {
          console.warn(`Analytics data load failed for endpoint ${index}:`, result.reason);
        }
      });

    } catch (err) {
      setError('Failed to load analytics data. Please try again.');
      console.error('Error loading analytics data:', err);
    } finally {
      setDataLoading(false);
    }
  };

  const formatTrendValue = (value: number | undefined, isPercentage = false): string => {
    // Handle undefined or null values
    if (value === undefined || value === null || isNaN(value)) {
      return isPercentage ? '0.0%' : '0';
    }
    
    if (isPercentage) {
      return `${value > 0 ? '+' : ''}${value.toFixed(1)}%`;
    }
    return value > 0 ? `+${value}` : value.toString();
  };

  const getTrendColor = (value: number): string => {
    return value >= 0 ? '#4caf50' : '#f44336';
  };

  const renderMetricCard = (title: string, value: string, trend?: number, icon?: React.ReactNode) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
          <Typography variant="h6" component="h3" color="text.secondary" fontSize="0.9rem">
            {title}
          </Typography>
          {icon && <Box color="primary.main">{icon}</Box>}
        </Box>
        <Typography variant="h4" component="h2" fontWeight="bold" mb={1}>
          {value}
        </Typography>
        {trend !== undefined && (
          <Box display="flex" alignItems="center">
            <TrendingUp 
              fontSize="small" 
              sx={{ 
                color: getTrendColor(trend),
                transform: trend < 0 ? 'rotate(180deg)' : 'none',
                mr: 0.5
              }} 
            />
            <Typography variant="body2" sx={{ color: getTrendColor(trend) }}>
              {formatTrendValue(trend, true)} vs last period
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );

  const renderLanguageDistribution = () => {
    const languages = analyticsData.languageTrends?.languages;
    if (!languages) return null;

    const total = Object.values(languages).reduce((sum, count) => sum + count, 0);
    const sortedLanguages = Object.entries(languages)
      .sort(([, a], [, b]) => b - a)
      .slice(0, 5); // Top 5 languages

    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Language Distribution
        </Typography>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {sortedLanguages.map(([language, count]) => {
            const percentage = ((count / total) * 100);
            const trend = analyticsData.languageTrends?.trends?.[language] || 0;
            
            return (
              <Box key={language}>
                <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
                  <Box display="flex" alignItems="center" gap={1}>
                    <Chip label={language} size="small" variant="outlined" />
                    <Typography variant="body2">
                      {percentage.toFixed(1)}%
                    </Typography>
                  </Box>
                  <Typography 
                    variant="body2" 
                    sx={{ color: getTrendColor(trend) }}
                  >
                    {formatTrendValue(trend, true)}
                  </Typography>
                </Box>
                <LinearProgress 
                  variant="determinate" 
                  value={percentage} 
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            );
          })}
        </Box>
      </Paper>
    );
  };

  const renderActivityHeatmap = () => {
    const patterns = analyticsData.activityPatterns?.patterns;
    if (!patterns) return null;

    const { hourly, daily } = patterns;

    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Activity Patterns
        </Typography>
        
        <Typography variant="subtitle2" gutterBottom sx={{ mt: 2 }}>
          Daily Activity
        </Typography>
        <Grid container spacing={1}>
          {Object.entries(daily).map(([day, activity]) => (
            <Grid item xs key={day}>
              <Box textAlign="center">
                <Typography variant="caption" display="block">
                  {day.slice(0, 3)}
                </Typography>
                <Box
                  sx={{
                    height: 40,
                    backgroundColor: `rgba(25, 118, 210, ${activity / 50})`, // Max opacity at 50 commits
                    borderRadius: 1,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mt: 0.5
                  }}
                >
                  <Typography variant="caption" color="white" fontWeight="bold">
                    {activity}
                  </Typography>
                </Box>
              </Box>
            </Grid>
          ))}
        </Grid>

        <Typography variant="subtitle2" gutterBottom sx={{ mt: 3 }}>
          Hourly Activity (Active Hours)
        </Typography>
        <Grid container spacing={1}>
          {Object.entries(hourly).map(([hour, activity]) => (
            <Grid item key={hour}>
              <Box textAlign="center">
                <Typography variant="caption" display="block">
                  {hour}:00
                </Typography>
                <Box
                  sx={{
                    width: 30,
                    height: 30,
                    backgroundColor: `rgba(76, 175, 80, ${activity / 30})`, // Max opacity at 30 commits
                    borderRadius: 1,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mt: 0.5
                  }}
                >
                  <Typography variant="caption" color="white" fontWeight="bold" fontSize="0.6rem">
                    {activity}
                  </Typography>
                </Box>
              </Box>
            </Grid>
          ))}
        </Grid>
      </Paper>
    );
  };

  const renderTrendCharts = () => {
    const chartData = analyticsData.repositoryTrends?.chartData;
    const trends = analyticsData.repositoryTrends?.trends;
    
    if (!chartData) return null;

    return (
      <>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <TrendChart
              data={chartData.linesOfCode || []}
              title="Lines of Code"
              color="#1976d2"
              yAxisLabel="Lines"
              height={300}
            />
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <TrendChart
              data={chartData.commits || []}
              title="Commit Activity"
              color="#4caf50"
              yAxisLabel="Commits"
              height={300}
            />
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <TrendChart
              data={chartData.qualityScore || []}
              title="Code Quality Score"
              color="#ff9800"
              yAxisLabel="Score (%)"
              height={300}
            />
          </Paper>
        </Grid>

        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <TrendChart
              data={chartData.coverage || []}
              title="Test Coverage"
              color="#9c27b0"
              yAxisLabel="Coverage (%)"
              height={300}
            />
          </Paper>
        </Grid>
      </>
    );
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (repositories.length === 0) {
    return (
      <Box textAlign="center" py={8}>
        <Assessment sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
        <Typography variant="h5" gutterBottom>
          No Repositories Found
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Add repositories to view analytics and trends.
        </Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
        <Typography variant="h4" component="h1">
          Repository Analytics
        </Typography>
        
        <Box display="flex" gap={2} alignItems="center">
          <FormControlLabel
            control={
              <Switch 
                checked={autoRefresh} 
                onChange={(e) => setAutoRefresh(e.target.checked)}
                size="small"
              />
            }
            label="Auto-refresh"
            sx={{ m: 0 }}
          />
          
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>Time Range</InputLabel>
            <Select
              value={timeRange}
              label="Time Range"
              onChange={(e) => setTimeRange(Number(e.target.value))}
            >
              <MenuItem value={7}>Last 7 days</MenuItem>
              <MenuItem value={30}>Last 30 days</MenuItem>
              <MenuItem value={90}>Last 3 months</MenuItem>
              <MenuItem value={180}>Last 6 months</MenuItem>
              <MenuItem value={365}>Last year</MenuItem>
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel>Repository</InputLabel>
            <Select
              value={selectedRepository || ''}
              label="Repository"
              onChange={(e) => setSelectedRepository(Number(e.target.value))}
            >
              {repositories.map((repo) => (
                <MenuItem key={repo.id} value={repo.id}>
                  {repo.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {selectedRepository && (
        <>
          {dataLoading && (
            <Box mb={2}>
              <LinearProgress />
            </Box>
          )}

          <Grid container spacing={3}>
            {/* Metrics Overview */}
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom>
                Key Metrics Overview
              </Typography>
            </Grid>

            {analyticsData.repositoryTrends?.trends ? (
              <>
                <Grid item xs={12} sm={6} md={3}>
                  {renderMetricCard(
                    "Lines of Code Trend",
                    formatTrendValue(analyticsData.repositoryTrends.trends.linesOfCodeTrend),
                    analyticsData.repositoryTrends.trends.linesOfCodeTrend,
                    <Code />
                  )}
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  {renderMetricCard(
                    "Commit Activity",
                    formatTrendValue(analyticsData.repositoryTrends.trends.commitsTrend),
                    analyticsData.repositoryTrends.trends.commitsTrend,
                    <Timeline />
                  )}
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  {renderMetricCard(
                    "Contributors",
                    formatTrendValue(analyticsData.repositoryTrends.trends.contributorsTrend),
                    analyticsData.repositoryTrends.trends.contributorsTrend,
                    <TrendingUp />
                  )}
                </Grid>
                <Grid item xs={12} sm={6} md={3}>
                  {renderMetricCard(
                    "Quality Score",
                    formatTrendValue(analyticsData.repositoryTrends.trends.qualityTrend, true),
                    analyticsData.repositoryTrends.trends.qualityTrend,
                    <Assessment />
                  )}
                </Grid>
              </>
            ) : dataLoading ? (
              <>
                {[1, 2, 3, 4].map((i) => (
                  <Grid item xs={12} sm={6} md={3} key={i}>
                    <Card sx={{ height: '100%' }}>
                      <CardContent>
                        <Skeleton variant="text" width="60%" />
                        <Skeleton variant="text" width="40%" height={40} sx={{ mt: 1 }} />
                        <Skeleton variant="text" width="80%" />
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </>
            ) : null}

            {/* Trend Charts */}
            {renderTrendCharts()}

            {/* Language Distribution and Activity Patterns */}
            <Grid item xs={12} md={6}>
              {dataLoading ? (
                <Paper sx={{ p: 3 }}>
                  <Skeleton variant="text" width="40%" height={32} />
                  <Skeleton variant="rectangular" width="100%" height={200} sx={{ mt: 2 }} />
                </Paper>
              ) : (
                renderLanguageDistribution()
              )}
            </Grid>

            <Grid item xs={12} md={6}>
              {dataLoading ? (
                <Paper sx={{ p: 3 }}>
                  <Skeleton variant="text" width="40%" height={32} />
                  <Skeleton variant="rectangular" width="100%" height={200} sx={{ mt: 2 }} />
                </Paper>
              ) : (
                renderActivityHeatmap()
              )}
            </Grid>
          </Grid>
        </>
      )}
    </Box>
  );
};

export default Analytics;
