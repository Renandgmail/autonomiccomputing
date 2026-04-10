import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Typography,
  Tabs,
  Tab,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert
} from '@mui/material';
import {
  TrendingUp,
  TableChart,
  Group,
  Security,
  Compare
} from '@mui/icons-material';

// Import tab components
import TrendsTab from './tabs/TrendsTab';
import FilesTab from './tabs/FilesTab';
import TeamTab from './tabs/TeamTab';
import SecurityTab from './tabs/SecurityTab';
import CompareTab from './tabs/CompareTab';

import apiService from '../../services/apiService';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`analytics-tabpanel-${index}`}
      aria-labelledby={`analytics-tab-${index}`}
    >
      {value === index && <Box>{children}</Box>}
    </div>
  );
}

const L3Analytics: React.FC = () => {
  const { repoId } = useParams<{ repoId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  
  const [repository, setRepository] = useState<Repository | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Tab management - derive from URL path
  const getTabFromPath = (): number => {
    const path = location.pathname;
    if (path.includes('/analytics/files')) return 1;
    if (path.includes('/analytics/team')) return 2;
    if (path.includes('/analytics/security')) return 3;
    if (path.includes('/analytics/compare')) return 4;
    return 0; // Default to Trends
  };

  const [currentTab, setCurrentTab] = useState(getTabFromPath());

  // Time range state - persists per user session
  const [timeRange, setTimeRange] = useState(() => {
    const saved = sessionStorage.getItem('repolens-analytics-timerange');
    return saved ? Number(saved) : 30;
  });

  useEffect(() => {
    if (repoId) {
      loadRepository();
    }
  }, [repoId]);

  // Update tab when URL changes
  useEffect(() => {
    setCurrentTab(getTabFromPath());
  }, [location.pathname]);

  // Persist time range selection
  useEffect(() => {
    sessionStorage.setItem('repolens-analytics-timerange', timeRange.toString());
  }, [timeRange]);

  const loadRepository = async () => {
    if (!repoId) return;

    try {
      setLoading(true);
      setError(null);
      
      const repo = await apiService.getRepository(Number(repoId));
      setRepository(repo);
    } catch (err) {
      setError('Failed to load repository. Please try again.');
      console.error('Error loading repository:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    // Client-side navigation only - no full page reload
    const tabRoutes = [
      `/repos/${repoId}/analytics`,
      `/repos/${repoId}/analytics/files`,
      `/repos/${repoId}/analytics/team`,
      `/repos/${repoId}/analytics/security`,
      `/repos/${repoId}/analytics/compare`
    ];

    navigate(tabRoutes[newValue], { replace: true });
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (error || !repository) {
    return (
      <Box py={8}>
        <Alert severity="error">
          {error || 'Repository not found'}
        </Alert>
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            {repository.name} Analytics
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Quality, team, and security insights for this repository
          </Typography>
        </Box>

        {/* Time Range Picker - Only show on relevant tabs */}
        {(currentTab === 0 || currentTab === 2) && (
          <FormControl size="small" sx={{ minWidth: 150 }}>
            <InputLabel>Time Range</InputLabel>
            <Select
              value={timeRange}
              label="Time Range"
              onChange={(e) => setTimeRange(Number(e.target.value))}
            >
              <MenuItem value={7}>Last 7 days</MenuItem>
              <MenuItem value={30}>Last 30 days</MenuItem>
              <MenuItem value={90}>Last 90 days</MenuItem>
              <MenuItem value={365}>Custom date range</MenuItem>
            </Select>
          </FormControl>
        )}
      </Box>

      {/* Tab Navigation */}
      <Paper sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs
          value={currentTab}
          onChange={handleTabChange}
          aria-label="Analytics tabs"
          sx={{
            '& .MuiTab-root': {
              minWidth: { xs: 'auto', sm: 120 },
              fontSize: { xs: '0.8rem', sm: '0.875rem' }
            }
          }}
          variant="scrollable"
          scrollButtons="auto"
        >
          <Tab
            label="Trends"
            icon={<TrendingUp />}
            iconPosition="start"
            sx={{ textTransform: 'none' }}
            id="analytics-tab-0"
            aria-controls="analytics-tabpanel-0"
          />
          <Tab
            label="Files"
            icon={<TableChart />}
            iconPosition="start"
            sx={{ textTransform: 'none' }}
            id="analytics-tab-1"
            aria-controls="analytics-tabpanel-1"
          />
          <Tab
            label="Team"
            icon={<Group />}
            iconPosition="start"
            sx={{ textTransform: 'none' }}
            id="analytics-tab-2"
            aria-controls="analytics-tabpanel-2"
          />
          <Tab
            label="Security"
            icon={<Security />}
            iconPosition="start"
            sx={{ textTransform: 'none' }}
            id="analytics-tab-3"
            aria-controls="analytics-tabpanel-3"
          />
          <Tab
            label="Compare"
            icon={<Compare />}
            iconPosition="start"
            sx={{ textTransform: 'none' }}
            id="analytics-tab-4"
            aria-controls="analytics-tabpanel-4"
          />
        </Tabs>
      </Paper>

      {/* Tab Content */}
      <TabPanel value={currentTab} index={0}>
        <TrendsTab 
          repositoryId={Number(repoId)} 
          repository={repository}
          timeRange={timeRange} 
        />
      </TabPanel>

      <TabPanel value={currentTab} index={1}>
        <FilesTab 
          repositoryId={Number(repoId)} 
          repository={repository}
        />
      </TabPanel>

      <TabPanel value={currentTab} index={2}>
        <TeamTab 
          repositoryId={Number(repoId)} 
          repository={repository}
          timeRange={timeRange} 
        />
      </TabPanel>

      <TabPanel value={currentTab} index={3}>
        <SecurityTab 
          repositoryId={Number(repoId)} 
          repository={repository}
        />
      </TabPanel>

      <TabPanel value={currentTab} index={4}>
        <CompareTab 
          repositoryId={Number(repoId)} 
          repository={repository}
        />
      </TabPanel>
    </Box>
  );
};

export default L3Analytics;
