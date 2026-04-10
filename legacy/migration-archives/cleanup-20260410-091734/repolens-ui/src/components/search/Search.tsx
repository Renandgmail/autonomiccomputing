import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  TextField,
  InputAdornment,
  Card,
  CardContent,
  CardActions,
  Button,
  Chip,
  CircularProgress,
  Alert,
  Grid,
  Paper,
  IconButton,
  Autocomplete,
  Skeleton,
  Badge,
  Tooltip
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  OpenInNew as OpenIcon,
  Code as CodeIcon,
  Folder as FolderIcon,
  GitHub as GitHubIcon,
  Language as LanguageIcon,
  Star as StarIcon,
  Assessment as AssessmentIcon,
  History as HistoryIcon,
  TrendingUp
} from '@mui/icons-material';
import { searchRepositories, getSearchSuggestions } from '../../services/apiService';

interface SearchResult {
  type: 'repository';
  id: string;
  title: string;
  description: string;
  url: string;
  relevance: number;
  metadata: {
    status: string;
    isPrivate: boolean;
    lastSyncAt?: string;
    providerType: string;
  };
}

interface SearchResponse {
  query: string;
  page: number;
  pageSize: number;
  totalResults: number;
  totalPages: number;
  results: SearchResult[];
}

interface SuggestionResponse {
  suggestions: string[];
}

