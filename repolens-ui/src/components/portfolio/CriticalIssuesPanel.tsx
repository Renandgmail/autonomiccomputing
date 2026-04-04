/**
 * Critical Issues Panel - Zone 3 of L1 Dashboard
 * Conditional display: only shown when >= 1 repository has critical issues
 * Maximum 5 items shown before "See all X critical issues" link
 */

import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  Button,
  Divider,
  Alert
} from '@mui/material';
import {
  Warning,
  Security,
  Schedule,
  BugReport,
  Speed,
  Policy,
  Assignment,
  ChevronRight
} from '@mui/icons-material';
import { 
  CriticalIssue,
  CriticalIssueType,
  IssueSeverity,
  CRITICAL_ISSUE_TYPE_LABELS,
  formatRelativeTime
} from '../../services/portfolioApiService';

interface CriticalIssuesPanelProps {
  issues: CriticalIssue[];
  onIssueClick: (issue: CriticalIssue) => void;
  onViewAllClick: () => void;
  totalCriticalRepositories: number;
}

export const CriticalIssuesPanel: React.FC<CriticalIssuesPanelProps> = ({
  issues,
  onIssueClick,
  onViewAllClick,
  totalCriticalRepositories
}) => {
  // Don't render if no issues
  if (issues.length === 0) {
    return null;
  }

  // Get icon for issue type
  const getIssueTypeIcon = (type: CriticalIssueType) => {
    switch (type) {
      case CriticalIssueType.Security:
        return <Security fontSize="small" />;
      case CriticalIssueType.TechnicalDebt:
        return <Assignment fontSize="small" />;
      case CriticalIssueType.TestCoverage:
        return <BugReport fontSize="small" />;
      case CriticalIssueType.HealthCritical:
        return <Warning fontSize="small" />;
      case CriticalIssueType.Stale:
        return <Schedule fontSize="small" />;
      case CriticalIssueType.Performance:
        return <Speed fontSize="small" />;
      case CriticalIssueType.Compliance:
        return <Policy fontSize="small" />;
      default:
        return <Warning fontSize="small" />;
    }
  };

  // Get color for severity
  const getSeverityColor = (severity: IssueSeverity): "error" | "warning" | "info" => {
    if (severity >= IssueSeverity.Critical) return "error";
    if (severity >= IssueSeverity.High) return "warning";
    return "info";
  };

  // Get severity label
  const getSeverityLabel = (severity: IssueSeverity): string => {
    switch (severity) {
      case IssueSeverity.Critical:
        return "Critical";
      case IssueSeverity.High:
        return "High";
      case IssueSeverity.Medium:
        return "Medium";
      case IssueSeverity.Low:
        return "Low";
      default:
        return "Info";
    }
  };

  // Show maximum 5 issues, then "See all" link
  const displayedIssues = issues.slice(0, 5);
  const hasMoreIssues = issues.length > 5;
  const remainingCount = issues.length - 5;

  return (
    <Card sx={{ height: 'fit-content' }}>
      <CardContent>
        {/* Panel Header */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
          <Warning sx={{ color: 'error.main', mr: 1 }} />
          <Typography variant="h6" sx={{ fontWeight: 600, color: 'error.main' }}>
            Critical Issues
          </Typography>
        </Box>

        {/* Engineering Manager Alert */}
        <Alert 
          severity="error" 
          sx={{ 
            mb: 2,
            backgroundColor: 'rgba(211, 47, 47, 0.1)',
            border: '1px solid rgba(211, 47, 47, 0.3)'
          }}
        >
          <Typography variant="body2" sx={{ fontWeight: 600 }}>
            {totalCriticalRepositories} {totalCriticalRepositories === 1 ? 'repository requires' : 'repositories require'} immediate attention
          </Typography>
        </Alert>

        {/* Issues List */}
        <List disablePadding>
          {displayedIssues.map((issue, index) => (
            <React.Fragment key={issue.id}>
              <ListItem
                sx={{
                  px: 0,
                  py: 1,
                  cursor: 'pointer',
                  borderRadius: 1,
                  '&:hover': {
                    backgroundColor: 'action.hover'
                  }
                }}
                onClick={() => onIssueClick(issue)}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <Box
                    sx={{
                      color: getSeverityColor(issue.severity) + '.main',
                      display: 'flex',
                      alignItems: 'center'
                    }}
                  >
                    {getIssueTypeIcon(issue.type)}
                  </Box>
                </ListItemIcon>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                      <Typography 
                        variant="body2" 
                        sx={{ 
                          fontWeight: 600,
                          color: 'text.primary',
                          fontSize: '0.875rem'
                        }}
                      >
                        {issue.repositoryName}
                      </Typography>
                      <Chip
                        label={getSeverityLabel(issue.severity)}
                        size="small"
                        color={getSeverityColor(issue.severity)}
                        sx={{ height: 18, fontSize: '0.7rem' }}
                      />
                    </Box>
                  }
                  secondary={
                    <Box>
                      <Typography 
                        variant="body2" 
                        color="text.secondary"
                        sx={{ 
                          fontSize: '0.8rem',
                          mb: 0.5,
                          lineHeight: 1.3
                        }}
                      >
                        {issue.description}
                      </Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography 
                          variant="caption" 
                          color="text.secondary"
                          sx={{ fontSize: '0.7rem' }}
                        >
                          {CRITICAL_ISSUE_TYPE_LABELS[issue.type]}
                        </Typography>
                        <Typography 
                          variant="caption" 
                          color="text.secondary"
                          sx={{ fontSize: '0.7rem' }}
                        >
                          • {formatRelativeTime(issue.detectedAt)}
                        </Typography>
                      </Box>
                    </Box>
                  }
                />
                <ChevronRight fontSize="small" color="action" />
              </ListItem>
              {index < displayedIssues.length - 1 && (
                <Divider sx={{ mx: 0 }} />
              )}
            </React.Fragment>
          ))}
        </List>

        {/* See All Link */}
        {hasMoreIssues && (
          <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
            <Button
              variant="text"
              color="error"
              size="small"
              onClick={onViewAllClick}
              endIcon={<ChevronRight />}
              sx={{ 
                textTransform: 'none',
                fontWeight: 600,
                width: '100%',
                justifyContent: 'space-between'
              }}
            >
              See all {remainingCount} more critical issues
            </Button>
          </Box>
        )}

        {/* Action Guidance */}
        <Box sx={{ mt: 2, pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
          <Typography 
            variant="caption" 
            color="text.secondary" 
            sx={{ 
              display: 'block',
              textAlign: 'center',
              fontSize: '0.75rem',
              fontStyle: 'italic'
            }}
          >
            Click any issue to navigate directly to the problem
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export default CriticalIssuesPanel;
