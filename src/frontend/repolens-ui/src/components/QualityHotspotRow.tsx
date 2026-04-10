/**
 * Quality Hotspot Row Component
 * Displays ranked files needing attention with severity indicators and metrics
 * Used in Repository Dashboard quality panels and file analytics views
 */

import React from 'react';
import { Box, Typography, Button, Chip, useTheme } from '@mui/material';
import { ArrowForward, Code, BugReport, Warning, Info } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

// Quality hotspot data interface
export interface QualityHotspot {
  id: string;
  filePath: string;
  complexityScore: number;      // 0-100
  churnRate: number;           // 0-100 percentage
  qualityDeficit: number;      // 0-100
  urgencyScore: number;        // Combined ranking score 0-100
  language: string;
  lineCount: number;
  lastModified: string;        // ISO date string
  issues?: {
    security: number;
    bugs: number;
    codeSmells: number;
  };
}

export interface QualityHotspotRowProps {
  hotspot: QualityHotspot;
  repositoryId: number;
  rank: number;               // 1, 2, 3, etc.
  onViewFile?: (filePath: string) => void;
  showMetrics?: boolean;      // Show complexity/churn metrics
  compact?: boolean;          // Compact layout for mobile
}

// Severity Badge Sub-component
interface SeverityBadgeProps {
  urgencyScore: number;
  size?: 'small' | 'medium';
}

const SeverityBadge: React.FC<SeverityBadgeProps> = ({ urgencyScore, size = 'small' }) => {
  const theme = useTheme();
  
  // Determine severity based on urgency score
  const getSeverity = (score: number): { level: 'high' | 'medium' | 'low'; color: string; icon: React.ElementType } => {
    if (score >= 80) return { 
      level: 'high', 
      color: theme.palette.error.main,
      icon: BugReport
    };
    if (score >= 50) return { 
      level: 'medium', 
      color: theme.palette.warning.main,
      icon: Warning
    };
    return { 
      level: 'low', 
      color: theme.palette.info.main,
      icon: Info
    };
  };
  
  const severity = getSeverity(urgencyScore);
  const SeverityIcon = severity.icon;
  
  return (
    <Chip
      icon={<SeverityIcon sx={{ fontSize: 14 }} />}
      label={severity.level.toUpperCase()}
      size={size}
      sx={{
        backgroundColor: severity.color,
        color: 'white',
        fontWeight: 600,
        fontSize: size === 'small' ? '11px' : '12px',
        '& .MuiChip-icon': {
          color: 'white',
        },
      }}
    />
  );
};