const Search: React.FC = () => {
  const [query, setQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [suggestionsLoading, setSuggestionsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(12);
  const [hasSearched, setHasSearched] = useState(false);
  const [recentSearches, setRecentSearches] = useState<string[]>([]);

  // Load recent searches from localStorage
  useEffect(() => {
    const saved = localStorage.getItem('repolens_recent_searches');
    if (saved) {
      try {
        setRecentSearches(JSON.parse(saved));
      } catch {
        // Ignore invalid saved data
      }
    }
  }, []);

  // Debounce search query
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(query.trim());
    }, 300);

    return () => clearTimeout(timer);
  }, [query]);

  // Load suggestions when query changes
  useEffect(() => {
    if (debouncedQuery.length >= 2) {
      loadSuggestions(debouncedQuery);
    } else {
      setSuggestions([]);
    }
  }, [debouncedQuery]);

  // Search when debounced query changes
  useEffect(() => {
    if (debouncedQuery.length >= 2) {
      performSearch(debouncedQuery, 1);
    } else if (debouncedQuery.length === 0) {
      setSearchResults(null);
      setHasSearched(false);
      setError(null);
    }
  }, [debouncedQuery]);

  const loadSuggestions = async (searchQuery: string) => {
    if (searchQuery.length < 2) return;

    try {
      setSuggestionsLoading(true);
      const response: SuggestionResponse = await getSearchSuggestions(searchQuery, 10);
      setSuggestions(response.suggestions || []);
    } catch (err) {
      console.warn('Failed to load suggestions:', err);
      setSuggestions([]);
    } finally {
      setSuggestionsLoading(false);
    }
  };

  const performSearch = async (searchQuery: string, searchPage: number) => {
    if (searchQuery.length < 2) return;

    try {
      setLoading(true);
      setError(null);
      setHasSearched(true);

      const response: SearchResponse = await searchRepositories(searchQuery, searchPage, pageSize);
      setSearchResults(response);
      setPage(searchPage);

      // Save to recent searches
      const updatedRecent = [searchQuery, ...recentSearches.filter(s => s !== searchQuery)].slice(0, 5);
      setRecentSearches(updatedRecent);
      localStorage.setItem('repolens_recent_searches', JSON.stringify(updatedRecent));

    } catch (err: any) {
      setError(err.message || 'Search failed. Please try again.');
      setSearchResults(null);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (searchQuery: string) => {
    setQuery(searchQuery);
    performSearch(searchQuery, 1);
  };

  const handlePageChange = (newPage: number) => {
    if (debouncedQuery) {
      performSearch(debouncedQuery, newPage);
    }
  };

  const handleClearSearch = () => {
    setQuery('');
    setSearchResults(null);
    setHasSearched(false);
    setError(null);
  };

  const getProviderIcon = (providerType: string) => {
    switch (providerType.toLowerCase()) {
      case 'github':
        return <GitHubIcon fontSize="small" />;
      case 'gitlab':
        return <LanguageIcon fontSize="small" />;
      case 'local':
        return <FolderIcon fontSize="small" />;
      default:
        return <CodeIcon fontSize="small" />;
    }
  };

  const getProviderColor = (providerType: string): string => {
    switch (providerType.toLowerCase()) {
      case 'github':
        return '#24292e';
      case 'gitlab':
        return '#fc6d26';
      case 'bitbucket':
        return '#0052cc';
      case 'azuredevops':
        return '#0078d4';
      case 'local':
        return '#757575';
      default:
        return '#1976d2';
    }
  };

  const getHealthScore = (result: SearchResult): number => {
    // Mock health score calculation based on metadata
    let score = 70; // Base score
    
    if (result.metadata.status === 'Active') score += 20;
    if (result.metadata.lastSyncAt) {
      const lastSync = new Date(result.metadata.lastSyncAt);
      const daysSince = (Date.now() - lastSync.getTime()) / (1000 * 60 * 60 * 24);
      if (daysSince < 7) score += 10;
    }
    
    return Math.min(100, Math.max(0, score));
  };

  const renderSearchResult = (result: SearchResult) => {
    const healthScore = getHealthScore(result);
    const providerColor = getProviderColor(result.metadata.providerType);

    return (
      <Card 
        key={result.id}
        sx={{ 
          height: '100%', 
          display: 'flex', 
          flexDirection: 'column',
          '&:hover': { 
            boxShadow: 3,
            transform: 'translateY(-2px)',
            transition: 'all 0.2s ease-in-out'
          }
        }}
      >
          <CardContent sx={{ flex: 1 }}>
            <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={1}>
              <Box display="flex" alignItems="center" gap={1} flex={1} minWidth={0}>
                {getProviderIcon(result.metadata.providerType)}
                <Typography 
                  variant="h6" 
                  component="h3" 
                  sx={{ 
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}
                >
                  {result.title}
                </Typography>
              </Box>
              <Box display="flex" gap={1} flexShrink={0}>
                <Chip
                  size="small"
                  label={result.metadata.providerType}
                  sx={{
                    backgroundColor: providerColor,
                    color: 'white',
                    fontSize: '0.75rem'
                  }}
                />
              </Box>
            </Box>

            <Typography 
              variant="body2" 
              color="text.secondary" 
              sx={{ 
                mb: 2,
                display: '-webkit-box',
                WebkitLineClamp: 2,
                WebkitBoxOrient: 'vertical',
                overflow: 'hidden'
              }}
            >
              {result.description}
            </Typography>

            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Box display="flex" gap={1}>
                <Chip
                  size="small"
                  label={result.metadata.status}
                  color={result.metadata.status === 'Active' ? 'success' : 'default'}
                  variant="outlined"
                />
                {result.metadata.isPrivate && (
                  <Chip size="small" label="Private" variant="outlined" />
                )}
              </Box>
              
              <Tooltip title={`Health Score: ${healthScore}%`}>
                <Box display="flex" alignItems="center" gap={0.5}>
                  <AssessmentIcon fontSize="small" color="primary" />
                  <Typography variant="body2" fontWeight="bold">
                    {healthScore}%
                  </Typography>
                </Box>
              </Tooltip>
            </Box>

            {result.metadata.lastSyncAt && (
              <Box display="flex" alignItems="center" gap={0.5} mb={1}>
                <HistoryIcon fontSize="small" color="action" />
                <Typography variant="caption" color="text.secondary">
                  Last sync: {new Date(result.metadata.lastSyncAt).toLocaleDateString()}
                </Typography>
              </Box>
            )}

            <Box display="flex" alignItems="center" gap={0.5}>
              <TrendingUp fontSize="small" color="action" />
              <Typography variant="caption" color="text.secondary">
                Relevance: {result.relevance}%
              </Typography>
            </Box>
          </CardContent>

          <CardActions>
            <Button
              size="small"
              startIcon={<OpenIcon />}
              href={result.url}
              target="_blank"
              rel="noopener noreferrer"
            >
              View Repository
            </Button>
            <Button size="small" color="primary">
              View Details
            </Button>
          </CardActions>
        </Card>
    );
  };

  const renderEmptyState = () => {
    if (!hasSearched) {
      return (
        <Paper sx={{ p: 6, textAlign: 'center', mt: 4 }}>
          <SearchIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h5" gutterBottom>
            Search Repositories
          </Typography>
          <Typography variant="body1" color="text.secondary" mb={4}>
            Find repositories by name, description, or content. Start typing to see suggestions.
          </Typography>

          {recentSearches.length > 0 && (
            <Box>
              <Typography variant="subtitle2" gutterBottom>
                Recent Searches
              </Typography>
              <Box display="flex" gap={1} justifyContent="center" flexWrap="wrap">
                {recentSearches.map((recent, index) => (
                  <Chip
                    key={index}
                    label={recent}
                    size="small"
                    onClick={() => handleSearch(recent)}
                    clickable
                  />
                ))}
              </Box>
            </Box>
          )}
        </Paper>
      );
    }

    if (searchResults?.totalResults === 0) {
      return (
        <Paper sx={{ p: 6, textAlign: 'center', mt: 4 }}>
          <SearchIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h5" gutterBottom>
            No Results Found
          </Typography>
          <Typography variant="body1" color="text.secondary" mb={2}>
            No repositories match your search for "{debouncedQuery}"
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Try searching with different keywords or check the spelling.
          </Typography>
        </Paper>
      );
    }

    return null;
  };

  const renderPagination = () => {
    if (!searchResults || searchResults.totalPages <= 1) return null;

    const startPage = Math.max(1, page - 2);
    const endPage = Math.min(searchResults.totalPages, page + 2);
    const pages = Array.from({ length: endPage - startPage + 1 }, (_, i) => startPage + i);

    return (
      <Box display="flex" justifyContent="center" alignItems="center" gap={1} mt={4}>
        <Button
          disabled={page === 1}
          onClick={() => handlePageChange(page - 1)}
          size="small"
        >
          Previous
        </Button>
        
        {pages.map((pageNum) => (
          <Button
            key={pageNum}
            variant={page === pageNum ? 'contained' : 'text'}
            onClick={() => handlePageChange(pageNum)}
            size="small"
            sx={{ minWidth: 40 }}
          >
            {pageNum}
          </Button>
        ))}
        
        <Button
          disabled={page === searchResults.totalPages}
          onClick={() => handlePageChange(page + 1)}
          size="small"
        >
          Next
        </Button>
      </Box>
    );
  };

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom>
        Search & Discovery
      </Typography>
      
      <Box sx={{ maxWidth: 600, mb: 4 }}>
        <Autocomplete
          freeSolo
          options={suggestions}
          loading={suggestionsLoading}
          value={query}
          onInputChange={(event, newValue) => setQuery(newValue || '')}
          renderInput={(params) => (
            <TextField
              {...params}
              fullWidth
              placeholder="Search repositories by name, description, or content..."
              variant="outlined"
              InputProps={{
                ...params.InputProps,
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
                endAdornment: (
                  <InputAdornment position="end">
                    {loading && <CircularProgress size={20} />}
                    {query && (
                      <IconButton onClick={handleClearSearch} size="small">
                        <ClearIcon />
                      </IconButton>
                    )}
                    {params.InputProps.endAdornment}
                  </InputAdornment>
                ),
              }}
            />
          )}
          renderOption={(props, option) => (
            <Box component="li" {...props}>
              <SearchIcon fontSize="small" sx={{ mr: 1, color: 'text.secondary' }} />
              {option}
            </Box>
          )}
        />
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {searchResults && searchResults.totalResults > 0 && (
        <Box mb={3}>
          <Typography variant="body1" color="text.secondary">
            Found {searchResults.totalResults.toLocaleString()} results for "{searchResults.query}"
            {searchResults.totalPages > 1 && (
              <span> (Page {page} of {searchResults.totalPages})</span>
            )}
          </Typography>
        </Box>
      )}

      {loading && hasSearched ? (
        <Box 
          sx={{ 
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', lg: 'repeat(3, 1fr)' },
            gap: 3
          }}
        >
          {Array.from({ length: pageSize }, (_, i) => (
            <Card key={i}>
              <CardContent>
                <Box display="flex" alignItems="center" gap={1} mb={1}>
                  <Skeleton variant="circular" width={24} height={24} />
                  <Skeleton variant="text" width="60%" />
                </Box>
                <Skeleton variant="text" width="100%" />
                <Skeleton variant="text" width="80%" />
                <Box display="flex" gap={1} mt={2}>
                  <Skeleton variant="rectangular" width={60} height={24} />
                  <Skeleton variant="rectangular" width={60} height={24} />
                </Box>
              </CardContent>
            </Card>
          ))}
        </Box>
      ) : searchResults && searchResults.results.length > 0 ? (
        <>
          <Box 
            sx={{ 
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', lg: 'repeat(3, 1fr)' },
              gap: 3
            }}
          >
            {searchResults.results.map(renderSearchResult)}
          </Box>
          {renderPagination()}
        </>
      ) : (
        renderEmptyState()
      )}
    </Box>
  );
};

export default Search;
