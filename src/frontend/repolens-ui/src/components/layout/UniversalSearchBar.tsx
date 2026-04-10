/**
 * Universal Search Bar Component
 * Extracts and adapts search logic from existing NaturalLanguageSearch component
 * Provides Ctrl+K shortcut and integrated search results dropdown
 */

import React, { useState, useEffect, useRef } from 'react';
import { 
  TextField, 
  InputAdornment, 
  Paper, 
  List, 
  ListItem, 
  ListItemButton,
  ListItemText,
  Chip,
  Box,
  ClickAwayListener,
  CircularProgress,
  Typography
} from '@mui/material';
import { Search } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useDebounce } from '../../hooks/useDebounce';
import apiService from '../../services/apiService';

interface SearchResult {
  id: number;
  type: string;
  title: string;
  description: string;
  filePath?: string;
  repositoryId?: number;
}

interface UniversalSearchBarProps {
  placeholder?: string;
  repositoryId?: number;  // For scoped search
  onSearchToggle?: (open: boolean) => void;
}

export const UniversalSearchBar: React.FC<UniversalSearchBarProps> = ({
  placeholder = "Search...",
  repositoryId,
  onSearchToggle
}) => {
  const navigate = useNavigate();
  const inputRef = useRef<HTMLInputElement>(null);
  
  // REUSE existing search state management from NaturalLanguageSearch
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  // REUSE existing search API integration with fallback to demo data
  useEffect(() => {
    const performSearch = async () => {
      if (debouncedSearchQuery.length < 2) {
        setSearchResults([]);
        setError(null);
        return;
      }
      
      setIsSearching(true);
      setError(null);
      
      try {
        // REUSE existing API service method with fallback
        const results = await apiService.processNaturalLanguageQuery(
          debouncedSearchQuery, 
          repositoryId,
          10 // Limit results for dropdown
        );
        
        // Adapt results to our interface
        const adaptedResults: SearchResult[] = (results.results || []).map((result: any, index: number) => ({
          id: result.id || index,
          type: result.type || 'File',
          title: result.title || result.filePath || 'Unknown',
          description: result.description || '',
          filePath: result.filePath,
          repositoryId: result.repositoryId || repositoryId
        }));
        
        setSearchResults(adaptedResults);
      } catch (error) {
        console.error('Search error:', error);
        setError('Search temporarily unavailable');
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    };

    performSearch();
  }, [debouncedSearchQuery, repositoryId]);

  // Keyboard shortcut handler (Ctrl+K)
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.metaKey || event.ctrlKey) && event.key === 'k') {
        event.preventDefault();
        inputRef.current?.focus();
        setSearchOpen(true);
        onSearchToggle?.(true);
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onSearchToggle]);

  const handleSearchFocus = () => {
    setSearchOpen(true);
    onSearchToggle?.(true);
  };

  const handleSearchBlur = () => {
    // Delay blur to allow click on results
    setTimeout(() => {
      setSearchOpen(false);
      onSearchToggle?.(false);
    }, 150);
  };

  const handleResultClick = (result: SearchResult) => {
    // Navigate based on result type
    if (result.type === 'repository') {
      navigate(`/repos/${result.id}`);
    } else if (result.filePath && result.repositoryId) {
      navigate(`/repos/${result.repositoryId}/files/${encodeURIComponent(result.filePath)}`);
    } else if (result.repositoryId) {
      navigate(`/repos/${result.repositoryId}`);
    }
    
    setSearchOpen(false);
    setSearchQuery('');
    onSearchToggle?.(false);
  };

  const getResultIcon = (type: string) => {
    // Simple text-based icons for different result types
    switch (type.toLowerCase()) {
      case 'method':
      case 'function':
        return '⚡';
      case 'class':
        return '📦';
      case 'interface':
        return '🔗';
      case 'file':
        return '📄';
      case 'repository':
        return '📁';
      default:
        return '🔍';
    }
  };

  return (
    <ClickAwayListener onClickAway={handleSearchBlur}>
      <Box sx={{ position: 'relative', width: '100%' }}>
        <TextField
          ref={inputRef}
          fullWidth
          size="small"
          placeholder={placeholder}
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          onFocus={handleSearchFocus}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search sx={{ color: 'text.secondary' }} />
              </InputAdornment>
            ),
            endAdornment: (
              <InputAdornment position="end">
                <Chip 
                  label="⌘K" 
                  size="small"
                  variant="outlined"
                  sx={{ 
                    height: 20, 
                    fontSize: '10px',
                    borderColor: 'divider'
                  }}
                />
              </InputAdornment>
            ),
            sx: {
              backgroundColor: 'rgba(255,255,255,0.1)',
              borderRadius: 1,
              '& .MuiOutlinedInput-notchedOutline': {
                borderColor: 'rgba(255,255,255,0.3)'
              },
              '&:hover .MuiOutlinedInput-notchedOutline': {
                borderColor: 'rgba(255,255,255,0.5)'
              },
              '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                borderColor: 'secondary.main'
              },
              '& input': {
                color: 'white',
                '&::placeholder': {
                  color: 'rgba(255,255,255,0.7)',
                  opacity: 1
                }
              }
            }
          }}
        />

        {/* Search Results Dropdown */}
        {searchOpen && (searchResults.length > 0 || isSearching || error) && (
          <Paper 
            sx={{
              position: 'absolute',
              top: '100%',
              left: 0,
              right: 0,
              mt: 1,
              zIndex: 1300,
              maxHeight: 400,
              overflow: 'auto',
              border: '1px solid',
              borderColor: 'divider'
            }}
          >
            {isSearching ? (
              <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
                <CircularProgress size={16} />
                <Typography variant="body2">Searching...</Typography>
              </Box>
            ) : error ? (
              <Box sx={{ p: 2 }}>
                <Typography variant="body2" color="error">
                  {error}
                </Typography>
              </Box>
            ) : searchResults.length > 0 ? (
              <List dense sx={{ py: 0 }}>
                {searchResults.map((result, index) => (
                  <ListItemButton
                    key={result.id || index}
                    onClick={() => handleResultClick(result)}
                    sx={{
                      '&:hover': {
                        backgroundColor: 'action.hover'
                      }
                    }}
                  >
                    <Box sx={{ mr: 1, fontSize: '16px' }}>
                      {getResultIcon(result.type)}
                    </Box>
                    <ListItemText 
                      primary={
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {result.title}
                        </Typography>
                      }
                      secondary={
                        <Typography variant="caption" color="text.secondary">
                          {result.type}{result.filePath ? ` • ${result.filePath}` : ''}
                        </Typography>
                      }
                    />
                  </ListItemButton>
                ))}
                
                {/* Show More Results Link */}
                {searchResults.length >= 10 && (
                  <ListItemButton
                    onClick={() => {
                      navigate(`/search?q=${encodeURIComponent(searchQuery)}`);
                      setSearchOpen(false);
                      setSearchQuery('');
                    }}
                    sx={{
                      borderTop: '1px solid',
                      borderColor: 'divider',
                      backgroundColor: 'action.hover'
                    }}
                  >
                    <ListItemText 
                      primary={
                        <Typography variant="body2" sx={{ textAlign: 'center', fontWeight: 500 }}>
                          View all results →
                        </Typography>
                      }
                    />
                  </ListItemButton>
                )}
              </List>
            ) : searchQuery.length >= 2 ? (
              <Box sx={{ p: 2 }}>
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center' }}>
                  No results found for "{searchQuery}"
                </Typography>
              </Box>
            ) : null}
          </Paper>
        )}
      </Box>
    </ClickAwayListener>
  );
};

export default UniversalSearchBar;
