/**
 * Zone 4: Quick Actions Panel Component (TERTIARY PANEL)
 * Navigation shortcuts for repository analysis and management
 * Engineering Manager shortcuts to L3 screens
 */

import React from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Button, 
  Skeleton,
  Grid,
  Tooltip,
  Stack
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { 
  Search as SearchIcon,
  Analytics as AnalyticsIcon,
  AccountTree as AccountTreeIcon,
  Group as GroupIcon,
  Security as SecurityIcon,
  Download as DownloadIcon,
  ArrowForward as ArrowForwardIcon
} from '@mui/icons-material';
import { 
  RepositoryQuickActions, 
  QuickActionRoute 
} from '../../services/repositoryApiService';

const PanelCard = styled(Card)(({ theme }) => ({
  height: 'fit-content',
  minHeight: '300px',
}));

const PanelHeader = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  marginBottom: theme.spacing(2),
}));

const PanelTitle = styled(Typography)(({ theme }) => ({
  fontSize: '1.125rem',
  fontWeight: 600,
  color: theme.palette.text.primary,
  display: 'flex',
  alignItems: 'center',
  gap: theme.spacing(1),
}));

const ActionsGrid = styled(Box)(({ theme }) => ({
  display: 'grid',
  gridTemplateColumns: 'repeat(2, 1fr)',
  gap: theme.spacing(2),
  
  // Mobile: single column
  [theme.breakpoints.down('sm')]: {
    gridTemplateColumns: '1fr',
  },
}));

const ActionButton = styled(Button)(({ theme }) => ({
  height: 'auto',
  padding: theme.spacing(2),
  textAlign: 'left',
  justifyContent: 'flex-start',
  textTransform: 'none',
  borderRadius: theme.spacing(1),
  border: `1px solid ${theme.palette.divider}`,
  backgroundColor: theme.palette.background.paper,
  color: theme.palette.text.primary,
  transition: 'all 0.2s ease-in-out',
  
  '&:hover': {
    borderColor: theme.palette.primary.main,
    backgroundColor: theme.palette.action.hover,
    transform: 'translateY(-2px)',
    boxShadow: theme.shadows[2],
  },
  
  '&:disabled': {
    opacity: 0.5,
    transform: 'none',
  },
}));

const ActionIcon = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: 40,
  height: 40,
  borderRadius: theme.spacing(1),
  backgroundColor: theme.palette.primary.main,
  color: 'white',
  marginRight: theme.spacing(1.5),
  flexShrink: 0,
}));

const ActionContent = styled(Box)({
  flexGrow: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'flex-start',
  gap: 4,
});

const LoadingContainer = styled(Box)(({ theme }) => ({
  display: 'grid',
  gridTemplateColumns: 'repeat(2, 1fr)',
  gap: theme.spacing(2),
}));

interface QuickActionsPanelProps {
  repositoryId: number;
  quickActions: RepositoryQuickActions | null;
  loading?: boolean;
  onNavigate?: (route: string) => void;
}

/**
 * Get the appropriate Material UI icon for action name
 */
const getActionIcon = (iconName: string): React.ElementType => {
  switch (iconName) {
    case 'Search':
      return SearchIcon;
    case 'Analytics':
      return AnalyticsIcon;
    case 'AccountTree':
      return AccountTreeIcon;
    case 'Group':
      return GroupIcon;
    case 'Security':
      return SecurityIcon;
    case 'Download':
      return DownloadIcon;
    default:
      return ArrowForwardIcon;
  }
};

/**
 * Single quick action button component
 */
const QuickActionButton: React.FC<{
  action: QuickActionRoute;
  onNavigate?: (route: string) => void;
}> = ({ action, onNavigate }) => {
  const IconComponent = getActionIcon(action.icon);
  
  const handleClick = () => {
    if (onNavigate && action.isEnabled) {
      onNavigate(action.route);
    }
  };

  return (
    <Tooltip title={action.description} placement="top">
      <ActionButton
        onClick={handleClick}
        disabled={!action.isEnabled}
        fullWidth
      >
        <ActionIcon>
          <IconComponent fontSize="medium" />
        </ActionIcon>
        <ActionContent>
          <Typography
            variant="subtitle2"
            sx={{
              fontWeight: 600,
              fontSize: '14px',
              lineHeight: 1.2,
            }}
          >
            {action.name}
          </Typography>
          <Typography
            variant="caption"
            color="textSecondary"
            sx={{
              fontSize: '12px',
              lineHeight: 1.2,
              textAlign: 'left',
            }}
          >
            {action.description}
          </Typography>
        </ActionContent>
      </ActionButton>
    </Tooltip>
  );
};

/**
 * Loading skeleton for quick actions
 */
const QuickActionsLoadingSkeleton: React.FC = () => (
  <LoadingContainer>
    {Array.from({ length: 6 }).map((_, index) => (
      <Skeleton 
        key={index} 
        variant="rectangular" 
        height={80} 
        sx={{ borderRadius: 1 }} 
      />
    ))}
  </LoadingContainer>
);

/**
 * Zone 4: Quick Actions Panel - TERTIARY PANEL
 * Navigation shortcuts to L3 analytics screens
 */
const QuickActionsPanel: React.FC<QuickActionsPanelProps> = ({
  repositoryId,
  quickActions,
  loading = false,
  onNavigate,
}) => {
  // Loading state
  if (loading || !quickActions) {
    return (
      <PanelCard>
        <CardContent>
          <PanelHeader>
            <PanelTitle>
              🚀 Quick Actions
            </PanelTitle>
          </PanelHeader>
          <QuickActionsLoadingSkeleton />
        </CardContent>
      </PanelCard>
    );
  }

  return (
    <PanelCard>
      <CardContent>
        <PanelHeader>
          <PanelTitle>
            🚀 Quick Actions
          </PanelTitle>
          <Typography variant="body2" color="textSecondary">
            {quickActions.actions.length} actions
          </Typography>
        </PanelHeader>

        {/* Actions Grid */}
        <ActionsGrid>
          {quickActions.actions.map((action) => (
            <QuickActionButton
              key={action.name}
              action={action}
              onNavigate={onNavigate}
            />
          ))}
        </ActionsGrid>

        {/* Navigation Help */}
        <Box sx={{ mt: 3, p: 2, backgroundColor: 'action.hover', borderRadius: 1 }}>
          <Typography variant="caption" color="textSecondary" sx={{ display: 'block', mb: 1 }}>
            <strong>Navigation Shortcuts:</strong>
          </Typography>
          <Stack direction="column" spacing={0.5}>
            <Typography variant="caption" color="textSecondary">
              • Search: Find specific code patterns
            </Typography>
            <Typography variant="caption" color="textSecondary">
              • Analytics: View trends and metrics
            </Typography>
            <Typography variant="caption" color="textSecondary">
              • Code Graph: Visualize architecture
            </Typography>
            <Typography variant="caption" color="textSecondary">
              • Team: Contributor insights
            </Typography>
          </Stack>
        </Box>

        {/* Repository Context */}
        <Box sx={{ mt: 2, textAlign: 'center' }}>
          <Typography variant="caption" color="textSecondary">
            Repository ID: {repositoryId}
          </Typography>
        </Box>
      </CardContent>
    </PanelCard>
  );
};

export default QuickActionsPanel;
