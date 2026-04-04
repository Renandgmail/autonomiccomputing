import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  TextField,
  InputAdornment,
  Tabs,
  Tab,
  Paper,
  Chip,
  IconButton,
  Button,
  FormControlLabel,
  Radio,
  RadioGroup,
  CircularProgress,
  Alert,
  Collapse,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
  Skeleton,
  ListItemButton
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterIcon,
  Folder as FolderIcon,
  Person as PersonIcon,
  Analytics as AnalyticsIcon,
  Code as CodeIcon
} from '@mui/icons-material';

// Simplified interfaces for L3 Universal Search
interface SearchResult {
  id: string;
  type: 'file' | 'function' | 'class' | 'interface';
  title: string;
  description: string;
  filePath: string;
  language: string;
  qualityScore: number;
  lastModified: string;
  repositoryName?: string;
  repositoryId?: number;
  issues: {
    critical: number;
    high: number;
    medium: number;
    low: number;
  };
}

interface ContributorResult {
  id: string;
  name: string;
  email: string;
  avatar: string;
  repositoryName?: string;
  role: string;
  relevance: string;
  repositoryId?: number;
}

interface MetricResult {
  id: string;
  title: string;
  description: string;
  value: string;
  category: 'quality' | 'security' | 'complexity' | 'coverage';
  analyticsPath: string;
}

interface L3UniversalSearchProps {
  repositoryId?: number;
  repository?: {
    id: number;
    name: string;
  };
}

