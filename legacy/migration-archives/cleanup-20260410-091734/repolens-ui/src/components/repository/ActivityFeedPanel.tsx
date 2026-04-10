/**
 * Zone 3: Activity Feed Panel Component (SECONDARY PANEL)
 * Shows quality events only (NOT commit messages) for Engineering Managers
 * Quality gate failures, new issues, security flags, etc.
 */

import React from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Skeleton,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Chip,
  Stack,
  Avatar
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { 
  CheckCircle, 
  Error as ErrorIcon, 
  Warning as WarningIcon, 
  Info as InfoIcon,
  Security as SecurityIcon,
  TrendingUp as TrendingUpIcon,
  Build as BuildIcon,
  Sync as SyncIcon,
  Search as SearchIcon
} from '@mui/icons-material';
import { 
  RepositoryActivityResponse, 
  RepositoryActivity, 
  RepositoryActivityType,
  ActivitySeverity,
  formatRelativeTime,
  getActivityIcon,
  getActivitySeverityColor
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

const ActivityList = styled(List)(({ theme }) => ({
  padding: 0,
  '& .MuiListItem-root': {
    paddingLeft: 0,
    paddingRight: 0,
    paddingTop: theme.spacing(1),
    paddingBottom: theme.spacing(1),
  },
}));

const EmptyState = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  padding: theme.spacing(4),
  textAlign: 'center',
  color: theme.palette.text.secondary,
}));

const LoadingContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing(1),
}));

const ActivityIcon = styled(Avatar)<{ severity: ActivitySeverity }>(({ theme, severity }) => ({
  width: 32,
  height: 32,
  backgroundColor: getActivitySeverityColor(severity),
  fontSize: '14px',
}));

const TimeStamp = styled(Typography)(({ theme }) => ({
  fontSize: '12px',
  color: theme.palette.text.secondary,
  fontWeight: 400,
}));

interface ActivityFeedPanelProps {
  repositoryId: number;
  activity: RepositoryActivityResponse | null;
  loading?: boolean;
}

/**
 * Get the appropriate Material UI icon for activity type
 */
const getActivityMuiIcon = (type: RepositoryActivityType): React.ElementType => {
  switch (type) {
    case RepositoryActivityType.QualityGatePass:
    case RepositoryActivityType.BuildSuccess:
      return CheckCircle;
    case RepositoryActivityType.QualityGateFail:
    case RepositoryActivityType.BuildFailure:
      return ErrorIcon;
    case RepositoryActivityType.NewCriticalIssue:
    case RepositoryActivityType.ComplexitySpike:
      return WarningIcon;
    case RepositoryActivityType.SecurityVulnerabilityDetected:
      return SecurityIcon;
    case RepositoryActivityType.SyncComplete:
      return SyncIcon;
    case RepositoryActivityType.AnalysisComplete:
      return SearchIcon;
    default:
      return InfoIcon;
  }
};

/**
 * Single activity item component
 */
const ActivityItem: React.FC<{ activity: RepositoryActivity }> = ({ activity }) => {
  const IconComponent = getActivityMuiIcon(activity.type);

  return (
    <ListItem divider>
      <ListItemIcon>
        <ActivityIcon severity={activity.severity}>
          <IconComponent fontSize="small" />
        </ActivityIcon>
      </ListItemIcon>
      <ListItemText
        primary={
          <Typography
            variant="body2"
            sx={{
              fontWeight: 500,
              lineHeight: 1.4,
              color: 'text.primary',
            }}
          >
            {activity.description}
          </Typography>
        }
        secondary={
          <Box sx={{ mt: 0.5, display: 'flex', alignItems: 'center', gap: 1 }}>
            <TimeStamp>
              {formatRelativeTime(activity.timestamp)}
            </TimeStamp>
            {activity.severity !== ActivitySeverity.Info && (
              <Chip
                size="small"
                label={ActivitySeverity[activity.severity]}
                sx={{
                  backgroundColor: getActivitySeverityColor(activity.severity),
                  color: 'white',
                  fontSize: '10px',
                  height: 18,
                  fontWeight: 600,
                }}
              />
            )}
          </Box>
        }
      />
    </ListItem>
  );
};

