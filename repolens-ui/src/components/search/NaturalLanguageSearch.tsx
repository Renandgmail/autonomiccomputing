import React, { useState, useEffect, useRef, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  TextField,
  IconButton,
  Typography,
  Chip,
  List,
  ListItem,
  ListItemText,
  Divider,
  Autocomplete,
  CircularProgress,
  Alert,
  Tooltip,
  Paper,
  Stack,
  Badge,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormControlLabel,
  Checkbox,
  Collapse,
  Button
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterIcon,
  Psychology as PsychologyIcon,
  Help as HelpIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Code as CodeIcon,
  Schedule as ScheduleIcon,
  Star as StarIcon
} from '@mui/icons-material';
import apiService from '../../services/apiService';

// Simple debounce function to avoid lodash dependency
const debounce = <T extends (...args: any[]) => any>(func: T, delay: number) => {
  let timeoutId: ReturnType<typeof setTimeout>;
  return (...args: Parameters<T>) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func(...args), delay);
  };
};

export interface SearchResult {
  id: number;
  type: string;
  title: string;
  description: string;
  filePath: string;
  language: string;
  startLine: number;
  endLine: number;
  relevanceScore: number;
  metadata: Record<string, any>;
  highlightedContent: string[];
}

interface QueryIntent {
  type: string;
  action: string;
  target: string;
  keywords: string[];
  entities: string[];
  confidence: number;
  parameters: Record<string, string>;
}

interface SearchFilters {
  languages: string[];
  fileExtensions: string[];
  elementTypes: string[];
  accessModifiers: string[];
  commonKeywords: string[];
  modificationDateRange: {
    earliest: string;
    latest: string;
  } | null;
}

interface SearchResponse {
  query: string;
  intent: QueryIntent;
  criteria: any;
  results: SearchResult[];
  summary: {
    totalCount: number;
    returnedCount: number;
    processingTime: string;
    confidenceScore: number;
  };
  suggestions: string[];
}

interface Props {
  repositoryId?: number;
  onResultSelect?: (result: SearchResult) => void;
  className?: string;
}