const L3UniversalSearch: React.FC<L3UniversalSearchProps> = ({
  repositoryId: propRepositoryId,
  repository: propRepository
}) => {
  const { repoId } = useParams<{ repoId: string }>();
  const navigate = useNavigate();
  
  const repositoryId = propRepositoryId || (repoId ? Number(repoId) : undefined);
  const [isGlobalSearch, setIsGlobalSearch] = useState(!repositoryId);
  const [query, setQuery] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentTab, setCurrentTab] = useState(0);
  const [showFilters, setShowFilters] = useState(false);
  const [searchHistory, setSearchHistory] = useState<string[]>([]);
  const [showHistory, setShowHistory] = useState(false);
  
  const searchInputRef = useRef<HTMLInputElement>(null);

  // Mock results for demonstration
  const [fileResults, setFileResults] = useState<SearchResult[]>([]);
  const [contributorResults, setContributorResults] = useState<ContributorResult[]>([]);
  const [metricResults, setMetricResults] = useState<MetricResult[]>([]);

  // Load search history
  useEffect(() => {
    const saved = localStorage.getItem('repolens-search-history');
    if (saved) {
      try {
        setSearchHistory(JSON.parse(saved).slice(0, 10));
      } catch (e) {
        console.warn('Failed to load search history:', e);
      }
    }
  }, []);

  // Global keyboard shortcut (Ctrl+K / Cmd+K)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        searchInputRef.current?.focus();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Generate mock results for development
  useEffect(() => {
    if (query.length > 2) {
      setIsLoading(true);
      
      setTimeout(() => {
        // Mock file results
        const mockFiles: SearchResult[] = Array.from({ length: 23 }, (_, i) => ({
          id: `file-${i}`,
          type: ['file', 'function', 'class', 'interface'][i % 4] as any,
          title: `AuthService.ts`,
          description: 'authenticateUser(), validateToken()',
          filePath: `frontend-app/src/auth/AuthService${i}.ts`,
          language: 'TypeScript',
          qualityScore: Math.floor(Math.random() * 100),
          lastModified: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
          repositoryName: isGlobalSearch ? ['repolens-frontend', 'analytics-service'][i % 2] : undefined,
          repositoryId: isGlobalSearch ? [1, 2][i % 2] : repositoryId,
          issues: {
            critical: Math.floor(Math.random() * 3),
            high: Math.floor(Math.random() * 3),
            medium: Math.floor(Math.random() * 5),
            low: Math.floor(Math.random() * 8)
          }
        }));

        // Mock contributor results
        const mockContributors: ContributorResult[] = Array.from({ length: 5 }, (_, i) => ({
          id: `contributor-${i}`,
          name: ['Alice Johnson', 'Bob Smith', 'Carol Wilson', 'David Brown', 'Eve Davis'][i],
          email: `user${i}@company.com`,
          avatar: '',
          repositoryName: isGlobalSearch ? 'repolens-frontend' : undefined,
          role: ['Author', 'Reviewer', 'Maintainer'][i % 3],
          relevance: `${85 + Math.floor(Math.random() * 15)}% of commits in auth module`,
          repositoryId: isGlobalSearch ? 1 : repositoryId
        }));

        // Mock metric results
        const mockMetrics: MetricResult[] = Array.from({ length: 12 }, (_, i) => ({
          id: `metric-${i}`,
          title: 'Authentication files: Average complexity 7.2',
          description: 'Based on 15 files in authentication modules',
          value: '7.2/10',
          category: ['quality', 'security', 'complexity', 'coverage'][i % 4] as any,
          analyticsPath: `/repos/${repositoryId || 1}/analytics`
        }));

        setFileResults(mockFiles);
        setContributorResults(mockContributors);
        setMetricResults(mockMetrics);
        setIsLoading(false);
      }, 300);
    } else {
      setFileResults([]);
      setContributorResults([]);
      setMetricResults([]);
    }
  }, [query, isGlobalSearch, repositoryId]);

  const handleScopeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const isGlobal = event.target.value === 'global';
    setIsGlobalSearch(isGlobal);
    
    if (isGlobal && repositoryId) {
      navigate('/search');
    } else if (!isGlobal && repositoryId) {
      navigate(`/repos/${repositoryId}/search`);
    }
  };

  const addToSearchHistory = (searchQuery: string) => {
    const newHistory = [searchQuery, ...searchHistory.filter(h => h !== searchQuery)].slice(0, 10);
    setSearchHistory(newHistory);
    localStorage.setItem('repolens-search-history', JSON.stringify(newHistory));
  };

  const handleFileResultClick = (result: SearchResult) => {
    const targetRepoId = result.repositoryId || repositoryId;
    navigate(`/repos/${targetRepoId}/files/${result.id}`);
  };

  const handleContributorResultClick = (result: ContributorResult) => {
    const targetRepoId = result.repositoryId || repositoryId;
    if (targetRepoId) {
      navigate(`/repos/${targetRepoId}/analytics/team`);
    }
  };

  const handleMetricResultClick = (result: MetricResult) => {
    navigate(result.analyticsPath);
  };

  const formatRelativeDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    return `${Math.floor(diffDays / 30)} months ago`;
  };

  const getIssueChips = (issues: SearchResult['issues']) => {
    const chips = [];
    if (issues.critical > 0) {
      chips.push(
        <Chip
          key="critical"
          label={`${issues.critical} Critical`}
          size="small"
          sx={{ backgroundColor: '#d32f2f', color: 'white' }}
        />
      );
    }
    if (issues.high > 0) {
      chips.push(
        <Chip
          key="high"
          label={`${issues.high} High`}
          size="small"
          sx={{ backgroundColor: '#ff9800', color: 'white' }}
        />
      );
    }
    if (issues.medium > 0) {
      chips.push(
        <Chip
          key="medium"
          label={`${issues.medium} Medium`}
          size="small"
          sx={{ backgroundColor: '#ffc107', color: 'white' }}
        />
      );
    }
    return chips.slice(0, 3);
  };

  return (
    <Box sx={{ maxWidth: '1200px', mx: 'auto', p: 3 }}>
      {/* Search Input */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <TextField
          ref={searchInputRef}
          fullWidth
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onFocus={() => setShowHistory(query === '' && searchHistory.length > 0)}
          onBlur={() => setTimeout(() => setShowHistory(false), 200)}
          placeholder="Search files, functions, patterns, or ask a question..."
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
            endAdornment: (
              <InputAdornment position="end">
                {isLoading && <CircularProgress size={20} sx={{ mr: 1 }} />}
                {query && (
                  <IconButton size="small" onClick={() => setQuery('')}>
                    <ClearIcon />
                  </IconButton>
                )}
              </InputAdornment>
            ),
          }}
          sx={{
            '& .MuiOutlinedInput-root': {
              fontSize: '1.1rem',
              '& fieldset': {
                borderColor: 'primary.main',
              },
            },
          }}
        />

        {/* Search History */}
        <Collapse in={showHistory}>
          <Box sx={{ mt: 2 }}>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              Recent searches
            </Typography>
            <Box display="flex" gap={1} flexWrap="wrap">
              {searchHistory.map((historyQuery, index) => (
                <Chip
                  key={index}
                  label={historyQuery}
                  size="small"
                  onClick={() => {
                    setQuery(historyQuery);
                    setShowHistory(false);
                    addToSearchHistory(historyQuery);
                  }}
                  clickable
                />
              ))}
            </Box>
          </Box>
        </Collapse>
      </Paper>

      {/* Scope Selector and Filters */}
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <RadioGroup
          value={isGlobalSearch ? 'global' : 'repository'}
          onChange={handleScopeChange}
          row
        >
          <FormControlLabel
            value="repository"
            control={<Radio />}
            label={`This repository${propRepository ? ` (${propRepository.name})` : ''}`}
            disabled={!repositoryId}
          />
          <FormControlLabel
            value="global"
            control={<Radio />}
            label="All repositories"
          />
        </RadioGroup>

        <Button
          startIcon={<FilterIcon />}
          onClick={() => setShowFilters(!showFilters)}
          variant="outlined"
          size="small"
        >
          Filters
        </Button>
      </Box>

      {/* Filters */}
      <Collapse in={showFilters}>
        <Paper sx={{ p: 2, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Search Filters
          </Typography>
          <Box display="flex" gap={2} flexWrap="wrap">
            <Chip label="Files" variant="outlined" size="small" />
            <Chip label="Last 30 days" variant="outlined" size="small" />
            <Button size="small" variant="text">+ Add filter</Button>
          </Box>
        </Paper>
      </Collapse>

      {/* Results Tabs */}
      {(query.length > 2 || isLoading) && (
        <Paper>
          <Tabs
            value={currentTab}
            onChange={(_, newValue) => setCurrentTab(newValue)}
            sx={{ borderBottom: 1, borderColor: 'divider' }}
          >
            <Tab label={`Files (${fileResults.length})`} icon={<FolderIcon />} iconPosition="start" />
            <Tab label={`Contributors (${contributorResults.length})`} icon={<PersonIcon />} iconPosition="start" />
            <Tab label={`Metrics (${metricResults.length})`} icon={<AnalyticsIcon />} iconPosition="start" />
          </Tabs>

          {/* Files Tab */}
          {currentTab === 0 && (
            <Box p={2}>
              {isLoading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <Box key={i} mb={2}>
                    <Skeleton variant="text" width="60%" height={24} />
                    <Skeleton variant="text" width="80%" height={20} />
                    <Skeleton variant="text" width="40%" height={16} />
                  </Box>
                ))
              ) : fileResults.length === 0 ? (
                <Typography variant="body2" color="text.secondary" p={2}>
                  No files found for "{query}"
                </Typography>
              ) : (
                <List disablePadding>
                  {fileResults.map((result, index) => (
                    <ListItem key={result.id} disablePadding>
                      <ListItemButton onClick={() => handleFileResultClick(result)}>
                        <ListItemAvatar>
                          <Avatar sx={{ bgcolor: 'primary.main' }}>
                            <CodeIcon />
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={
                            <Box>
                              {result.repositoryName && (
                                <Typography variant="caption" color="primary">
                                  {result.repositoryName} • 
                                </Typography>
                              )}
                              <Typography variant="body1" component="span" sx={{ fontFamily: 'monospace' }}>
                                {result.filePath}
                              </Typography>
                            </Box>
                          }
                          secondary={
                            <Box>
                              <Typography variant="body2" color="text.secondary" gutterBottom>
                                {result.description}
                              </Typography>
                              <Box display="flex" alignItems="center" gap={2} flexWrap="wrap">
                                <Typography variant="caption">Quality: {result.qualityScore}%</Typography>
                                <Typography variant="caption">{formatRelativeDate(result.lastModified)}</Typography>
                                <Box display="flex" gap={0.5}>{getIssueChips(result.issues)}</Box>
                              </Box>
                            </Box>
                          }
                        />
                      </ListItemButton>
                    </ListItem>
                  ))}
                </List>
              )}
            </Box>
          )}

          {/* Contributors Tab */}
          {currentTab === 1 && (
            <Box p={2}>
              {contributorResults.length === 0 ? (
                <Typography variant="body2" color="text.secondary" p={2}>
                  No contributors found for "{query}"
                </Typography>
              ) : (
                <List disablePadding>
                  {contributorResults.map((result) => (
                    <ListItem key={result.id} disablePadding>
                      <ListItemButton onClick={() => handleContributorResultClick(result)}>
                        <ListItemAvatar>
                          <Avatar src={result.avatar}><PersonIcon /></Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={
                            <Box>
                              {result.repositoryName && (
                                <Typography variant="caption" color="primary">
                                  {result.repositoryName} • 
                                </Typography>
                              )}
                              {result.name}
                              <Chip label={result.role} size="small" sx={{ ml: 1 }} />
                            </Box>
                          }
                          secondary={result.relevance}
                        />
                      </ListItemButton>
                    </ListItem>
                  ))}
                </List>
              )}
            </Box>
          )}

          {/* Metrics Tab */}
          {currentTab === 2 && (
            <Box p={2}>
              {metricResults.length === 0 ? (
                <Typography variant="body2" color="text.secondary" p={2}>
                  No metrics found for "{query}"
                </Typography>
              ) : (
                <List disablePadding>
                  {metricResults.map((result) => (
                    <ListItem key={result.id} disablePadding>
                      <ListItemButton onClick={() => handleMetricResultClick(result)}>
                        <ListItemText primary={result.title} secondary={result.description} />
                      </ListItemButton>
                    </ListItem>
                  ))}
                </List>
              )}
            </Box>
          )}

          {/* Empty State */}
          {!isLoading && query.length > 2 && 
           fileResults.length === 0 && contributorResults.length === 0 && metricResults.length === 0 && (
            <Box p={4} textAlign="center">
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No results for "{query}"
              </Typography>
              <Typography variant="body2" color="text.secondary" paragraph>
                Try different keywords or broaden your scope.
              </Typography>
              <Box display="flex" gap={2} justifyContent="center">
                {!isGlobalSearch && (
                  <Button variant="outlined" onClick={() => setIsGlobalSearch(true)}>
                    Search all repositories →
                  </Button>
                )}
                <Button variant="outlined" onClick={() => setShowFilters(false)}>
                  Clear filters →
                </Button>
              </Box>
            </Box>
          )}
        </Paper>
      )}
    </Box>
  );
};

export default L3UniversalSearch;