/**
 * Loading skeleton for activity feed
 */
const ActivityLoadingSkeleton: React.FC = () => (
  <LoadingContainer>
    {Array.from({ length: 6 }).map((_, index) => (
      <Box key={index} sx={{ display: 'flex', alignItems: 'center', gap: 2, p: 1 }}>
        <Skeleton variant="circular" width={32} height={32} />
        <Box sx={{ flexGrow: 1 }}>
          <Skeleton variant="text" width="80%" />
          <Skeleton variant="text" width="40%" />
        </Box>
      </Box>
    ))}
  </LoadingContainer>
);

/**
 * Zone 3: Activity Feed Panel - SECONDARY PANEL
 * Shows quality events (NOT commits) for context
 */
const ActivityFeedPanel: React.FC<ActivityFeedPanelProps> = ({
  repositoryId,
  activity,
  loading = false,
}) => {
  // Loading state
  if (loading || !activity) {
    return (
      <PanelCard>
        <CardContent>
          <PanelHeader>
            <PanelTitle>
              📈 Recent Activity
              <Chip size="small" label="Secondary" color="info" variant="outlined" />
            </PanelTitle>
          </PanelHeader>
          <ActivityLoadingSkeleton />
        </CardContent>
      </PanelCard>
    );
  }

  // Empty state
  if (activity.activities.length === 0) {
    return (
      <PanelCard>
        <CardContent>
          <PanelHeader>
            <PanelTitle>
              📈 Recent Activity
              <Chip size="small" label="Secondary" color="default" variant="outlined" />
            </PanelTitle>
          </PanelHeader>
          <EmptyState>
            <InfoIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
            <Typography variant="body2" color="textSecondary">
              No recent quality events detected.
            </Typography>
            <Typography variant="caption" color="textSecondary">
              Quality events will appear here as they occur.
            </Typography>
          </EmptyState>
        </CardContent>
      </PanelCard>
    );
  }

  return (
    <PanelCard>
      <CardContent sx={{ pb: 2 }}>
        <PanelHeader>
          <PanelTitle>
            📈 Recent Activity
            <Chip size="small" label="Secondary" color="info" variant="outlined" />
          </PanelTitle>
          <Typography variant="body2" color="textSecondary">
            {activity.totalCount} events
          </Typography>
        </PanelHeader>

        {/* Activity List */}
        <ActivityList>
          {activity.activities.map((activityItem) => (
            <ActivityItem
              key={activityItem.id}
              activity={activityItem}
            />
          ))}
        </ActivityList>

        {/* Summary Footer */}
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
          <Stack direction="row" spacing={2} justifyContent="center">
            <Box textAlign="center">
              <Typography variant="body2" color="success.main" fontWeight={600}>
                {activity.activities.filter(a => a.severity === ActivitySeverity.Success).length}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Success
              </Typography>
            </Box>
            <Box textAlign="center">
              <Typography variant="body2" color="warning.main" fontWeight={600}>
                {activity.activities.filter(a => a.severity === ActivitySeverity.Warning).length}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Warning
              </Typography>
            </Box>
            <Box textAlign="center">
              <Typography variant="body2" color="error.main" fontWeight={600}>
                {activity.activities.filter(a => 
                  a.severity === ActivitySeverity.Error || a.severity === ActivitySeverity.Critical
                ).length}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Issues
              </Typography>
            </Box>
          </Stack>
        </Box>

        {/* Last Updated */}
        <Box sx={{ mt: 1, textAlign: 'center' }}>
          <Typography variant="caption" color="textSecondary">
            Updated {formatRelativeTime(activity.lastUpdated)}
          </Typography>
        </Box>
      </CardContent>
    </PanelCard>
  );
};

export default ActivityFeedPanel;
