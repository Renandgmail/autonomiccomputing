import React from 'react';
import {
  Box,
  Typography,
  Button,
  Paper,
  SxProps,
  Theme
} from '@mui/material';
import {
  Folder as FolderIcon,
  Search as SearchIcon,
  Assessment as AnalyticsIcon,
  Dashboard as DashboardIcon,
  Add as AddIcon
} from '@mui/icons-material';

export interface EmptyStateProps {
  icon?: React.ReactNode;
  title: string;
  description: string;
  actionLabel?: string;
  onAction?: () => void;
  illustration?: 'dashboard' | 'repositories' | 'analytics' | 'search' | 'settings' | 'custom';
  sx?: SxProps<Theme>;
}

const ILLUSTRATIONS = {
  dashboard: <DashboardIcon sx={{ fontSize: 64, color: 'text.secondary' }} />,
  repositories: <FolderIcon sx={{ fontSize: 64, color: 'text.secondary' }} />,
  analytics: <AnalyticsIcon sx={{ fontSize: 64, color: 'text.secondary' }} />,
  search: <SearchIcon sx={{ fontSize: 64, color: 'text.secondary' }} />,
  settings: <DashboardIcon sx={{ fontSize: 64, color: 'text.secondary' }} />
};

const EmptyState: React.FC<EmptyStateProps> = ({
  icon,
  title,
  description,
  actionLabel,
  onAction,
  illustration = 'repositories',
  sx = {}
}) => {
  const displayIcon = icon || ILLUSTRATIONS[illustration as keyof typeof ILLUSTRATIONS];

  return (
    <Paper 
      sx={{ 
        textAlign: 'center', 
        py: 8, 
        px: 4,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: 300,
        ...sx 
      }}
    >
      <Box mb={3}>
        {displayIcon}
      </Box>
      
      <Typography variant="h5" component="h2" gutterBottom color="text.primary">
        {title}
      </Typography>
      
      <Typography 
        variant="body1" 
        color="text.secondary" 
        sx={{ maxWidth: 500, mb: 4 }}
      >
        {description}
      </Typography>
      
      {actionLabel && onAction && (
        <Button
          variant="contained"
          size="large"
          startIcon={<AddIcon />}
          onClick={onAction}
          sx={{ mt: 1 }}
        >
          {actionLabel}
        </Button>
      )}
    </Paper>
  );
};

export default EmptyState;
