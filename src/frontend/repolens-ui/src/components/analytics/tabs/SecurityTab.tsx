import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Chip,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Divider
} from '@mui/material';
import {
  Security,
  Warning,
  Error,
  CheckCircle,
  Block
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface SecurityTabProps {
  repositoryId: number;
  repository: Repository;
}

interface SecurityIssue {
  id: string;
  filePath: string;
  type: 'SQL Injection' | 'XSS' | 'Auth' | 'Encryption' | 'Config' | 'Other';
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  dateDetected: string;
  status: 'Open' | 'Acknowledged' | 'Resolved';
}

const SecurityTab: React.FC<SecurityTabProps> = ({ repositoryId, repository }) => {
  const navigate = useNavigate();
  const [severityFilter, setSeverityFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');

  // Mock security issues data following L3_ANALYTICS spec
  const mockIssues: SecurityIssue[] = [
    {
      id: 'SEC-001',
      filePath: 'src/auth/LoginController.cs',
      type: 'Auth',
      severity: 'Critical',
      description: 'JWT token validation bypassed in authentication middleware',
      dateDetected: '2 days ago',
      status: 'Open'
    },
    {
      id: 'SEC-002',
      filePath: 'src/api/UserController.cs',
      type: 'SQL Injection',
      severity: 'Critical',
      description: 'User input not sanitized in database query',
      dateDetected: '5 days ago',
      status: 'Open'
    },
    {
      id: 'SEC-003',
      filePath: 'src/web/ProfilePage.tsx',
      type: 'XSS',
      severity: 'High',
      description: 'User-generated content rendered without escaping',
      dateDetected: '1 week ago',
      status: 'Acknowledged'
    },
    {
      id: 'SEC-004',
      filePath: 'config/database.json',
      type: 'Config',
      severity: 'High',
      description: 'Database credentials exposed in configuration file',
      dateDetected: '1 week ago',
      status: 'Open'
    },
    {
      id: 'SEC-005',
      filePath: 'src/crypto/PasswordHash.cs',
      type: 'Encryption',
      severity: 'High',
      description: 'Weak hashing algorithm used for password storage',
      dateDetected: '2 weeks ago',
      status: 'Acknowledged'
    }
  ];

  // Filter issues
  const filteredIssues = mockIssues.filter(issue => {
    if (severityFilter && issue.severity !== severityFilter) return false;
    if (typeFilter && issue.type !== typeFilter) return false;
    if (statusFilter && issue.status !== statusFilter) return false;
    return true;
  });

  // Get counts by severity
  const severityCounts = {
    Critical: mockIssues.filter(i => i.severity === 'Critical').length,
    High: mockIssues.filter(i => i.severity === 'High').length,
    Medium: mockIssues.filter(i => i.severity === 'Medium').length,
    Low: mockIssues.filter(i => i.severity === 'Low').length
  };

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'Critical': return '#d32f2f';
      case 'High': return '#ff9800';
      case 'Medium': return '#ffc107';
      case 'Low': return '#4caf50';
      default: return '#757575';
    }
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'Critical': return <Error />;
      case 'High': return <Warning />;
      case 'Medium': return <Warning />;
      case 'Low': return <CheckCircle />;
      default: return <Security />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Open': return '#f44336';
      case 'Acknowledged': return '#ff9800';
      case 'Resolved': return '#4caf50';
      default: return '#757575';
    }
  };

  const handleStatusUpdate = (issueId: string, newStatus: 'Acknowledged' | 'Resolved') => {
    // In real implementation, this would call an API
    console.log(`Update issue ${issueId} to ${newStatus}`);
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Security Vulnerabilities
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        Identify and track security issues for compliance checks and security reviews
      </Typography>

      {/* Severity Summary Strip */}
      <Box display="flex" gap={1} mb={3} flexWrap="wrap">
        {Object.entries(severityCounts).map(([severity, count]) => (
          <Chip
            key={severity}
            label={`${count} ${severity}`}
            onClick={() => setSeverityFilter(severityFilter === severity ? '' : severity)}
            sx={{
              backgroundColor: severityFilter === severity ? getSeverityColor(severity) : 'transparent',
              color: severityFilter === severity ? 'white' : getSeverityColor(severity),
              border: `1px solid ${getSeverityColor(severity)}`,
              fontWeight: 600,
              cursor: 'pointer',
              '&:hover': {
                backgroundColor: getSeverityColor(severity),
                color: 'white'
              }
            }}
          />
        ))}
      </Box>

      {/* Filters */}
      <Box display="flex" gap={2} mb={3} flexWrap="wrap">
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Severity</InputLabel>
          <Select
            value={severityFilter}
            label="Severity"
            onChange={(e) => setSeverityFilter(e.target.value)}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Critical">Critical</MenuItem>
            <MenuItem value="High">High</MenuItem>
            <MenuItem value="Medium">Medium</MenuItem>
            <MenuItem value="Low">Low</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Type</InputLabel>
          <Select
            value={typeFilter}
            label="Type"
            onChange={(e) => setTypeFilter(e.target.value)}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="SQL Injection">SQL Injection</MenuItem>
            <MenuItem value="XSS">XSS</MenuItem>
            <MenuItem value="Auth">Auth</MenuItem>
            <MenuItem value="Encryption">Encryption</MenuItem>
            <MenuItem value="Config">Config</MenuItem>
            <MenuItem value="Other">Other</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={statusFilter}
            label="Status"
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <MenuItem value="">All</MenuItem>
            <MenuItem value="Open">Open</MenuItem>
            <MenuItem value="Acknowledged">Acknowledged</MenuItem>
            <MenuItem value="Resolved">Resolved</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {/* Security Issues List */}
      <Box>
        {filteredIssues.map((issue) => (
          <Card key={issue.id} sx={{ mb: 2 }}>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2}>
                <Box display="flex" alignItems="center" gap={2}>
                  <Box sx={{ color: getSeverityColor(issue.severity) }}>
                    {getSeverityIcon(issue.severity)}
                  </Box>
                  <Box>
                    <Typography variant="h6" component="h3">
                      {issue.type} - {issue.severity}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{
                        fontFamily: 'monospace',
                        color: 'primary.main',
                        cursor: 'pointer'
                      }}
                      onClick={() => navigate(`/repos/${repositoryId}/files/${encodeURIComponent(issue.filePath)}`)}
                    >
                      {issue.filePath}
                    </Typography>
                  </Box>
                </Box>
                
                <Chip
                  label={issue.status}
                  size="small"
                  sx={{
                    backgroundColor: getStatusColor(issue.status),
                    color: 'white'
                  }}
                />
              </Box>

              <Typography variant="body2" color="text.secondary" paragraph>
                {issue.description}
              </Typography>

              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Typography variant="caption" color="text.secondary">
                  Detected {issue.dateDetected} • Issue ID: {issue.id}
                </Typography>

                <Box display="flex" gap={1}>
                  {issue.status === 'Open' && (
                    <>
                      <Button
                        size="small"
                        variant="outlined"
                        onClick={() => handleStatusUpdate(issue.id, 'Acknowledged')}
                      >
                        Acknowledge
                      </Button>
                      <Button
                        size="small"
                        variant="contained"
                        color="success"
                        onClick={() => handleStatusUpdate(issue.id, 'Resolved')}
                      >
                        Mark Resolved
                      </Button>
                    </>
                  )}
                  {issue.status === 'Acknowledged' && (
                    <Button
                      size="small"
                      variant="contained"
                      color="success"
                      onClick={() => handleStatusUpdate(issue.id, 'Resolved')}
                    >
                      Mark Resolved
                    </Button>
                  )}
                </Box>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>

      {filteredIssues.length === 0 && (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <CheckCircle sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
            <Typography variant="h6" gutterBottom>
              No Security Issues Found
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {severityFilter || typeFilter || statusFilter
                ? 'No issues match your current filter criteria.'
                : 'All security scans passed. Keep up the good work!'}
            </Typography>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default SecurityTab;