export const NaturalLanguageSearch: React.FC<Props> = ({
  repositoryId,
  onResultSelect,
  className
}) => {
  // State management
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [suggestions, setSuggestions] = useState<string[]>([]);
  const [filters, setFilters] = useState<SearchFilters | null>(null);
  const [selectedFilters, setSelectedFilters] = useState({
    languages: [] as string[],
    elementTypes: [] as string[],
    accessModifiers: [] as string[],
  });
  const [intent, setIntent] = useState<QueryIntent | null>(null);
  const [summary, setSummary] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isLoadingSuggestions, setIsLoadingSuggestions] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showFilters, setShowFilters] = useState(false);
  const [showExamples, setShowExamples] = useState(false);
  const [examples, setExamples] = useState<Record<string, string[]>>({});

  const searchInputRef = useRef<HTMLInputElement>(null);

  // Load initial data
  useEffect(() => {
    loadExamples();
    if (repositoryId) {
      loadFilters();
    }
  }, [repositoryId]);

  // Debounced suggestions using apiService
  const debouncedGetSuggestions = useCallback(
    debounce(async (partialQuery: string) => {
      if (partialQuery.length < 2) {
        setSuggestions([]);
        return;
      }

      setIsLoadingSuggestions(true);
      try {
        const result = await apiService.getSearchSuggestions(partialQuery, repositoryId, 10);
        setSuggestions(result.suggestions || []);
      } catch (error) {
        console.warn('Failed to load suggestions:', error);
        setSuggestions([]);
      } finally {
        setIsLoadingSuggestions(false);
      }
    }, 300),
    [repositoryId]
  );

  useEffect(() => {
    if (query.trim()) {
      debouncedGetSuggestions(query);
    } else {
      setSuggestions([]);
    }
  }, [query, debouncedGetSuggestions]);

  const loadFilters = async () => {
    if (!repositoryId) return;

    try {
      const result = await apiService.getSearchFilters(repositoryId);
      setFilters(result.filters);
    } catch (error) {
      console.warn('Failed to load filters:', error);
    }
  };

  const loadExamples = async () => {
    try {
      const result = await apiService.getExampleQueries();
      setExamples(result);
    } catch (error) {
      console.warn('Failed to load examples:', error);
    }
  };

  const executeSearch = async (searchQuery: string) => {
    if (!searchQuery.trim()) return;

    setIsLoading(true);
    setError(null);

    try {
      const result = await apiService.processNaturalLanguageQuery(searchQuery, repositoryId, 50);
      setResults(result.results);
      setIntent(result.intent);
      setSummary(result.summary);
      setSuggestions(result.suggestions);
    } catch (error: any) {
      setError(error.message || 'Failed to execute search');
      setResults([]);
      setIntent(null);
      setSummary(null);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    executeSearch(query);
  };

  const handleSuggestionSelect = (suggestion: string) => {
    setQuery(suggestion);
    executeSearch(suggestion);
    setSuggestions([]);
  };

  const handleExampleSelect = (example: string) => {
    setQuery(example);
    executeSearch(example);
    setShowExamples(false);
  };

  const clearSearch = () => {
    setQuery('');
    setResults([]);
    setIntent(null);
    setSummary(null);
    setSuggestions([]);
    setError(null);
    searchInputRef.current?.focus();
  };

  const getIntentIcon = (intentType: string) => {
    switch (intentType.toLowerCase()) {
      case 'find': return <SearchIcon />;
      case 'search': return <SearchIcon />;
      case 'list': return <CodeIcon />;
      case 'count': return '#';
      case 'analyze': return <PsychologyIcon />;
      default: return <SearchIcon />;
    }
  };

  const getIntentColor = (confidence: number) => {
    if (confidence >= 0.8) return 'success';
    if (confidence >= 0.6) return 'warning';
    return 'error';
  };

  const formatResultType = (type: string) => {
    return type.charAt(0).toUpperCase() + type.slice(1).toLowerCase();
  };

  return (
    <Box className={className} sx={{ width: '100%', maxWidth: '1200px', mx: 'auto' }}>
      {/* Search Header */}
      <Card elevation={2} sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h5" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <PsychologyIcon color="primary" />
            Natural Language Code Search
          </Typography>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Ask questions about your code in plain English. Try: "find all authentication methods" or "show me async functions"
          </Typography>

          {/* Search Input */}
          <Box component="form" onSubmit={handleSearchSubmit} sx={{ mt: 2 }}>
            <Box sx={{ position: 'relative' }}>
              <Autocomplete
                freeSolo
                options={suggestions}
                value={query}
                onInputChange={(_, newValue) => setQuery(newValue || '')}
                onChange={(_, value) => {
                  if (value && typeof value === 'string') {
                    handleSuggestionSelect(value);
                  }
                }}
                loading={isLoadingSuggestions}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    inputRef={searchInputRef}
                    fullWidth
                    placeholder="Ask about your code: find authentication, list all classes, count methods..."
                    variant="outlined"
                    InputProps={{
                      ...params.InputProps,
                      startAdornment: <SearchIcon sx={{ color: 'text.secondary', mr: 1 }} />,
                      endAdornment: (
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          {isLoadingSuggestions && <CircularProgress size={20} sx={{ mr: 1 }} />}
                          {query && (
                            <IconButton size="small" onClick={clearSearch} sx={{ mr: 1 }}>
                              <ClearIcon />
                            </IconButton>
                          )}
                          <IconButton 
                            type="submit" 
                            disabled={isLoading || !query.trim()}
                            size="small"
                          >
                            {isLoading ? <CircularProgress size={20} /> : <SearchIcon />}
                          </IconButton>
                        </Box>
                      ),
                    }}
                  />
                )}
                renderOption={(props, option) => (
                  <ListItem {...props} dense>
                    <ListItemText 
                      primary={option}
                      primaryTypographyProps={{ variant: 'body2' }}
                    />
                  </ListItem>
                )}
              />
            </Box>
          </Box>

          {/* Action Buttons */}
          <Box sx={{ mt: 2, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {repositoryId && (
              <Button
                size="small"
                variant="outlined"
                startIcon={<FilterIcon />}
                onClick={() => setShowFilters(!showFilters)}
              >
                Filters
              </Button>
            )}
            <Button
              size="small"
              variant="outlined"
              startIcon={<HelpIcon />}
              onClick={() => setShowExamples(!showExamples)}
            >
              Examples
            </Button>
            {intent && (
              <Tooltip title={`Intent: ${intent.type} (${Math.round(intent.confidence * 100)}% confidence)`}>
                <Chip
                  size="small"
                  label={`${intent.type}: ${intent.target}`}
                  color={getIntentColor(intent.confidence) as any}
                  variant="outlined"
                />
              </Tooltip>
            )}
          </Box>
        </CardContent>
      </Card>

      {/* Filters Panel */}
      {repositoryId && filters && (
        <Collapse in={showFilters}>
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Search Filters
              </Typography>
              <Grid container spacing={2}>
                {filters.languages.length > 0 && (
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Languages</InputLabel>
                      <Select
                        multiple
                        value={selectedFilters.languages}
                        onChange={(e) => setSelectedFilters(prev => ({
                          ...prev,
                          languages: e.target.value as string[]
                        }))}
                        renderValue={(selected) => (
                          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                            {(selected as string[]).map((value) => (
                              <Chip key={value} label={value} size="small" />
                            ))}
                          </Box>
                        )}
                      >
                        {filters.languages.map((lang) => (
                          <MenuItem key={lang} value={lang}>
                            <FormControlLabel
                              control={
                                <Checkbox 
                                  checked={selectedFilters.languages.includes(lang)}
                                  size="small"
                                />
                              }
                              label={lang}
                            />
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                )}
                {filters.elementTypes.length > 0 && (
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Element Types</InputLabel>
                      <Select
                        multiple
                        value={selectedFilters.elementTypes}
                        onChange={(e) => setSelectedFilters(prev => ({
                          ...prev,
                          elementTypes: e.target.value as string[]
                        }))}
                        renderValue={(selected) => (
                          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                            {(selected as string[]).map((value) => (
                              <Chip key={value} label={value} size="small" />
                            ))}
                          </Box>
                        )}
                      >
                        {filters.elementTypes.map((type) => (
                          <MenuItem key={type} value={type}>
                            <FormControlLabel
                              control={
                                <Checkbox 
                                  checked={selectedFilters.elementTypes.includes(type)}
                                  size="small"
                                />
                              }
                              label={formatResultType(type)}
                            />
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                )}
                {filters.accessModifiers.length > 0 && (
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Access Modifiers</InputLabel>
                      <Select
                        multiple
                        value={selectedFilters.accessModifiers}
                        onChange={(e) => setSelectedFilters(prev => ({
                          ...prev,
                          accessModifiers: e.target.value as string[]
                        }))}
                        renderValue={(selected) => (
                          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                            {(selected as string[]).map((value) => (
                              <Chip key={value} label={value} size="small" />
                            ))}
                          </Box>
                        )}
                      >
                        {filters.accessModifiers.map((modifier) => (
                          <MenuItem key={modifier} value={modifier}>
                            <FormControlLabel
                              control={
                                <Checkbox 
                                  checked={selectedFilters.accessModifiers.includes(modifier)}
                                  size="small"
                                />
                              }
                              label={modifier}
                            />
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                )}
              </Grid>
            </CardContent>
          </Card>
        </Collapse>
      )}

      {/* Examples Panel */}
      <Collapse in={showExamples}>
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Example Queries
            </Typography>
            <Grid container spacing={2}>
              {Object.entries(examples).map(([category, queries]) => (
                <Grid item xs={12} md={6} lg={4} key={category}>
                  <Typography variant="subtitle2" sx={{ textTransform: 'capitalize', mb: 1 }}>
                    {category}
                  </Typography>
                  <Stack spacing={1}>
                    {queries.map((example, index) => (
                      <Button
                        key={index}
                        variant="text"
                        size="small"
                        onClick={() => handleExampleSelect(example)}
                        sx={{ justifyContent: 'flex-start', textAlign: 'left' }}
                      >
                        {example}
                      </Button>
                    ))}
                  </Stack>
                </Grid>
              ))}
            </Grid>
          </CardContent>
        </Card>
      </Collapse>

      {/* Error Display */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Search Summary */}
      {summary && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={8}>
                <Typography variant="h6">
                  Search Results
                  <Chip 
                    label={`${summary.returnedCount} of ${summary.totalCount} results`}
                    size="small"
                    sx={{ ml: 1 }}
                  />
                </Typography>
                {intent && (
                  <Typography variant="body2" color="text.secondary">
                    Understood as: {intent.type.toLowerCase()} query for "{intent.target}"
                    {intent.keywords.length > 0 && (
                      <span> • Keywords: {intent.keywords.join(', ')}</span>
                    )}
                  </Typography>
                )}
              </Grid>
              <Grid item xs={12} md={4} sx={{ textAlign: { md: 'right' } }}>
                <Stack direction="row" spacing={2} justifyContent={{ xs: 'flex-start', md: 'flex-end' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <ScheduleIcon fontSize="small" color="action" />
                    <Typography variant="body2">{summary.processingTime}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <StarIcon fontSize="small" color="action" />
                    <Typography variant="body2">
                      {Math.round(summary.confidenceScore * 100)}% confidence
                    </Typography>
                  </Box>
                </Stack>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}

      {/* Results List */}
      {results.length > 0 && (
        <Card>
          <CardContent>
            <List disablePadding>
              {results.map((result, index) => (
                <React.Fragment key={result.id}>
                  {index > 0 && <Divider />}
                  <ListItem
                    component="div"
                    onClick={() => onResultSelect?.(result)}
                    sx={{ py: 2, cursor: 'pointer', '&:hover': { backgroundColor: 'action.hover' } }}
                  >
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                          <Chip 
                            label={formatResultType(result.type)} 
                            size="small" 
                            color="primary" 
                            variant="outlined"
                          />
                          <Typography variant="h6" component="span">
                            {result.title}
                          </Typography>
                          <Badge 
                            badgeContent={Math.round(result.relevanceScore * 100)} 
                            color="secondary"
                            sx={{ ml: 'auto' }}
                          />
                        </Box>
                      }
                      secondary={
                        <Box>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            {result.description}
                          </Typography>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
                            <Chip label={result.language} size="small" variant="outlined" />
                            <Typography variant="caption">
                              {result.filePath}
                              {result.startLine > 0 && ` • Lines ${result.startLine}-${result.endLine}`}
                            </Typography>
                            {result.metadata.AccessModifier && (
                              <Chip 
                                label={result.metadata.AccessModifier}
                                size="small"
                                variant="outlined"
                                color="info"
                              />
                            )}
                            {result.metadata.IsStatic && (
                              <Chip label="static" size="small" variant="outlined" color="warning" />
                            )}
                            {result.metadata.IsAsync && (
                              <Chip label="async" size="small" variant="outlined" color="success" />
                            )}
                          </Box>
                        </Box>
                      }
                    />
                  </ListItem>
                </React.Fragment>
              ))}
            </List>
          </CardContent>
        </Card>
      )}

      {/* No Results */}
      {!isLoading && query && results.length === 0 && !error && (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No results found
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Try refining your search or using different keywords.
          </Typography>
          {suggestions.length > 0 && (
            <Box>
              <Typography variant="body2" sx={{ mb: 1 }}>
                Suggestions:
              </Typography>
              <Stack direction="row" spacing={1} flexWrap="wrap" justifyContent="center">
                {suggestions.map((suggestion, index) => (
                  <Chip
                    key={index}
                    label={suggestion}
                    size="small"
                    onClick={() => handleSuggestionSelect(suggestion)}
                    clickable
                  />
                ))}
              </Stack>
            </Box>
          )}
        </Paper>
      )}
    </Box>
  );
};

export default NaturalLanguageSearch;
