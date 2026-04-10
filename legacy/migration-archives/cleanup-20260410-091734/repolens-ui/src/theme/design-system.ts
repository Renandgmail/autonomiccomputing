/**
 * RepoLens Design System
 * Based on specification from 04-design-system/DESIGN_SYSTEM.md
 * Reuses existing Material-UI theme structure from App.tsx
 */

import { createTheme, Theme } from '@mui/material/styles';

// Design Tokens - Core color system
export const designTokens = {
  // Brand Colors
  brandPrimary: '#1A2B4A',      // Headers, primary CTAs, selected states
  interactive: '#0F7BFF',       // Links, active states, focus rings, progress bars
  
  // Status Colors - Semantic only
  statusSuccess: '#16A34A',     // Excellent health, passing gates, resolved issues
  statusWarning: '#D97706',     // Fair health, approaching thresholds, medium severity
  statusDanger: '#DC2626',      // Critical health, security vulns, failed gates
  statusHigh: '#EA580C',        // High severity (between warning and danger)
  feature: '#0D9488',           // AI assistant accent, selected tabs, Good health band
  
  // Text Colors
  textPrimary: '#111827',       // Headings, primary body text, table data
  textSecondary: '#6B7280',     // Captions, metadata, placeholder text
  
  // Surface Colors
  surface: '#F9FAFB',           // Page background, table alternating rows
  card: '#FFFFFF',              // Card surfaces, modal backgrounds, dropdowns
  border: '#E5E7EB',            // Card borders, table dividers, input borders
} as const;

// Health Band Color Mapping
export const healthBands = {
  excellent: { color: designTokens.statusSuccess, min: 90, max: 100, label: 'Excellent' },
  good: { color: designTokens.feature, min: 70, max: 89, label: 'Good' },
  fair: { color: designTokens.statusWarning, min: 50, max: 69, label: 'Fair' },
  poor: { color: designTokens.statusHigh, min: 30, max: 49, label: 'Poor' },
  critical: { color: designTokens.statusDanger, min: 0, max: 29, label: 'Critical' },
} as const;

// Spacing System - 8px base grid
export const spacing = {
  space1: '4px',    // Icon-to-label gaps, badge padding
  space2: '8px',    // List item spacing, chip padding, inline element gaps
  space3: '16px',   // Card internal padding, form field spacing
  space4: '24px',   // Between cards, section dividers, panel padding
  space5: '32px',   // Between major sections, page-level padding
  space6: '48px',   // Page margins, modal padding
} as const;

// Typography Scale
export const typography = {
  // Font families
  fontFamily: '"IBM Plex Sans", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  fontFamilyMono: '"IBM Plex Mono", "Fira Code", "Source Code Pro", monospace',
  
  // Type scale
  h1: {
    fontSize: '28px',
    fontWeight: 600,
    lineHeight: 1.3,
    fontFamily: '"IBM Plex Sans", sans-serif',
  },
  h2: {
    fontSize: '20px',
    fontWeight: 600,
    lineHeight: 1.4,
    fontFamily: '"IBM Plex Sans", sans-serif',
  },
  h3: {
    fontSize: '16px',
    fontWeight: 500,
    lineHeight: 1.4,
    fontFamily: '"IBM Plex Sans", sans-serif',
  },
  body: {
    fontSize: '14px',
    fontWeight: 400,
    lineHeight: 1.6,
    fontFamily: '"IBM Plex Sans", sans-serif',
  },
  metricValue: {
    fontSize: '32px',
    fontWeight: 500,
    lineHeight: 1.2,
    fontFamily: '"IBM Plex Mono", monospace',
  },
  code: {
    fontSize: '13px',
    fontWeight: 400,
    lineHeight: 1.5,
    fontFamily: '"IBM Plex Mono", monospace',
  },
  caption: {
    fontSize: '12px',
    fontWeight: 400,
    lineHeight: 1.5,
    fontFamily: '"IBM Plex Sans", sans-serif',
  },
} as const;

// Responsive Breakpoints
export const breakpoints = {
  mobile: '768px',
  tablet: '1024px',
  desktop: '1440px',
} as const;

// Utility function to get health band color based on score
export const getHealthColor = (score: number): string => {
  if (score >= healthBands.excellent.min) return healthBands.excellent.color;
  if (score >= healthBands.good.min) return healthBands.good.color;
  if (score >= healthBands.fair.min) return healthBands.fair.color;
  if (score >= healthBands.poor.min) return healthBands.poor.color;
  return healthBands.critical.color;
};

// Utility function to get health band info
export const getHealthBand = (score: number) => {
  if (score >= healthBands.excellent.min) return healthBands.excellent;
  if (score >= healthBands.good.min) return healthBands.good;
  if (score >= healthBands.fair.min) return healthBands.fair;
  if (score >= healthBands.poor.min) return healthBands.poor;
  return healthBands.critical;
};

// Enhanced Material-UI theme using existing structure from App.tsx
export const repoLensTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: designTokens.brandPrimary,
      light: '#42a5f5', // Keep existing light variant
      dark: '#1565c0',  // Keep existing dark variant
    },
    secondary: {
      main: designTokens.interactive,
    },
    background: {
      default: designTokens.surface,
      paper: designTokens.card,
    },
    text: {
      primary: designTokens.textPrimary,
      secondary: designTokens.textSecondary,
    },
    success: {
      main: designTokens.statusSuccess,
    },
    warning: {
      main: designTokens.statusWarning,
    },
    error: {
      main: designTokens.statusDanger,
    },
    info: {
      main: designTokens.feature,
    },
    divider: designTokens.border,
  },
  typography: {
    fontFamily: typography.fontFamily,
    h1: typography.h1,
    h2: typography.h2,
    h3: typography.h3,
    h4: {
      fontWeight: 600,
      fontFamily: typography.fontFamily,
    },
    h5: {
      fontWeight: 600,
      fontFamily: typography.fontFamily,
    },
    h6: {
      fontWeight: 600,
      fontFamily: typography.fontFamily,
    },
    body1: typography.body,
    body2: {
      ...typography.body,
      fontSize: '13px',
    },
    caption: typography.caption,
  },
  spacing: 8, // Keep 8px base
  components: {
    // Enhanced Card component
    MuiCard: {
      styleOverrides: {
        root: {
          boxShadow: '0 2px 10px rgba(0,0,0,0.1)',
          borderRadius: '8px',
          border: `1px solid ${designTokens.border}`,
        },
      },
    },
    // Enhanced Button component
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: '6px',
          textTransform: 'none',
          fontWeight: 600,
          fontFamily: typography.fontFamily,
        },
      },
    },
    // Typography components
    MuiTypography: {
      styleOverrides: {
        root: {
          fontFamily: typography.fontFamily,
        },
      },
    },
  },
});

// Custom theme interface extending Material-UI theme
declare module '@mui/material/styles' {
  interface Theme {
    repoLens?: {
      designTokens: typeof designTokens;
      healthBands: typeof healthBands;
      spacing: typeof spacing;
      typography: typeof typography;
      breakpoints: typeof breakpoints;
    };
  }
  
  interface ThemeOptions {
    repoLens?: {
      designTokens?: typeof designTokens;
      healthBands?: typeof healthBands;
      spacing?: typeof spacing;
      typography?: typeof typography;
      breakpoints?: typeof breakpoints;
    };
  }
}

// Add custom RepoLens properties to theme
repoLensTheme.repoLens = {
  designTokens,
  healthBands,
  spacing,
  typography,
  breakpoints,
};

export default repoLensTheme;

// Named exports for convenient access
export {
  designTokens as tokens,
};
