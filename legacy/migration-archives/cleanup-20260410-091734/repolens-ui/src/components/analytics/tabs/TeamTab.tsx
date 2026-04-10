import React, { useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Alert
} from '@mui/material';
import { Group, Visibility } from '@mui/icons-material';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface TeamTabProps {
  repositoryId: number;
  repository: Repository;
  timeRange: number;
}

const TeamTab: React.FC<TeamTabProps> = ({ repositoryId, repository, timeRange }) => {
  const [showIndividualView, setShowIndividualView] = useState(false);

  // Following L3_ANALYTICS spec privacy rules
  const teamLevelData = {
    totalCommits: 1247,
    activeContributors: 8,
    avgReviewTime: '2.3 days',
    moduleOwnership: [
      { module: 'Frontend', distributed: 65, concentrated: 35 },
      { module: 'Backend API', distributed: 40, concentrated: 60 },
      { module: 'Infrastructure', distributed: 20, concentrated: 80 },
      { module: 'Tests', distributed: 75, concentrated: 25 }
    ]
  };

  const individualData = [
    { name: 'Contributor A', commits: 234, quality: 'stable', reviewActivity: 'high', focus: 'Frontend Components' },
    { name: 'Contributor B', commits: 189, quality: 'improving', reviewActivity: 'medium', focus: 'Backend Services' },
    { name: 'Contributor C', commits: 156, quality: 'stable', reviewActivity: 'high', focus: 'Infrastructure' },
    { name: 'Contributor D', commits: 143, quality: 'stable', reviewActivity: 'low', focus: 'Frontend & Backend' }
  ];

  if (!showIndividualView) {
    // Team-level view (default) - Following L3_ANALYTICS spec
    return (
      <Box>
        <Typography variant="h6" gutterBottom>
          Team Collaboration Overview
        </Typography>
        <Typography variant="body2" color="text.secondary" paragraph>
          Aggregate team metrics over the last {timeRange} days
        </Typography>

        {/* Team Metrics Cards */}
        <Box display="flex" gap={2} mb={4} flexWrap="wrap">
          <Card sx={{ minWidth: 200 }}>
            <CardContent>
              <Typography variant="h4" component="h2" fontWeight="bold" color="primary.main">
                {teamLevelData.totalCommits}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Total Commits
              </Typography>
            </CardContent>
          </Card>

          <Card sx={{ minWidth: 200 }}>
            <CardContent>
              <Typography variant="h4" component="h2" fontWeight="bold" color="primary.main">
                {teamLevelData.activeContributors}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Active Contributors
              </Typography>
            </CardContent>
          </Card>

          <Card sx={{ minWidth: 200 }}>
            <CardContent>
              <Typography variant="h4" component="h2" fontWeight="bold" color="primary.main">
                {teamLevelData.avgReviewTime}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Avg Review Time
              </Typography>
            </CardContent>
          </Card>
        </Box>

        {/* Module Ownership Distribution */}
        <Card sx={{ mb: 4 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Module Ownership Distribution
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Shows knowledge distribution vs concentration - identifies potential silos
            </Typography>
            
            {teamLevelData.moduleOwnership.map((module) => (
              <Box key={module.module} sx={{ mb: 2 }}>
                <Typography variant="subtitle2" gutterBottom>
                  {module.module}
                </Typography>
                <Box display="flex" alignItems="center" gap={2}>
                  <Box
                    sx={{
                      flexGrow: 1,
                      height: 24,
                      backgroundColor: '#f5f5f5',
                      borderRadius: 1,
                      overflow: 'hidden',
                      position: 'relative'
                    }}
                  >
                    <Box
                      sx={{
                        width: `${module.distributed}%`,
                        height: '100%',
                        backgroundColor: '#4caf50',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center'
                      }}
                    >
                      <Typography variant="caption" color="white" fontWeight="bold">
                        {module.distributed}% Distributed
                      </Typography>
                    </Box>
                  </Box>
                  <Typography variant="body2" color="text.secondary" sx={{ minWidth: 100 }}>
                    {module.concentrated}% Concentrated
                  </Typography>
                </Box>
              </Box>
            ))}
          </CardContent>
        </Card>

        {/* Team Velocity Placeholder */}
        <Card sx={{ mb: 4 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Team Velocity Trend
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Effective code changes per sprint (aggregate team data)
            </Typography>
            
            <Box sx={{ height: 200, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                Bar chart showing weekly team velocity would be displayed here
              </Typography>
            </Box>
          </CardContent>
        </Card>

        {/* CTA to Individual View */}
        <Box display="flex" justifyContent="center">
          <Button
            variant="outlined"
            startIcon={<Visibility />}
            onClick={() => setShowIndividualView(true)}
            sx={{ textTransform: 'none' }}
          >
            View by contributor →
          </Button>
        </Box>
      </Box>
    );
  }

  // Individual view (second click) - Following L3_ANALYTICS privacy rules
  return (
    <Box>
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <Button
          variant="text"
          onClick={() => setShowIndividualView(false)}
          sx={{ textTransform: 'none' }}
        >
          ← Back to Team View
        </Button>
        <Typography variant="h6">
          Individual Contributor View
        </Typography>
      </Box>

      <Alert severity="info" sx={{ mb: 3 }}>
        Individual data requires explicit navigation. No rankings, comparisons, or performance scores are shown.
      </Alert>

      {/* Individual Contributors */}
      <Box display="flex" flexDirection="column" gap={2}>
        {individualData.map((contributor, index) => (
          <Card key={index}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                {contributor.name}
              </Typography>
              
              <Box display="flex" gap={4} flexWrap="wrap">
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Recent Activity
                  </Typography>
                  <Typography variant="body1">
                    {contributor.commits} commits (last {timeRange} days)
                  </Typography>
                </Box>
                
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Quality Trends
                  </Typography>
                  <Typography variant="body1">
                    {contributor.quality}
                  </Typography>
                </Box>
                
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Review Activity
                  </Typography>
                  <Typography variant="body1">
                    {contributor.reviewActivity}
                  </Typography>
                </Box>
                
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Areas of Focus
                  </Typography>
                  <Typography variant="body1">
                    {contributor.focus}
                  </Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Box>
  );
};

export default TeamTab;
