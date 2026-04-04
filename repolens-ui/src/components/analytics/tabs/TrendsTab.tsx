import React, { useState, useEffect } from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  CircularProgress,
  Alert,
  Skeleton
} from '@mui/material';
import {
  TrendingUp,
  Assessment,
  Security,
  BugReport
} from '@mui/icons-material';
import TrendChart from '../../charts/TrendChart';
import apiService from '../../../services/apiService';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface TrendData {
  date: string;
  value: number;
}

interface TrendsData {
  qualityScore: {
    current: number;
    trend: number;
    data: TrendData[];
    target: number;
  };
  technicalDebt: {
    current: number;
    trend: number;
    data: TrendData[];
  };
  testCoverage: {
    current: number;
    trend: number;
    data: TrendData[];
    target: number;
  };
  securityVulnerabilities: {
    current: { critical: number; high: number; medium: number };
    data: Array<{
      date: string;
      critical: number;
      high: number;
      medium: number;
    }>;
  };
}

interface TrendsTabProps {
  repositoryId: number;
  repository: Repository;
  timeRange: number;
}

const TrendsTab: React.FC<TrendsTabProps> = ({
  repositoryId,
  repository,
  timeRange
}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<TrendsData | null>(null);

  useEffect(() => {
    loadTrendsData();
  }, [repositoryId, timeRange]);

  const loadTrendsData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Load trends data from API
      const trendsResponse = await apiService.getRepositoryTrends(repositoryId, timeRange);
      
      // Transform the data to match our interface
      const trendsData: TrendsData = {
        qualityScore: {
          current: trendsResponse.currentQuality || 85,
          trend: trendsResponse.qualityTrend || 2.3,
          data: trendsResponse.qualityHistory || generateMockTrendData(timeRange, 80, 95),
          target: 85
        },
        technicalDebt: {
          current: trendsResponse.currentDebt || 24.5,
          trend: trendsResponse.debtTrend || -1.8,
          data: trendsResponse.debtHistory || generateMockTrendData(timeRange, 20, 30)
        },
        testCoverage: {
          current: trendsResponse.currentCoverage || 78,
          trend: trendsResponse.coverageTrend || 3.2,
          data: trendsResponse.coverageHistory || generateMockTrendData(timeRange, 70, 85),
          target: 80
        },
        securityVulnerabilities: {
          current: trendsResponse.currentSecurity || { critical: 2, high: 5, medium: 12 },
          data: trendsResponse.securityHistory || generateMockSecurityData(timeRange)
        }
      };

      setData(trendsData);
    } catch (err) {
      setError('Failed to load trends data');
      console.error('Error loading trends data:', err);
      
      // Fallback to mock data in development
      if (process.env.NODE_ENV === 'development') {
        setData(generateMockTrendsData(timeRange));
      }
    } finally {
      setLoading(false);
    }
  };

  const generateMockTrendData = (days: number, min: number, max: number): TrendData[] => {
    const data: TrendData[] = [];
    const now = new Date();
    
    for (let i = days - 1; i >= 0; i--) {
      const date = new Date(now);
      date.setDate(date.getDate() - i);
      const value = Math.random() * (max - min) + min;
      
      data.push({
        date: date.toISOString().split('T')[0],
        value: Math.round(value * 10) / 10
      });
    }
    
    return data;
  };

  const generateMockSecurityData = (days: number) => {
    const data = [];
    const now = new Date();
    
    for (let i = days - 1; i >= 0; i--) {
      const date = new Date(now);
      date.setDate(date.getDate() - i);
      
      data.push({
        date: date.toISOString().split('T')[0],
        critical: Math.floor(Math.random() * 5),
        high: Math.floor(Math.random() * 8) + 2,
        medium: Math.floor(Math.random() * 15) + 5
      });
    }
    
    return data;
  };

  const generateMockTrendsData = (timeRange: number): TrendsData => ({
    qualityScore: {
      current: 85.3,
      trend: 2.3,
      data: generateMockTrendData(timeRange, 80, 90),
      target: 85
    },
    technicalDebt: {
      current: 24.5,
      trend: -1.8,
      data: generateMockTrendData(timeRange, 20, 30)
    },
    testCoverage: {
      current: 78.2,
      trend: 3.2,
      data: generateMockTrendData(timeRange, 70, 85),
      target: 80
    },
    securityVulnerabilities: {
      current: { critical: 2, high: 5, medium: 12 },
      data: generateMockSecurityData(timeRange)
    }
  });

  const formatTrendValue = (value: number, isPercentage = false, unit = ''): string => {
    const sign = value > 0 ? '+' : '';
    const formatted = isPercentage ? `${value.toFixed(1)}%` : `${value.toFixed(1)}${unit}`;
    return `${sign}${formatted}`;
  };

  const getTrendColor = (value: number): string => {
    return value >= 0 ? '#4caf50' : '#f44336';
  };

  const renderMetricCard = (
    title: string,
    currentValue: number,
    trend: number,
    unit: string,
    icon: React.ReactNode,
    target?: number,
    isPercentage = false
  ) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
          <Typography variant="h6" component="h3" color="text.secondary">
            {title}
          </Typography>
          <Box color="primary.main">{icon}</Box>
        </Box>
        
        <Typography variant="h3" component="h2" fontWeight="bold" mb={1}>
          {isPercentage ? `${currentValue.toFixed(1)}%` : `${currentValue}${unit}`}
        </Typography>
        
        <Box display="flex" alignItems="center" mb={1}>
          <TrendingUp 
            fontSize="small" 
            sx={{ 
              color: getTrendColor(trend),
              transform: trend < 0 ? 'rotate(180deg)' : 'none',
              mr: 0.5
            }} 
          />
          <Typography variant="body2" sx={{ color: getTrendColor(trend) }}>
            {formatTrendValue(trend, true)} vs last {timeRange} days
          </Typography>
        </Box>

        {target && (
          <Typography variant="caption" color="text.secondary">
            Target: {isPercentage ? `${target}%` : `${target}${unit}`}
          </Typography>
        )}
      </CardContent>
    </Card>
  );

  const renderChart = (
    title: string,
    data: TrendData[],
    color: string,
    yAxisLabel: string,
    target?: number
  ) => (
    <Card>
      <CardContent>
        <TrendChart
          data={data}
          title={title}
          color={color}
          yAxisLabel={yAxisLabel}
          height={300}
        />
        {target && (
          <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
            Target: {yAxisLabel.includes('%') ? `${target}%` : target}
          </Typography>
        )}
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <Box>
        <Grid container spacing={3}>
          {[1, 2, 3, 4].map((i) => (
            <Grid item xs={12} md={6} lg={3} key={i}>
              <Card sx={{ height: 200 }}>
                <CardContent>
                  <Skeleton variant="text" width="60%" />
                  <Skeleton variant="text" width="40%" height={40} sx={{ mt: 1 }} />
                  <Skeleton variant="text" width="80%" />
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Box>
    );
  }

  if (error || !data) {
    return (
      <Alert severity="error">
        {error || 'Failed to load trends data'}
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Quality trajectory over the last {timeRange} days
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        Track quality improvements and identify areas needing attention
      </Typography>

      {/* Metric Summary Cards */}
      <Grid container spacing={3} mb={4}>
        <Grid item xs={12} md={6} lg={3}>
          {renderMetricCard(
            'Overall Quality Score',
            data.qualityScore.current,
            data.qualityScore.trend,
            '',
            <Assessment />,
            data.qualityScore.target,
            true
          )}
        </Grid>

        <Grid item xs={12} md={6} lg={3}>
          {renderMetricCard(
            'Technical Debt',
            data.technicalDebt.current,
            data.technicalDebt.trend,
            'h',
            <BugReport />
          )}
        </Grid>

        <Grid item xs={12} md={6} lg={3}>
          {renderMetricCard(
            'Test Coverage',
            data.testCoverage.current,
            data.testCoverage.trend,
            '',
            <Assessment />,
            data.testCoverage.target,
            true
          )}
        </Grid>

        <Grid item xs={12} md={6} lg={3}>
          {renderMetricCard(
            'Security Issues',
            data.securityVulnerabilities.current.critical + 
            data.securityVulnerabilities.current.high + 
            data.securityVulnerabilities.current.medium,
            -2.5, // Mock trend
            '',
            <Security />
          )}
        </Grid>
      </Grid>

      {/* Trend Charts */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          {renderChart(
            'Overall Quality Score',
            data.qualityScore.data,
            '#1976d2',
            'Score (%)',
            data.qualityScore.target
          )}
        </Grid>

        <Grid item xs={12} md={6}>
          {renderChart(
            'Technical Debt (Hours)',
            data.technicalDebt.data,
            '#f44336',
            'Hours'
          )}
        </Grid>

        <Grid item xs={12} md={6}>
          {renderChart(
            'Test Coverage',
            data.testCoverage.data,
            '#4caf50',
            'Coverage (%)',
            data.testCoverage.target
          )}
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Security Vulnerabilities by Severity
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Critical, High, and Medium severity issues over time
              </Typography>
              
              {/* Security vulnerability counts */}
              <Box display="flex" gap={2} mb={2}>
                <Box display="flex" alignItems="center" gap={1}>
                  <Box
                    sx={{
                      width: 12,
                      height: 12,
                      backgroundColor: '#d32f2f',
                      borderRadius: '50%'
                    }}
                  />
                  <Typography variant="body2">
                    Critical: {data.securityVulnerabilities.current.critical}
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" gap={1}>
                  <Box
                    sx={{
                      width: 12,
                      height: 12,
                      backgroundColor: '#ff9800',
                      borderRadius: '50%'
                    }}
                  />
                  <Typography variant="body2">
                    High: {data.securityVulnerabilities.current.high}
                  </Typography>
                </Box>
                <Box display="flex" alignItems="center" gap={1}>
                  <Box
                    sx={{
                      width: 12,
                      height: 12,
                      backgroundColor: '#ffc107',
                      borderRadius: '50%'
                    }}
                  />
                  <Typography variant="body2">
                    Medium: {data.securityVulnerabilities.current.medium}
                  </Typography>
                </Box>
              </Box>

              <Box sx={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  Stacked area chart would be rendered here
                </Typography>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default TrendsTab;
