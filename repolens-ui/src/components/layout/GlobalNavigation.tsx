/**
 * Global Navigation Component
 * Professional enterprise top navigation bar with universal search and repository switching
 * Replaces sidebar navigation while preserving all existing functionality
 */

import React, { useState } from 'react';
import { AppBar, Toolbar, Box, IconButton, Avatar, Badge, Typography, Select, MenuItem, FormControl } from '@mui/material';
import { Search, Notifications, Menu, SwapHoriz } from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import UniversalSearchBar from './UniversalSearchBar';
import RepositorySwitcher from './RepositorySwitcher';

// User mode types for stakeholder-driven navigation
type UserMode = 'portfolio' | 'developer' | 'devops' | 'security' | 'product' | 'expert';

interface ModeInfo {
  value: UserMode;
  label: string;
  icon: string;
  description: string;
}

const USER_MODES: ModeInfo[] = [
  { value: 'portfolio', label: 'Manager View', icon: '👔', description: 'Team oversight & resource management' },
  { value: 'developer', label: 'Developer View', icon: '💻', description: 'Code quality & technical analysis' },
  { value: 'devops', label: 'DevOps View', icon: '⚙️', description: 'System monitoring & performance' },
  { value: 'security', label: 'Security View', icon: '🔒', description: 'Security analysis & compliance' },
  { value: 'product', label: 'Product View', icon: '📊', description: 'Requirements & progress tracking' },
  { value: 'expert', label: 'Expert Mode', icon: '🚀', description: 'Full access to all features' }
];

interface GlobalNavigationProps {
  onMobileMenuToggle?: () => void;  // For mobile sidebar toggle
}

export const GlobalNavigation: React.FC<GlobalNavigationProps> = ({
  onMobileMenuToggle
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const [searchOpen, setSearchOpen] = useState(false);
  
  // Stakeholder-driven navigation state
  const [currentMode, setCurrentMode] = useState<UserMode>(() => {
    // Load saved mode from localStorage or default to portfolio
    return (localStorage.getItem('repolens_user_mode') as UserMode) || 'portfolio';
  });
  
  // REUSE existing navigation structure
  const handleLogoClick = () => {
    navigate('/');  // Portfolio Dashboard
  };

  const handleProfileClick = () => {
    // TODO: Add profile dropdown menu
    console.log('Profile clicked');
  };

  const handleNotificationsClick = () => {
    // TODO: Add notifications dropdown
    console.log('Notifications clicked');
  };

  // Handle mode switching
  const handleModeChange = (newMode: UserMode) => {
    setCurrentMode(newMode);
    localStorage.setItem('repolens_user_mode', newMode);
    
    // Log mode change for analytics
    console.log(`🎯 User switched to ${newMode} mode`);
    
    // TODO: Trigger navigation state refresh based on new mode
    // This will eventually filter routes and tabs based on stakeholder needs
  };

  return (
    <AppBar 
      position="fixed" 
      sx={{ 
        zIndex: (theme) => theme.zIndex.drawer + 1,
        height: 56,  // Fixed height from specification
        backgroundColor: 'primary.main', // Enterprise navy from design system
        borderBottom: '1px solid',
        borderColor: 'divider',
        boxShadow: '0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24)'
      }}
    >
      <Toolbar sx={{ minHeight: 56, px: 2 }}>
        {/* Logo */}
        <Box 
          onClick={handleLogoClick}
          sx={{ 
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            mr: 4,
            transition: 'opacity 0.2s ease-in-out',
            '&:hover': {
              opacity: 0.8
            }
          }}
        >
          {/* RepoLens Logo - Using text for now, can be replaced with actual logo */}
          <Typography
            variant="h6"
            sx={{
              fontWeight: 600,
              color: 'white',
              fontFamily: 'IBM Plex Sans, sans-serif',
              letterSpacing: '-0.5px'
            }}
          >
            RepoLens
          </Typography>
        </Box>

        {/* Universal Search Bar */}
        <Box sx={{ 
          flexGrow: 1, 
          maxWidth: 480, 
          mx: 2,
          display: { xs: 'none', md: 'block' }  // Hide on mobile, show search icon instead
        }}>
          <UniversalSearchBar 
            placeholder="Search repositories, files, and contributors..."
            onSearchToggle={setSearchOpen}
          />
        </Box>

        {/* Mobile Search Button */}
        <IconButton
          size="large"
          color="inherit"
          sx={{ 
            mx: 1,
            display: { xs: 'block', md: 'none' } 
          }}
          onClick={() => {
            // TODO: Open mobile search overlay
            console.log('Mobile search clicked');
          }}
        >
          <Search />
        </IconButton>

        {/* Stakeholder Mode Selector */}
        <Box sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center', mr: 2 }}>
          <FormControl size="small">
            <Select
              value={currentMode}
              onChange={(e) => handleModeChange(e.target.value as UserMode)}
              sx={{ 
                color: 'white',
                minWidth: 160,
                '& .MuiSelect-icon': { 
                  color: 'white' 
                },
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.3)'
                },
                '&:hover .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.5)'
                },
                '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(255, 255, 255, 0.7)'
                }
              }}
              MenuProps={{
                PaperProps: {
                  sx: {
                    bgcolor: 'background.paper',
                    mt: 1
                  }
                }
              }}
            >
              {USER_MODES.map((mode) => (
                <MenuItem 
                  key={mode.value} 
                  value={mode.value}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1
                  }}
                >
                  <span style={{ fontSize: '16px' }}>{mode.icon}</span>
                  <Box>
                    <Typography variant="body2" fontWeight={600}>
                      {mode.label}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {mode.description}
                    </Typography>
                  </Box>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>

        {/* Repository Switcher */}
        <Box sx={{ display: { xs: 'none', sm: 'block' } }}>
          <RepositorySwitcher />
        </Box>

        {/* Notifications */}
        <IconButton 
          size="large"
          color="inherit"
          sx={{ mx: 1 }}
          onClick={handleNotificationsClick}
        >
          <Badge badgeContent={3} color="error">
            <Notifications />
          </Badge>
        </IconButton>

        {/* Profile */}
        <IconButton 
          size="large" 
          color="inherit"
          onClick={handleProfileClick}
          sx={{
            padding: '4px'  // Tighter padding for avatar
          }}
        >
          <Avatar 
            sx={{ 
              width: 32, 
              height: 32,
              bgcolor: 'secondary.main',
              fontSize: '14px',
              fontWeight: 600
            }}
            src={user?.avatarUrl}
          >
            {user?.name?.charAt(0).toUpperCase() || 'U'}
          </Avatar>
        </IconButton>

        {/* Mobile Menu Toggle */}
        <IconButton
          color="inherit"
          onClick={onMobileMenuToggle}
          sx={{ 
            ml: 1,
            display: { xs: 'block', md: 'none' } 
          }}
        >
          <Menu />
        </IconButton>
      </Toolbar>
    </AppBar>
  );
};

export default GlobalNavigation;
