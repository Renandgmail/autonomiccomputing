/**
 * Repository Health Chip Component
 * Provides instant visual health status with percentage and optional trend indicator
 * Used in Portfolio Dashboard, Repository lists, and Context bars
 */

import React from 'react';
import { Box, Typography, useTheme } from '@mui/material';
import { TrendingUp, TrendingDown, TrendingFlat } from '@mui/icons-material';
import { getHealthColor, getHealthBand } from '../theme/design-system';

// Component interfaces
interface HealthTrend {
  direction: 'up' | 'down' | 'flat';
  delta: number;            // +/- percentage points
}

export interface RepositoryHealthChipProps {
  healthScore: number;        // 0-100, maps to health band colors
  trend?: HealthTrend;       // Optional trend indicator
  size?: 'small' | 'medium' | 'large';    // Size variants
  showLabel?: boolean;        // Display "Excellent", "Good", etc.
  onClick?: () => void;       // Optional click handler
  'aria-label'?: string;      // Accessibility override
}

// Trend Arrow Sub-component
interface TrendArrowProps {
  trend: HealthTrend;
  size: 'small' | 'medium' | 'large';
}

const TrendArrow: React.FC<TrendArrowProps> = ({ trend, size }) => {
  const theme = useTheme();
  
  // Determine if trend is positive (health improving)
  const isPositiveTrend = trend.direction === 'up';
  const isNegativeTrend = trend.direction === 'down';
  
  // Color based on trend direction (up=good, down=bad for health)
  const trendColor = trend.direction === 'flat' 
    ? theme.palette.text.secondary
    : isPositiveTrend 
      ? theme.palette.success.main 
      : theme.palette.error.main;

  // Icon size based on chip size
  const iconSize = {
    small: 12,
    medium: 14,
    large: 16
  };

  const TrendIcon = trend.direction === 'up' ? TrendingUp : 
                   trend.direction === 'down' ? TrendingDown : 
                   TrendingFlat;

  return (
    <TrendIcon 
      sx={{ 
        fontSize: iconSize[size], 
        color: trendColor,
        ml: 0.5
      }} 
    />
  );
};

export const RepositoryHealthChip: React.FC<RepositoryHealthChipProps> = ({
  healthScore,
  trend,
  size = 'medium',
  showLabel = false,
  onClick,
  'aria-label': ariaLabel,
}) => {
  const theme = useTheme();
  
  // Get health information from design system
  const healthBand = getHealthBand(healthScore);
  const healthColor = getHealthColor(healthScore);
  
  // Size configurations for responsive design
  const sizeConfig = {
    small: { 
      dotSize: 8, 
      fontSize: '12px', 
      gap: '4px',
      padding: '2px 6px'
    },
    medium: { 
      dotSize: 10, 
      fontSize: '14px', 
      gap: '6px',
      padding: '4px 8px'
    },
    large: { 
      dotSize: 12, 
      fontSize: '16px', 
      gap: '8px',
      padding: '6px 12px'
    }
  };
  
  const config = sizeConfig[size];
  
  // Determine if component should be interactive
  const isInteractive = !!onClick;
  
  // Generate accessible label
  const defaultAriaLabel = `Repository health: ${Math.round(healthScore)}% - ${healthBand.label}${
    trend ? `, trend ${trend.direction} ${Math.abs(trend.delta)} points` : ''
  }`;

  return (
    <Box
      onClick={onClick}
      role={isInteractive ? 'button' : undefined}
      tabIndex={isInteractive ? 0 : undefined}
      aria-label={ariaLabel || defaultAriaLabel}
      onKeyDown={isInteractive ? (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault();
          onClick?.();
        }
      } : undefined}
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: config.gap,
        padding: config.padding,
        borderRadius: '16px',
        backgroundColor: isInteractive ? 'rgba(0,0,0,0.04)' : 'transparent',
        cursor: isInteractive ? 'pointer' : 'default',
        transition: 'background-color 0.2s ease-in-out',
        '&:hover': isInteractive ? {
          backgroundColor: 'rgba(0,0,0,0.08)',
        } : {},
        '&:focus': isInteractive ? {
          outline: `2px solid ${theme.palette.secondary.main}`,
          outlineOffset: '2px',
        } : {},
      }}
    >
      {/* Health Status Dot */}
      <Box
        sx={{
          width: config.dotSize,
          height: config.dotSize,
          borderRadius: '50%',
          backgroundColor: healthColor,
          flexShrink: 0,
          // Add subtle shadow for better visibility
          boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
        }}
      />
      
      {/* Health Percentage */}
      <Typography
        sx={{
          fontSize: config.fontSize,
          fontWeight: 500,
          color: healthColor,
          fontFamily: theme.repoLens?.typography.fontFamilyMono || '"IBM Plex Mono", monospace',
          lineHeight: 1,
        }}
      >
        {Math.round(healthScore)}%
      </Typography>
      
      {/* Health Label (optional) */}
      {showLabel && (
        <Typography
          sx={{
            fontSize: config.fontSize,
            fontWeight: 400,
            color: theme.palette.text.secondary,
            lineHeight: 1,
          }}
        >
          {healthBand.label}
        </Typography>
      )}
      
      {/* Trend Arrow (optional) */}
      {trend && (
        <TrendArrow trend={trend} size={size} />
      )}
    </Box>
  );
};

// Default export for easier importing
export default RepositoryHealthChip;

// Type exports for use in other components
export type { HealthTrend };
