/**
 * Main Layout Component
 * Updated to use Global Navigation + Context Bar architecture
 * Preserves all existing authentication and routing logic while switching to top navigation
 */

import React, { useState, useEffect } from 'react';
import { Outlet, useNavigate, useParams } from 'react-router-dom';
import { Box, useTheme, useMediaQuery } from '@mui/material';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import GlobalNavigation from './GlobalNavigation';
import ContextBar from './ContextBar';
import AIAssistantOverlay from '../ai/AIAssistantOverlay';
import apiService from '../../services/apiService';

interface Repository {
  id: number;
  name: string;
  healthScore?: number;
  lastSyncAt?: string;
  syncStatus?: 'synced' | 'syncing' | 'error';
}

const MainLayout: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { user, loading } = useAuth(); // REUSE existing auth logic
  const { id: repoId } = useParams();
  const navigate = useNavigate();

  // Repository state for context bar
  const [currentRepository, setCurrentRepository] = useState<Repository | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  // Load repository data when on repository pages
  useEffect(() => {
    const loadRepositoryContext = async () => {
      if (!repoId) {
        setCurrentRepository(null);
        return;
      }

      try {
        const repo = await apiService.getRepository(parseInt(repoId));
        
        // Adapt repository data to our context interface
        const adaptedRepo: Repository = {
          id: repo.id,
          name: repo.name,
          healthScore: repo.healthScore || calculateHealthScore(repo),
          lastSyncAt: repo.lastSyncAt,
          syncStatus: determineSyncStatus(repo)
        };

        setCurrentRepository(adaptedRepo);
      } catch (error) {
        console.error('Failed to load repository context:', error);
        setCurrentRepository(null);
      }
    };

    loadRepositoryContext();
  }, [repoId]);

  // Calculate health score if not provided
  const calculateHealthScore = (repo: any): number => {
    let score = 70; // Base score
    
    if (repo.codeQualityScore) {
      score = (score + repo.codeQualityScore) / 2;
    }
    
    if (repo.projectHealthScore) {
      score = repo.projectHealthScore;
    }
    
    // Adjust based on last sync
    if (repo.lastSyncAt) {
      const daysSinceSync = (Date.now() - new Date(repo.lastSyncAt).getTime()) / (1000 * 60 * 60 * 24);
      if (daysSinceSync > 7) score -= 10;
      if (daysSinceSync > 30) score -= 20;
    }
    
    return Math.max(0, Math.min(100, score));
  };

  // Determine sync status from repository data
  const determineSyncStatus = (repo: any): 'synced' | 'syncing' | 'error' => {
    if (repo.syncErrorMessage) return 'error';
    if (repo.status === 2 || repo.processingStatus === 2) return 'syncing'; // Status.Syncing or ProcessingStatus.InProgress
    return 'synced';
  };

  // Handle repository refresh
  const handleRepositoryRefresh = async () => {
    if (!currentRepository || refreshing) return;

    setRefreshing(true);
    try {
      await apiService.syncRepository(currentRepository.id);
      
      // Update sync status to syncing
      setCurrentRepository(prev => prev ? {
        ...prev,
        syncStatus: 'syncing'
      } : null);
      
      // Reload repository data after a short delay
      setTimeout(async () => {
        try {
          const repo = await apiService.getRepository(currentRepository.id);
          const adaptedRepo: Repository = {
            id: repo.id,
            name: repo.name,
            healthScore: repo.healthScore || calculateHealthScore(repo),
            lastSyncAt: repo.lastSyncAt,
            syncStatus: determineSyncStatus(repo)
          };
          setCurrentRepository(adaptedRepo);
        } catch (error) {
          console.error('Failed to reload repository after sync:', error);
        }
      }, 2000);
    } catch (error) {
      console.error('Failed to sync repository:', error);
      setCurrentRepository(prev => prev ? {
        ...prev,
        syncStatus: 'error'
      } : null);
    } finally {
      setRefreshing(false);
    }
  };

  // Handle mobile menu toggle (for future mobile sidebar implementation)
  const handleMobileMenuToggle = () => {
    // TODO: Implement mobile menu sidebar if needed
    console.log('Mobile menu toggle');
  };

  // PRESERVE all existing authentication logic
  if (!user && !loading) {
    return <Navigate to="/login" replace />;
  }

  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100vh',
          backgroundColor: 'background.default'
        }}
      >
        Loading...
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      {/* Global Navigation */}
      <GlobalNavigation onMobileMenuToggle={handleMobileMenuToggle} />
      
      {/* Context Bar (show on repository pages L2+) */}
      {currentRepository && (
        <ContextBar 
          repository={currentRepository}
          onRefresh={handleRepositoryRefresh}
        />
      )}
      
      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          mt: currentRepository ? '104px' : '56px', // Account for nav + context bar height
          overflow: 'auto',
          backgroundColor: 'background.default'
        }}
      >
        <Outlet />
      </Box>

      {/* AI Assistant Overlay - Available on all screens */}
      <AIAssistantOverlay />
    </Box>
  );
};

export default MainLayout;
