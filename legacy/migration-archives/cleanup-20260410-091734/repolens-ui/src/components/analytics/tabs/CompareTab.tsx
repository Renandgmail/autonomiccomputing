import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert
} from '@mui/material';
import {
  Compare,
  GetApp,
  Add,
  Close
} from '@mui/icons-material';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface CompareTabProps {
  repositoryId: number;
  repository: Repository;
}

interface ComparisonRepo {
  id: number;
  name: string;
  healthScore: number;
  avgComplexity: number;
  technicalDebt: number;
  testCoverage: number;
  criticalSecurityIssues: number;
  activeContributors: number;
  linesOfCode: number;
}

const CompareTab: React.FC<CompareTabProps> = ({ repositoryId, repository }) => {
  const [selectedRepos, setSelectedRepos] = useState<number[]>([]);
  const [availableRepos] = useState<Repository[]>([
    { id: 1, name: 'repolens-frontend', url: 'https://github.com/org/repolens-frontend' },
    { id: 2, name: 'analytics-service', url: 'https://github.com/org/analytics-service' },
    { id: 3, name: 'user-management', url: 'https://github.com/org/user-management' },
    { id: 4, name: 'payment-gateway', url: 'https://github.com/org/payment-gateway' }
  ]);

  // Mock comparison data
  const currentRepo: ComparisonRepo = {
    id: repositoryId,
    name: repository.name,
    healthScore: 85,
    avgComplexity: 6.2,
    technicalDebt: 24.5,
    testCoverage: 78,
    criticalSecurityIssues: 2,
    activeContributors: 8,
    linesOfCode: 45230
  };

  const comparisonData: ComparisonRepo[] = [
    currentRepo,
    {
      id: 1,
      name: 'repolens-frontend',
      healthScore: 92,
      avgComplexity: 4.8,
      technicalDebt: 18.2,
      testCoverage: 85,
      criticalSecurityIssues: 0,
      activeContributors: 12,
      linesOfCode: 38450
    },
    {
      id: 2,
      name: 'analytics-service',
      healthScore: 76,
      avgComplexity: 7.1,
      technicalDebt: 31.8,
      testCoverage: 65,
      criticalSecurityIssues: 3,
      activeContributors: 6,
      linesOfCode: 28340
    }
  ].filter(repo => repo.id === repositoryId || selectedRepos.includes(repo.id));

  const addRepository = (repoId: number) => {
    if (selectedRepos.length < 2 && !selectedRepos.includes(repoId)) {
      setSelectedRepos([...selectedRepos, repoId]);
    }
  };

  const removeRepository = (repoId: number) => {
    setSelectedRepos(selectedRepos.filter(id => id !== repoId));
  };

  const handleExport = (format: 'PDF' | 'CSV') => {
    // In real implementation, this would generate and download the file
    console.log(`Exporting comparison as ${format}`, comparisonData);
  };

  const getScoreColor = (value: number, isReverse = false) => {
    if (isReverse) {
      return value <= 2 ? '#4caf50' : value <= 5 ? '#ff9800' : '#f44336';
    }
    return value >= 80 ? '#4caf50' : value >= 60 ? '#ff9800' : '#f44336';
  };

  const formatNumber = (value: number, suffix = '') => {
    if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}K${suffix}`;
    }
    return `${value}${suffix}`;
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Repository Comparison
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        Side-by-side comparison of up to 3 repositories for executive reviews and portfolio decisions
      </Typography>

      {/* Repository Selector */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Add Repositories to Compare
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Select up to 2 additional repositories to compare against {repository.name}
          </Typography>

          <Box display="flex" gap={2} alignItems="center" flexWrap="wrap">
            <FormControl size="small" sx={{ minWidth: 200 }} disabled={selectedRepos.length >= 2}>
              <InputLabel>Select Repository</InputLabel>
              <Select value="" label="Select Repository">
                <MenuItem value="" disabled>
                  <em>Choose a repository...</em>
                </MenuItem>
                {availableRepos
                  .filter(repo => !selectedRepos.includes(repo.id))
                  .map(repo => (
                    <MenuItem
                      key={repo.id}
                      value={repo.id}
                      onClick={() => addRepository(repo.id)}
                    >
                      {repo.name}
                    </MenuItem>
                  ))}
              </Select>
            </FormControl>

            {selectedRepos.length >= 2 && (
              <Typography variant="body2" color="text.secondary">
                Maximum 3 repositories (including current)
              </Typography>
            )}
          </Box>

          {/* Selected Repositories */}
          {selectedRepos.length > 0 && (
            <Box display="flex" gap={1} mt={2} flexWrap="wrap">
              <Chip
                label={`${repository.name} (current)`}
                color="primary"
                variant="filled"
              />
              {selectedRepos.map(repoId => {
                const repo = availableRepos.find(r => r.id === repoId);
                return repo ? (
                  <Chip
                    key={repoId}
                    label={repo.name}
                    onDelete={() => removeRepository(repoId)}
                    color="secondary"
                  />
                ) : null;
              })}
            </Box>
          )}
        </CardContent>
      </Card>

      {/* Comparison Table */}
      {comparisonData.length > 1 ? (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">
                Metrics Comparison
              </Typography>
              <Box display="flex" gap={1}>
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<GetApp />}
                  onClick={() => handleExport('PDF')}
                >
                  Export PDF
                </Button>
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<GetApp />}
                  onClick={() => handleExport('CSV')}
                >
                  Export CSV
                </Button>
              </Box>
            </Box>

            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Metric</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <strong>{repo.name}</strong>
                        {repo.id === repositoryId && (
                          <Chip size="small" label="Current" sx={{ ml: 1 }} />
                        )}
                      </TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  <TableRow>
                    <TableCell><strong>Overall Health Score</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography
                          sx={{
                            color: getScoreColor(repo.healthScore),
                            fontWeight: 'bold'
                          }}
                        >
                          {repo.healthScore}%
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Average Complexity</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography
                          sx={{
                            color: getScoreColor(repo.avgComplexity * 10, true),
                            fontWeight: 'bold'
                          }}
                        >
                          {repo.avgComplexity}/10
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Technical Debt</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography
                          sx={{
                            color: getScoreColor(100 - repo.technicalDebt * 2, true),
                            fontWeight: 'bold'
                          }}
                        >
                          {repo.technicalDebt}h
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Test Coverage</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography
                          sx={{
                            color: getScoreColor(repo.testCoverage),
                            fontWeight: 'bold'
                          }}
                        >
                          {repo.testCoverage}%
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Critical Security Issues</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography
                          sx={{
                            color: getScoreColor(repo.criticalSecurityIssues, true),
                            fontWeight: 'bold'
                          }}
                        >
                          {repo.criticalSecurityIssues}
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Active Contributors</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography sx={{ fontWeight: 'bold' }}>
                          {repo.activeContributors}
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                  <TableRow>
                    <TableCell><strong>Lines of Code</strong></TableCell>
                    {comparisonData.map(repo => (
                      <TableCell key={repo.id} align="center">
                        <Typography sx={{ fontWeight: 'bold' }}>
                          {formatNumber(repo.linesOfCode)}
                        </Typography>
                      </TableCell>
                    ))}
                  </TableRow>
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      ) : (
        <Alert severity="info">
          Add repositories above to see a side-by-side comparison. You can compare up to 3 repositories total.
        </Alert>
      )}

      {/* Legend */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Color Legend
          </Typography>
          <Box display="flex" gap={3} flexWrap="wrap">
            <Box display="flex" alignItems="center" gap={1}>
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  backgroundColor: '#4caf50',
                  borderRadius: 1
                }}
              />
              <Typography variant="body2">Good / Low Risk</Typography>
            </Box>
            <Box display="flex" alignItems="center" gap={1}>
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  backgroundColor: '#ff9800',
                  borderRadius: 1
                }}
              />
              <Typography variant="body2">Moderate / Medium Risk</Typography>
            </Box>
            <Box display="flex" alignItems="center" gap={1}>
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  backgroundColor: '#f44336',
                  borderRadius: 1
                }}
              />
              <Typography variant="body2">Poor / High Risk</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default CompareTab;
