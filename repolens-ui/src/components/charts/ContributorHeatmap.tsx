import React from 'react';
import {
  Box,
  Typography,
  Tooltip,
  Paper,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Chip
} from '@mui/material';
import {
  Person as PersonIcon,
  TrendingUp,
  Commit as CommitIcon
} from '@mui/icons-material';

interface ContributorData {
  email: string;
  name: string;
  totalCommits: number;
  weeklyData: { [week: string]: number }; // ISO week string -> commit count
}

interface ContributorHeatmapProps {
  contributors: ContributorData[];
  selectedContributor?: string;
  onContributorChange: (contributor: string) => void;
  weeks?: number; // Number of weeks to display
  title?: string;
}

const ContributorHeatmap: React.FC<ContributorHeatmapProps> = ({
  contributors,
  selectedContributor,
  onContributorChange,
  weeks = 26, // Default to 6 months
  title = "Contributor Activity"
}) => {
  // Generate the last N weeks
  const generateWeeks = (numWeeks: number): string[] => {
    const weeks: string[] = [];
    const now = new Date();
    
    for (let i = numWeeks - 1; i >= 0; i--) {
      const date = new Date(now);
      date.setDate(date.getDate() - (i * 7));
      
      // Get ISO week
      const startOfYear = new Date(date.getFullYear(), 0, 1);
      const pastDaysOfYear = (date.getTime() - startOfYear.getTime()) / 86400000;
      const week = Math.ceil((pastDaysOfYear + startOfYear.getDay() + 1) / 7);
      
      weeks.push(`${date.getFullYear()}-W${week.toString().padStart(2, '0')}`);
    }
    
    return weeks;
  };

  const weeksList = generateWeeks(weeks);
  const selectedContributorData = contributors.find(c => c.email === selectedContributor);
  
  if (!selectedContributorData) {
    return null;
  }

  // Calculate intensity levels (0-4 based on commit frequency)
  const maxCommits = Math.max(...Object.values(selectedContributorData.weeklyData), 1);
  const getIntensity = (commits: number): number => {
    if (commits === 0) return 0;
    if (commits <= maxCommits * 0.25) return 1;
    if (commits <= maxCommits * 0.5) return 2;
    if (commits <= maxCommits * 0.75) return 3;
    return 4;
  };

  const getIntensityColor = (intensity: number): string => {
    const colors = [
      '#ebedf0', // No activity
      '#c6e48b', // Low activity
      '#7bc96f', // Medium-low activity
      '#239a3b', // Medium-high activity
      '#196127', // High activity
    ];
    return colors[intensity] || colors[0];
  };

  const renderWeekSquare = (week: string, index: number) => {
    const commits = selectedContributorData.weeklyData[week] || 0;
    const intensity = getIntensity(commits);
    const color = getIntensityColor(intensity);
    
    // Parse week for tooltip
    const [year, weekNum] = week.split('-W');
    const weekStart = new Date(parseInt(year), 0, (parseInt(weekNum) - 1) * 7 + 1);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);
    
    return (
      <Tooltip
        key={week}
        title={
          <Box>
            <Typography variant="body2" fontWeight="bold">
              {commits} commit{commits !== 1 ? 's' : ''}
            </Typography>
            <Typography variant="caption">
              {weekStart.toLocaleDateString()} - {weekEnd.toLocaleDateString()}
            </Typography>
          </Box>
        }
        arrow
      >
        <Box
          sx={{
            width: 11,
            height: 11,
            backgroundColor: color,
            border: '1px solid rgba(27,31,36,0.06)',
            borderRadius: '2px',
            cursor: 'pointer',
            '&:hover': {
              border: '1px solid rgba(27,31,36,0.3)',
            }
          }}
        />
      </Tooltip>
    );
  };

  // Group weeks into rows (7 days per row to mimic GitHub layout)
  const weeksPerRow = 7;
  const rows: string[][] = [];
  for (let i = 0; i < weeksList.length; i += weeksPerRow) {
    rows.push(weeksList.slice(i, i + weeksPerRow));
  }

  const totalCommits = selectedContributorData.totalCommits;
  const activeWeeks = Object.values(selectedContributorData.weeklyData).filter(count => count > 0).length;
  const avgCommitsPerWeek = activeWeeks > 0 ? (totalCommits / activeWeeks).toFixed(1) : '0';

  return (
    <Paper sx={{ p: 3 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h6">{title}</Typography>
        
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Contributor</InputLabel>
          <Select
            value={selectedContributor || ''}
            label="Contributor"
            onChange={(e) => onContributorChange(e.target.value)}
          >
            {contributors.map((contributor) => (
              <MenuItem key={contributor.email} value={contributor.email}>
                <Box display="flex" alignItems="center" gap={1}>
                  <PersonIcon fontSize="small" />
                  <Box>
                    <Typography variant="body2">
                      {contributor.name || contributor.email}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {contributor.totalCommits} commits
                    </Typography>
                  </Box>
                </Box>
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {selectedContributorData && (
        <>
          {/* Stats */}
          <Box display="flex" gap={2} mb={3}>
            <Chip
              icon={<CommitIcon />}
              label={`${totalCommits} total commits`}
              variant="outlined"
              size="small"
            />
            <Chip
              icon={<TrendingUp />}
              label={`${avgCommitsPerWeek} avg/week`}
              variant="outlined"
              size="small"
            />
            <Chip
              label={`${activeWeeks} active weeks`}
              variant="outlined"
              size="small"
            />
          </Box>

          {/* Heatmap */}
          <Box>
            <Typography variant="caption" color="text.secondary" display="block" mb={1}>
              Activity over the last {weeks} weeks
            </Typography>
            
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'column',
                gap: 0.3,
                mb: 2,
              }}
            >
              {rows.map((row, rowIndex) => (
                <Box
                  key={rowIndex}
                  sx={{
                    display: 'flex',
                    gap: 0.3,
                  }}
                >
                  {row.map((week, weekIndex) => renderWeekSquare(week, weekIndex))}
                </Box>
              ))}
            </Box>

            {/* Legend */}
            <Box display="flex" justifyContent="space-between" alignItems="center">
              <Typography variant="caption" color="text.secondary">
                Less
              </Typography>
              
              <Box display="flex" gap={0.3}>
                {[0, 1, 2, 3, 4].map((intensity) => (
                  <Box
                    key={intensity}
                    sx={{
                      width: 11,
                      height: 11,
                      backgroundColor: getIntensityColor(intensity),
                      border: '1px solid rgba(27,31,36,0.06)',
                      borderRadius: '2px',
                    }}
                  />
                ))}
              </Box>
              
              <Typography variant="caption" color="text.secondary">
                More
              </Typography>
            </Box>
          </Box>
        </>
      )}
    </Paper>
  );
};

export default ContributorHeatmap;
