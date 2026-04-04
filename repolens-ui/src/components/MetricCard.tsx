/**
 * MetricCard Component
 * Enterprise-grade metric display card following RepoLens specification
 * Reuses existing Material-UI Card structure with enhanced functionality
 */

import React from 'react';
import { Card, CardContent, Typography, Box, Skeleton, useTheme } from '@mui/material';
import { TrendingUp, TrendingDown, TrendingFlat } from '@mui/icons-material';
import { getHealthColor, getHealthBand } from '../theme/design-system';

// Trend Arrow Component
interface TrendArrowProps {
  trend: {
    direction: 'up' | 'down' | 'flat';
    positive: 'up' | 'down'; // Is "up" good or bad for this metric?
  };
}

const TrendArrow: React.FC<TrendArrowProps> = ({ trend }) => {
  const theme = useTheme();
  
  // Determine if this trend is positive (good) or negative (bad)
  const isPositiveTrend = 
    (trend.direction === 'up' && trend.positive === 'up') ||
    (trend.direction === 'down' && trend.positive === 'down');
  
  // Color based on whether trend is positive or negative
  const trendColor = trend.direction === 'flat' 
    ? theme.palette.text.secondary
    : isPositiveTrend 
      ? theme.palette.success.main 
      : theme.palette.error.main;

  const TrendIcon = trend.direction === 'up' ? TrendingUp : 
                   trend.direction === 'down' ? TrendingDown : 
                   TrendingFlat;

  return (
    <TrendIcon 
      sx={{ 
        fontSize: 16, 
        color: trendColor, 
        mr: 0.5 
      }} 
    />
  );
};

// Loading Skeleton Component
const MetricCardSkeleton: React.FC = () => (
  <Card sx={{ width: 220, height: 'auto' }}>
    <CardContent>
      <Skeleton variant="text" width={80} height={16} sx={{ mb: 1 }} />
      <Skeleton variant="text" width={60} height={40} sx={{ mb: 1 }} />
      <Skeleton variant="text" width={100} height={16} />
    </CardContent>
  </Card>
);

// Main MetricCard Interface
export interface MetricCardProps {
  label: string;
  value: string | number;
  unit?: string;           // e.g. "%" or "h"
  
  // Trend support for contextual information
  trend?: {
    direction: 'up' | 'down' | 'flat';
    delta: string;          // e.g. "+5%", "-2h", "+3"
    context: string;        // e.g. "vs last week", "this month"
    positive: 'up' | 'down'; // Semantic meaning of direction
  };
  
  // Interactive navigation
  onClick?: () => void;
  
  // Loading states
  loading?: boolean;
  
  // Health status coloring (0-100, auto-colors based on bands)
  healthScore?: number;
  
  // Accessibility
  'aria-label'?: string;
  'aria-describedby'?: string;
}

export const MetricCard: React.FC<MetricCardProps> = ({
  label,
  value,
  unit = '',
  trend,
  onClick,
  loading = false,
  healthScore,
  'aria-label': ariaLabel,
  'aria-describedby': ariaDescribedBy,
}) => {
  const theme = useTheme();

  // Show skeleton during loading
  if (loading) {
    return <MetricCardSkeleton />;
  }

  // Determine value color based on health score
  const valueColor = healthScore !== undefined 
    ? getHealthColor(healthScore)
    : theme.palette.primary.main;

  // Format value with unit
  const displayValue = `${value}${unit}`;

  // Determine if card should be interactive
  const isInteractive = !!onClick;

  return (
    <Card
      sx={{
        width: 220, // Fixed width from specification
        cursor: isInteractive ? 'pointer' : 'default',
        transition: 'box-shadow 0.2s ease-in-out, transform 0.1s ease-in-out',
        '&:hover': isInteractive ? {
          boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
          transform: 'translateY(-1px)',
        } : {},
        '&:focus': isInteractive ? {
          outline: `2px solid ${theme.palette.secondary.main}`,
          outlineOffset: '2px',
        } : {},
      }}
      onClick={onClick}
      role={isInteractive ? 'button' : undefined}
      tabIndex={isInteractive ? 0 : undefined}
      aria-label={ariaLabel || `${label}: ${displayValue}${trend ? `, trend ${trend.direction} ${trend.delta}` : ''}`}
      aria-describedby={ariaDescribedBy}
      onKeyDown={isInteractive ? (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick?.();
        }
      } : undefined}
    >
      <CardContent sx={{ pb: 2 }}>
        {/* Label */}
        <Typography 
          variant="caption" 
          color="text.secondary"
          sx={{ 
            display: 'block',
            fontFamily: theme.typography.fontFamily,
            fontSize: '12px',
            lineHeight: 1.5,
            mb: 1,
          }}
        >
          {label}
        </Typography>

        {/* Value */}
        <Typography
          sx={{
            fontFamily: theme.repoLens?.typography.fontFamilyMono || '"IBM Plex Mono", monospace',
            fontSize: '32px',
            fontWeight: 500,
            lineHeight: 1.2,
            color: valueColor,
            mb: trend ? 1 : 0,
          }}
          component="div"
        >
          {displayValue}
        </Typography>

        {/* Trend Information */}
        {trend && (
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <TrendArrow trend={trend} />
            <Typography 
              variant="body2" 
              color="text.secondary"
              sx={{ 
                fontSize: '13px',
                lineHeight: 1.5,
              }}
            >
              {trend.delta} {trend.context}
            </Typography>
          </Box>
        )}

        {/* Health Score Indicator (if provided) */}
        {healthScore !== undefined && (
          <Box sx={{ mt: 1 }}>
            <Typography 
              variant="caption" 
              sx={{ 
                color: valueColor,
                fontWeight: 500,
                fontSize: '11px',
              }}
            >
              {getHealthBand(healthScore).label}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

// Default export for easier importing
export default MetricCard;

// Type exports for use in other components
export type { TrendArrowProps };
