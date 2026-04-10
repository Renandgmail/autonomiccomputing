/**
 * Repository Switcher Component
 * Dropdown for quick repository switching with health indicators
 * Uses existing repository API and RepositoryHealthChip component
 */

import React, { useState, useEffect } from 'react';
import {
  Button,
  Menu,
  MenuItem,
  Box,
  Typography,
  TextField,
  InputAdornment,
  Divider,
  ListItemIcon,
  ListItemText,
  CircularProgress
} from '@mui/material';
import { 
  KeyboardArrowDown, 
  Search, 
  Star, 
  StarBorder,
  FolderOpen,
  Folder
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import RepositoryHealthChip from '../RepositoryHealthChip';
import apiService from '../../services/apiService';

interface Repository {
  id: number;
  name: string;
  healthScore?: number;
  isFavourite?: boolean;
  lastSyncAt?: string;
  updatedAt?: string;
}

export const RepositorySwitcher: React.FC = () => {
  const navigate = useNavigate();
  const { id: currentRepoId } = useParams();
  
  // REUSE existing repository state management from MainLayout and RepositoryDetails
  const [repositories, setRepositories] = useState<Repository[]>([]);
  const [currentRepo, setCurrentRepo] = useState<Repository | null>(null);
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // REUSE existing repository API from apiService
  useEffect(() => {
    const loadRepositories = async () => {
      try {
        setLoading(true);
        setError(null);
        const repos = await apiService.getRepositories();
        
        // Adapt repository data to our interface with health score calculation
        const adaptedRepos: Repository[] = repos.map(repo => ({
          id: repo.id,
          name: repo.name,
          healthScore: repo.healthScore || calculateHealthScore(repo),
          isFavourite: false, // TODO: Add favorite functionality to API
          lastSyncAt: repo.lastSyncAt,
          updatedAt: repo.updatedAt
        }));
        
        setRepositories(adaptedRepos);
        
        // Set current repository if on repo page
        if (currentRepoId) {
          const current = adaptedRepos.find(r => r.id === parseInt(currentRepoId));
          setCurrentRepo(current || null);
        }
      } catch (error) {
        console.error('Failed to load repositories:', error);
        setError('Failed to load repositories');
      } finally {
        setLoading(false);
      }
    };

    loadRepositories();
  }, [currentRepoId]);

  // Calculate basic health score if not provided by API
  const calculateHealthScore = (repo: any): number => {
    // Simple health calculation based on available metrics
    let score = 70; // Base score
    
    if (repo.codeQualityScore) {
      score = (score + repo.codeQualityScore) / 2;
    }
    
    if (repo.projectHealthScore) {
      score = repo.projectHealthScore;
    }
    
    // Adjust based on last sync (fresher data = higher score)
    if (repo.lastSyncAt) {
      const daysSinceSync = (Date.now() - new Date(repo.lastSyncAt).getTime()) / (1000 * 60 * 60 * 24);
      if (daysSinceSync > 7) score -= 10;
      if (daysSinceSync > 30) score -= 20;
    }
    
    return Math.max(0, Math.min(100, score));
  };

  // Filter repositories based on search
  const filteredRepositories = repositories.filter(repo =>
    repo.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Group repositories: Favorites -> Recent -> All
  const favourites = filteredRepositories.filter(r => r.isFavourite);
  const recent = filteredRepositories
    .filter(r => !r.isFavourite && r.lastSyncAt)
    .sort((a, b) => new Date(b.lastSyncAt!).getTime() - new Date(a.lastSyncAt!).getTime())
    .slice(0, 5);
  const others = filteredRepositories.filter(r => 
    !r.isFavourite && !recent.includes(r)
  );

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
    setSearchTerm('');
  };

  const handleRepositorySelect = (repository: Repository) => {
    navigate(`/repositories/${repository.id}`);
    setCurrentRepo(repository);
    handleClose();
  };

  const toggleFavourite = async (repoId: number, event: React.MouseEvent) => {
    event.stopPropagation();
    
    try {
      // TODO: Implement favorite toggle in API
      // await apiService.toggleRepositoryFavourite(repoId);
      
      // Update local state for now
      setRepositories(repos => 
        repos.map(r => 
          r.id === repoId ? { ...r, isFavourite: !r.isFavourite } : r
        )
      );
    } catch (error) {
      console.error('Failed to toggle favourite:', error);
    }
  };

  const formatLastSync = (lastSync: string | undefined): string => {
    if (!lastSync) return 'Never synced';
    
    const date = new Date(lastSync);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just synced';
    if (diffMins < 60) return `${diffMins}m ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}d ago`;
  };

  return (
    <>
      <Button
        onClick={handleOpen}
        endIcon={<KeyboardArrowDown />}
        sx={{
          color: 'inherit',
          textTransform: 'none',
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          mx: 1,
          minWidth: 'auto'
        }}
      >
        <FolderOpen sx={{ fontSize: 20 }} />
        <Box sx={{ 
          textAlign: 'left', 
          display: { xs: 'none', md: 'block' },
          minWidth: 0
        }}>
          <Typography variant="body2" sx={{ 
            lineHeight: 1.2,
            maxWidth: 150,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap'
          }}>
            {currentRepo?.name || 'Portfolio'}
          </Typography>
          {currentRepo?.healthScore !== undefined && (
            <RepositoryHealthChip 
              healthScore={currentRepo.healthScore}
              size="small"
            />
          )}
        </Box>
      </Button>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        PaperProps={{
          sx: {
            width: 360,
            maxHeight: 500,
            mt: 1
          }
        }}
      >
        {/* Search Input */}
        <Box sx={{ p: 2, pb: 1 }}>
          <TextField
            fullWidth
            size="small"
            placeholder="Search repositories..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search sx={{ fontSize: 20 }} />
                </InputAdornment>
              )
            }}
          />
        </Box>

        {/* Loading State */}
        {loading && (
          <Box sx={{ p: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
            <CircularProgress size={16} />
            <Typography variant="body2">Loading repositories...</Typography>
          </Box>
        )}

        {/* Error State */}
        {error && (
          <Box sx={{ p: 2 }}>
            <Typography variant="body2" color="error">
              {error}
            </Typography>
          </Box>
        )}

        {/* Repositories List */}
        {!loading && !error && (
          <>
            {/* Favourites */}
            {favourites.length > 0 && (
              <>
                <Box sx={{ px: 2, py: 1 }}>
                  <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                    FAVOURITES
                  </Typography>
                </Box>
                {favourites.map((repo) => (
                  <MenuItem
                    key={repo.id}
                    onClick={() => handleRepositorySelect(repo)}
                    sx={{ 
                      px: 2, 
                      py: 1,
                      '&:hover': {
                        backgroundColor: 'action.hover'
                      }
                    }}
                  >
                    <ListItemIcon>
                      <Star 
                        sx={{ 
                          color: 'warning.main', 
                          cursor: 'pointer',
                          fontSize: 20
                        }}
                        onClick={(e) => toggleFavourite(repo.id, e)}
                      />
                    </ListItemIcon>
                    <ListItemText 
                      primary={
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {repo.name}
                        </Typography>
                      }
                      secondary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                          <RepositoryHealthChip 
                            healthScore={repo.healthScore || 0}
                            size="small"
                          />
                          <Typography variant="caption" color="text.secondary">
                            • {formatLastSync(repo.lastSyncAt)}
                          </Typography>
                        </Box>
                      }
                    />
                  </MenuItem>
                ))}
                <Divider />
              </>
            )}

            {/* Recent */}
            {recent.length > 0 && (
              <>
                <Box sx={{ px: 2, py: 1 }}>
                  <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                    RECENT
                  </Typography>
                </Box>
                {recent.map((repo) => (
                  <MenuItem
                    key={repo.id}
                    onClick={() => handleRepositorySelect(repo)}
                    sx={{ 
                      px: 2, 
                      py: 1,
                      '&:hover': {
                        backgroundColor: 'action.hover'
                      }
                    }}
                  >
                    <ListItemIcon>
                      <StarBorder 
                        sx={{ 
                          cursor: 'pointer',
                          fontSize: 20,
                          color: 'text.secondary'
                        }}
                        onClick={(e) => toggleFavourite(repo.id, e)}
                      />
                    </ListItemIcon>
                    <ListItemText 
                      primary={
                        <Typography variant="body2" sx={{ fontWeight: 500 }}>
                          {repo.name}
                        </Typography>
                      }
                      secondary={
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                          <RepositoryHealthChip 
                            healthScore={repo.healthScore || 0}
                            size="small"
                          />
                          <Typography variant="caption" color="text.secondary">
                            • {formatLastSync(repo.lastSyncAt)}
                          </Typography>
                        </Box>
                      }
                    />
                  </MenuItem>
                ))}
              </>
            )}

            {/* Other Repositories */}
            {others.length > 0 && recent.length > 0 && <Divider />}
            {others.slice(0, 5).map((repo) => (
              <MenuItem
                key={repo.id}
                onClick={() => handleRepositorySelect(repo)}
                sx={{ 
                  px: 2, 
                  py: 1,
                  '&:hover': {
                    backgroundColor: 'action.hover'
                  }
                }}
              >
                <ListItemIcon>
                  <Folder sx={{ fontSize: 20, color: 'text.secondary' }} />
                </ListItemIcon>
                <ListItemText 
                  primary={
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                      {repo.name}
                    </Typography>
                  }
                  secondary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                      <RepositoryHealthChip 
                        healthScore={repo.healthScore || 0}
                        size="small"
                      />
                      <Typography variant="caption" color="text.secondary">
                        • {formatLastSync(repo.lastSyncAt)}
                      </Typography>
                    </Box>
                  }
                />
              </MenuItem>
            ))}

            {/* View All Link */}
            {repositories.length > 0 && (
              <>
                <Divider />
                <MenuItem 
                  onClick={() => { 
                    navigate('/repositories'); 
                    handleClose(); 
                  }}
                  sx={{
                    px: 2,
                    py: 1.5,
                    backgroundColor: 'action.hover',
                    '&:hover': {
                      backgroundColor: 'action.selected'
                    }
                  }}
                >
                  <ListItemText 
                    primary={
                      <Typography variant="body2" sx={{ 
                        textAlign: 'center', 
                        fontWeight: 500,
                        color: 'primary.main'
                      }}>
                        View all repositories →
                      </Typography>
                    }
                  />
                </MenuItem>
              </>
            )}

            {/* No Results */}
            {filteredRepositories.length === 0 && searchTerm && (
              <Box sx={{ p: 2, textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  No repositories found for "{searchTerm}"
                </Typography>
              </Box>
            )}
          </>
        )}
      </Menu>
    </>
  );
};

export default RepositorySwitcher;
