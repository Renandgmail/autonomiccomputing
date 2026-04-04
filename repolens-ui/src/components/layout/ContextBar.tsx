/**
 * Context Bar Component
 * Persistent context bar for L2+ screens with breadcrumb navigation
 * Shows repository context, health status, and sync information
 */

import React from 'react';
import { 
  Box, 
  Breadcrumbs, 
  Typography, 
  Link, 
  IconButton,
  Chip
} from '@mui/material';
import { 
  Home, 
  ChevronRight, 
  Refresh, 
  Sync,
  FolderOpen,
  Code,
  Analytics,
  Search as SearchIcon
} from '@mui/icons-material';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import RepositoryHealthChip from '../RepositoryHealthChip';

interface Repository {
  id: number;
  name: string;
  healthScore?: number;
  lastSyncAt?: string;
  syncStatus?: 'synced' | 'syncing' | 'error';
}

interface ContextBarProps {
  repository?: Repository;
  onRefresh?: () => void;
}

export const ContextBar: React.FC<ContextBarProps> = ({
  repository,
  onRefresh
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { id: repoId, fileId } = useParams();

  // Generate breadcrumb based on current route
  const generateBreadcrumbs = () => {
    const segments = [];

    // Portfolio Dashboard (L1)
    segments.push({
      label: 'Portfolio',
      href: '/dashboard',
      icon: <Home sx={{ fontSize: 16 }} />
    });

    // Repository level (L2)
    if (repository) {
      segments.push({
        label: repository.name,
        href: `/repositories/${repository.id}`,
        icon: <FolderOpen sx={{ fontSize: 16 }} />
      });

      // Sub-pages (L3)
      if (location.pathname.includes('/analytics')) {
        segments.push({
          label: 'Analytics',
          href: `/repositories/${repository.id}/analytics`,
          icon: <Analytics sx={{ fontSize: 16 }} />
        });
      } else if (location.pathname.includes('/search')) {
        segments.push({
          label: 'Search',
          href: `/repositories/${repository.id}/search`,
          icon: <SearchIcon sx={{ fontSize: 16 }} />
        });
      } else if (location.pathname.includes('/files') && fileId) {
        segments.push({
          label: 'File Details',
          href: `/repositories/${repository.id}/files/${fileId}`,
          icon: <Code sx={{ fontSize: 16 }} />
        });
      }
    }

    return segments;
  };

  const breadcrumbs = generateBreadcrumbs();

  const getSyncStatusColor = (status?: string) => {
    switch (status) {
      case 'synced': return 'success';
      case 'syncing': return 'warning';
      case 'error': return 'error';
      default: return 'default';
    }
  };

  const getSyncStatusText = (status?: string, lastSync?: string) => {
    if (!status && !lastSync) return 'Unknown status';
    
    switch (status) {
      case 'synced': return `Synced ${formatTimeAgo(lastSync)}`;
      case 'syncing': return 'Syncing...';
      case 'error': return 'Sync failed';
      default: return lastSync ? `Last sync ${formatTimeAgo(lastSync)}` : 'Never synced';
    }
  };

  const formatTimeAgo = (dateString?: string): string => {
    if (!dateString) return 'unknown';
    
    try {
      const date = new Date(dateString);
      const now = new Date();
      const diffMs = now.getTime() - date.getTime();
      const diffMins = Math.floor(diffMs / 60000);
      
      if (diffMins < 1) return 'just now';
      if (diffMins < 60) return `${diffMins}m ago`;
      const diffHours = Math.floor(diffMins / 60);
      if (diffHours < 24) return `${diffHours}h ago`;
      const diffDays = Math.floor(diffHours / 24);
      if (diffDays < 7) return `${diffDays}d ago`;
      const diffWeeks = Math.floor(diffDays / 7);
      if (diffWeeks < 4) return `${diffWeeks}w ago`;
      return 'over a month ago';
    } catch (error) {
      return 'unknown';
    }
  };

  // Don't render if we're on L1 (Portfolio Dashboard)
  if (!repository) {
    return null;
  }

  return (
    <Box
      sx={{
        height: 48,
        backgroundColor: 'background.paper',
        borderBottom: '1px solid',
        borderColor: 'divider',
        display: 'flex',
        alignItems: 'center',
        px: 3,
        gap: 2,
        position: 'sticky',
        top: 56, // Below global navigation
        zIndex: 100
      }}
    >
      {/* Breadcrumbs */}
      <Box sx={{ flexGrow: 1 }}>
        <Breadcrumbs 
          separator={<ChevronRight fontSize="small" />}
          sx={{ fontSize: '14px' }}
        >
          {breadcrumbs.map((crumb, index) => {
            const isLast = index === breadcrumbs.length - 1;
            
            return (
              <Link
                key={index}
                href={crumb.href}
                onClick={(e) => {
                  e.preventDefault();
                  navigate(crumb.href);
                }}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 0.5,
                  color: isLast ? 'text.primary' : 'text.secondary',
                  textDecoration: 'none',
                  fontWeight: isLast ? 500 : 400,
                  '&:hover': {
                    textDecoration: 'underline',
                    color: 'primary.main'
                  },
                  cursor: 'pointer'
                }}
              >
                {crumb.icon}
                <Typography 
                  variant="body2" 
                  sx={{ 
                    fontSize: '14px',
                    fontWeight: 'inherit'
                  }}
                >
                  {crumb.label}
                </Typography>
              </Link>
            );
          })}
        </Breadcrumbs>
      </Box>

      {/* Repository Context (Health + Sync Status + Actions) */}
      <Box sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        gap: 1.5,
        flexShrink: 0
      }}>
        {/* Health Chip */}
        {repository.healthScore !== undefined && (
          <RepositoryHealthChip 
            healthScore={repository.healthScore}
            size="small"
          />
        )}

        {/* Sync Status */}
        <Chip
          label={getSyncStatusText(repository.syncStatus, repository.lastSyncAt)}
          size="small"
          color={getSyncStatusColor(repository.syncStatus)}
          variant="outlined"
          icon={repository.syncStatus === 'syncing' ? <Sync /> : undefined}
          sx={{
            fontSize: '11px',
            height: 24,
            '& .MuiChip-icon': {
              fontSize: 14,
              animation: repository.syncStatus === 'syncing' ? 'spin 1s linear infinite' : 'none'
            },
            '@keyframes spin': {
              '0%': { transform: 'rotate(0deg)' },
              '100%': { transform: 'rotate(360deg)' }
            }
          }}
        />

        {/* Refresh Button */}
        <IconButton
          size="small"
          onClick={onRefresh}
          disabled={repository.syncStatus === 'syncing'}
          sx={{
            padding: '4px',
            '&:disabled': {
              opacity: 0.5
            }
          }}
          title="Refresh repository data"
        >
          <Refresh sx={{ fontSize: 18 }} />
        </IconButton>
      </Box>
    </Box>
  );
};

export default ContextBar;