export const QualityHotspotRow: React.FC<QualityHotspotRowProps> = ({
  hotspot,
  repositoryId,
  rank,
  onViewFile,
  showMetrics = true,
  compact = false
}) => {
  const theme = useTheme();
  const navigate = useNavigate();
  
  // Handle file navigation
  const handleViewFile = () => {
    if (onViewFile) {
      onViewFile(hotspot.filePath);
    } else {
      // Default navigation to file detail view
      navigate(`/repos/${repositoryId}/files/${encodeURIComponent(hotspot.filePath)}`);
    }
  };
  
  // Format file path for display
  const getDisplayPath = (path: string): { dir: string; file: string } => {
    const parts = path.split('/');
    const fileName = parts.pop() || path;
    const directory = parts.length > 0 ? parts.join('/') + '/' : '';
    return { dir: directory, file: fileName };
  };
  
  const { dir, file } = getDisplayPath(hotspot.filePath);
  
  // Format last modified date
  const formatDate = (dateString: string): string => {
    try {
      const date = new Date(dateString);
      const now = new Date();
      const diffTime = Math.abs(now.getTime() - date.getTime());
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      if (diffDays === 1) return '1 day ago';
      if (diffDays < 7) return `${diffDays} days ago`;
      if (diffDays < 30) return `${Math.ceil(diffDays / 7)} weeks ago`;
      return date.toLocaleDateString();
    } catch {
      return 'Recently';
    }
  };

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        py: compact ? 1.5 : 2,
        px: compact ? 2 : 3,
        borderBottom: `1px solid ${theme.palette.divider}`,
        transition: 'background-color 0.2s ease-in-out',
        '&:hover': {
          backgroundColor: theme.palette.action.hover,
        },
        flexDirection: compact ? 'column' : 'row',
        gap: compact ? 1 : 0,
      }}
    >
      {/* Rank Badge */}
      <Box
        sx={{
          width: compact ? 24 : 32,
          height: compact ? 24 : 32,
          borderRadius: '50%',
          backgroundColor: rank <= 3 ? theme.palette.error.main : 
                          rank <= 10 ? theme.palette.warning.main : 
                          theme.palette.info.main,
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          fontSize: compact ? '12px' : '14px',
          fontWeight: 600,
          flexShrink: 0,
          mr: compact ? 0 : 2,
        }}
      >
        {rank}
      </Box>
      
      {/* File Information */}
      <Box sx={{ flexGrow: 1, minWidth: 0 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 0.5 }}>
          <Code sx={{ 
            fontSize: 16, 
            color: theme.palette.text.secondary, 
            mr: 1,
            flexShrink: 0
          }} />
          <Typography
            sx={{
              fontSize: compact ? '13px' : '14px',
              fontWeight: 500,
              color: theme.palette.text.primary,
              fontFamily: theme.repoLens?.typography.fontFamilyMono || '"IBM Plex Mono", monospace',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
            title={hotspot.filePath}
          >
            {compact ? file : (
              <>
                <span style={{ color: theme.palette.text.secondary }}>{dir}</span>
                <span style={{ fontWeight: 600 }}>{file}</span>
              </>
            )}
          </Typography>
        </Box>
        
        {/* File Metadata */}
        <Box sx={{ 
          display: 'flex', 
          gap: compact ? 1 : 2, 
          alignItems: 'center',
          flexWrap: 'wrap'
        }}>
          <Typography
            sx={{
              fontSize: '12px',
              color: theme.palette.text.secondary,
            }}
          >
            {hotspot.language} • {hotspot.lineCount.toLocaleString()} lines
          </Typography>
          
          <Typography
            sx={{
              fontSize: '12px',
              color: theme.palette.text.secondary,
            }}
          >
            Modified {formatDate(hotspot.lastModified)}
          </Typography>
          
          {/* Issue Count Summary */}
          {hotspot.issues && (hotspot.issues.security + hotspot.issues.bugs + hotspot.issues.codeSmells > 0) && (
            <Typography
              sx={{
                fontSize: '12px',
                color: theme.palette.warning.main,
                fontWeight: 500,
              }}
            >
              {hotspot.issues.security + hotspot.issues.bugs + hotspot.issues.codeSmells} issues
            </Typography>
          )}
        </Box>
      </Box>
      
      {/* Metrics Display */}
      {showMetrics && !compact && (
        <Box sx={{ 
          display: 'flex', 
          gap: 2, 
          mr: 2,
          minWidth: 120
        }}>
          <Box sx={{ textAlign: 'center' }}>
            <Typography sx={{ 
              fontSize: '12px', 
              color: theme.palette.text.secondary,
              lineHeight: 1
            }}>
              Complexity
            </Typography>
            <Typography sx={{ 
              fontSize: '14px', 
              fontWeight: 500,
              color: hotspot.complexityScore >= 80 ? theme.palette.error.main :
                     hotspot.complexityScore >= 60 ? theme.palette.warning.main :
                     theme.palette.text.primary
            }}>
              {Math.round(hotspot.complexityScore)}
            </Typography>
          </Box>
          
          <Box sx={{ textAlign: 'center' }}>
            <Typography sx={{ 
              fontSize: '12px', 
              color: theme.palette.text.secondary,
              lineHeight: 1
            }}>
              Churn
            </Typography>
            <Typography sx={{ 
              fontSize: '14px', 
              fontWeight: 500,
              color: hotspot.churnRate >= 80 ? theme.palette.error.main :
                     hotspot.churnRate >= 60 ? theme.palette.warning.main :
                     theme.palette.text.primary
            }}>
              {Math.round(hotspot.churnRate)}%
            </Typography>
          </Box>
        </Box>
      )}
      
      {/* Severity Badge and Action */}
      <Box sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        gap: compact ? 1 : 2,
        flexShrink: 0,
        alignSelf: compact ? 'flex-end' : 'center'
      }}>
        <SeverityBadge 
          urgencyScore={hotspot.urgencyScore} 
          size={compact ? 'small' : 'medium'} 
        />
        
        <Button
          size="small"
          variant="outlined"
          endIcon={<ArrowForward sx={{ fontSize: 16 }} />}
          onClick={handleViewFile}
          sx={{
            minWidth: 'auto',
            fontSize: '12px',
            textTransform: 'none',
            py: 0.5,
            px: 1.5,
          }}
        >
          View
        </Button>
      </Box>
    </Box>
  );
};

// Default export for easier importing
export default QualityHotspotRow;
