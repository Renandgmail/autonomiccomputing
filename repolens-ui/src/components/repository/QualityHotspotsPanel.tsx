/**
 * Zone 2: Quality Hotspots Panel Component (PRIMARY PANEL)
 * Most important panel - shows worst files first for Engineering Managers
 * Ranked by composite score: complexity × churn × (1 - quality)
 */

import React from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  Typography, 
  Button, 
  Skeleton,
  Alert,
  Chip,
  Stack
} from '@mui/material';
import { styled } from '@mui/material/styles';
import { ArrowForward as ArrowForwardIcon } from '@mui/icons-material';
import QualityHotspotRow from '../QualityHotspotRow';
import { 
  QualityHotspotsResponse, 
  QualityHotspot, 
  HotspotSeverity,
  HOTSPOT_SEVERITY_LABELS,
  HOTSPOT_ISSUE_TYPE_LABELS,
  formatEstimatedTime,
  getHotspotSeverityColor
} from '../../services/repositoryApiService';

const PanelCard = styled(Card)(({ theme }) => ({
  height: 'fit-content',
  minHeight: '400px',
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

const HotspotsList = styled(Box)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing(1),
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

const SeeAllButton = styled(Button)(({ theme }) => ({
  marginTop: theme.spacing(2),
  justifyContent: 'space-between',
  textTransform: 'none',
  fontWeight: 500,
}));

const SuccessEmoji = styled(Typography)(({ theme }) => ({
  fontSize: '48px',
  marginBottom: theme.spacing(1),
}));

interface QualityHotspotsPanelProps {
  repositoryId: number;
  hotspots: QualityHotspotsResponse | null;
  loading?: boolean;
  onHotspotClick?: (hotspot: QualityHotspot) => void;
  onSeeAll?: () => void;
}

/**
 * Simple hotspot row component for this panel
 */
const HotspotRowWrapper: React.FC<{ 
  hotspot: QualityHotspot; 
  onClick?: (hotspot: QualityHotspot) => void; 
}> = ({ hotspot, onClick }) => {
  const handleClick = () => {
    if (onClick) {
      onClick(hotspot);
    }
  };

  return (
    <Box
      onClick={handleClick}
      sx={{
        display: 'flex',
        alignItems: 'center',
        padding: 2,
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 1,
        cursor: onClick ? 'pointer' : 'default',
        transition: 'all 0.2s ease-in-out',
        '&:hover': onClick ? {
          backgroundColor: 'action.hover',
          borderColor: 'primary.main',
          transform: 'translateY(-1px)',
          boxShadow: 1,
        } : {},
      }}
    >
      {/* Severity Badge */}
      <Chip
        label={HOTSPOT_SEVERITY_LABELS[hotspot.severity]}
        size="small"
        sx={{
          backgroundColor: getHotspotSeverityColor(hotspot.severity),
          color: 'white',
          fontWeight: 600,
          minWidth: 70,
          mr: 2,
        }}
      />
      
      {/* File Path */}
      <Box sx={{ flexGrow: 1, minWidth: 0, mr: 2 }}>
        <Typography
          variant="body2"
          sx={{
            fontFamily: 'monospace',
            fontWeight: 500,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
          title={hotspot.filePath}
        >
          {hotspot.filePath}
        </Typography>
        <Typography
          variant="caption"
          color="textSecondary"
          sx={{ display: 'block' }}
        >
          {HOTSPOT_ISSUE_TYPE_LABELS[hotspot.issueType]} • {formatEstimatedTime(hotspot.estimatedFixHours)}
        </Typography>
      </Box>
      
      {/* Navigation Arrow */}
      {onClick && (
        <ArrowForwardIcon 
          color="action" 
          fontSize="small" 
        />
      )}
    </Box>
  );
};

/**
 * Loading skeleton for hotspots
 */
const HotspotsLoadingSkeleton: React.FC = () => (
  <LoadingContainer>
    {Array.from({ length: 5 }).map((_, index) => (
      <Skeleton key={index} variant="rectangular" height={60} />
    ))}
  </LoadingContainer>
);

/**
 * Zone 2: Quality Hotspots Panel - PRIMARY PANEL
 * Shows top 5 worst files ranked by composite score
 * Essential for Engineering Manager decisions
 */
const QualityHotspotsPanel: React.FC<QualityHotspotsPanelProps> = ({
  repositoryId,
  hotspots,
  loading = false,
  onHotspotClick,
  onSeeAll,
}) => {
  // Loading state
  if (loading || !hotspots) {
    return (
      <PanelCard>
        <CardContent>
          <PanelHeader>
            <PanelTitle>
              🔥 Quality Hotspots
              <Chip size="small" label="Primary" color="error" variant="outlined" />
            </PanelTitle>
          </PanelHeader>
          <HotspotsLoadingSkeleton />
        </CardContent>
      </PanelCard>
    );
  }

  // Empty state - No hotspots (healthy repository)
  if (hotspots.totalCount === 0) {
    return (
      <PanelCard>
        <CardContent>
          <PanelHeader>
            <PanelTitle>
              🔥 Quality Hotspots
              <Chip size="small" label="Primary" color="success" variant="outlined" />
            </PanelTitle>
          </PanelHeader>
          <EmptyState>
            <SuccessEmoji>✅</SuccessEmoji>
            <Typography variant="h6" color="success.main" sx={{ mb: 1 }}>
              No quality hotspots detected
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Your codebase is healthy! All files are below quality issue thresholds.
            </Typography>
            {onSeeAll && (
              <Button 
                variant="outlined" 
                onClick={onSeeAll}
                sx={{ mt: 2 }}
              >
                View Full Analytics
              </Button>
            )}
          </EmptyState>
        </CardContent>
      </PanelCard>
    );
  }

  return (
    <PanelCard>
      <CardContent>
        <PanelHeader>
          <PanelTitle>
            🔥 Quality Hotspots
            <Chip 
              size="small" 
              label="Primary" 
              color="error" 
              variant="outlined" 
            />
          </PanelTitle>
          <Typography variant="body2" color="textSecondary">
            {hotspots.shownCount} of {hotspots.totalCount} files
          </Typography>
        </PanelHeader>

        {/* Hotspots explanation */}
        <Alert severity="info" sx={{ mb: 2, fontSize: '0.875rem' }}>
          <strong>Worst files first:</strong> Ranked by complexity × churn × quality deficit. 
          These files need immediate attention.
        </Alert>

        {/* Hotspots list */}
        <HotspotsList>
          {hotspots.hotspots.map((hotspot) => (
            <HotspotRowWrapper
              key={`${hotspot.repositoryId}-${hotspot.fileId}`}
              hotspot={hotspot}
              onClick={onHotspotClick}
            />
          ))}
        </HotspotsList>

        {/* See all button */}
        {hotspots.hasMore && onSeeAll && (
          <SeeAllButton
            variant="text"
            fullWidth
            onClick={onSeeAll}
            endIcon={<ArrowForwardIcon />}
          >
            <span>See all {hotspots.totalCount} hotspots</span>
          </SeeAllButton>
        )}

        {/* Summary stats */}
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
          <Stack direction="row" spacing={2} justifyContent="center">
            <Box textAlign="center">
              <Typography variant="h6" color="error.main">
                {hotspots.hotspots.filter(h => h.severity === HotspotSeverity.Critical).length}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Critical
              </Typography>
            </Box>
            <Box textAlign="center">
              <Typography variant="h6" color="warning.main">
                {hotspots.hotspots.filter(h => h.severity === HotspotSeverity.High).length}
              </Typography>
              <Typography variant="caption" color="textSecondary">
                High
              </Typography>
            </Box>
            <Box textAlign="center">
              <Typography variant="h6" color="info.main">
                {hotspots.hotspots.reduce((total, h) => total + h.estimatedFixHours, 0).toFixed(1)}h
              </Typography>
              <Typography variant="caption" color="textSecondary">
                Est. Fix Time
              </Typography>
            </Box>
          </Stack>
        </Box>
      </CardContent>
    </PanelCard>
  );
};

export default QualityHotspotsPanel;
