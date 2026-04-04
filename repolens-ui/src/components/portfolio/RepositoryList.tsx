/**
 * Repository List - Zone 2 of L1 Dashboard
 * Main decision-making area for Engineering Managers
 * Default sort: starred first, then health score ascending (worst first)
 */

import React, { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Skeleton,
  Tooltip
} from '@mui/material';
import {
  Star,
  StarBorder,
  FilterList,
  Clear,
  Language,
  Group,
  AccessTime
} from '@mui/icons-material';
import { 
  PortfolioRepositoryListResponse,
  PortfolioFilters,
  PortfolioRepository,
  RepositoryHealthBand,
  HEALTH_BAND_COLORS,
  HEALTH_BAND_LABELS,
  formatRelativeTime
} from '../../services/portfolioApiService';
import RepositoryHealthChip from '../RepositoryHealthChip';

interface RepositoryListProps {
  data: PortfolioRepositoryListResponse | null;
  filters: Partial<PortfolioFilters>;
  onFiltersChange: (filters: Partial<PortfolioFilters>) => void;
  onRepositoryClick: (repositoryId: number) => void;
  onStarToggle: (repositoryId: number, isStarred: boolean) => void;
  loading?: boolean;
}

export const RepositoryList: React.FC<RepositoryListProps> = ({
  data,
  filters,
  onFiltersChange,
  onRepositoryClick,
  onStarToggle,
  loading = false
}) => {
  const [showFilters, setShowFilters] = useState(false);

  // Handle filter changes
  const handleHealthBandChange = (healthBands: RepositoryHealthBand[]) => {
    onFiltersChange({ ...filters, healthBands });
  };

  const handleLanguageChange = (languages: string[]) => {
    onFiltersChange({ ...filters, languages });
  };

  const handleTeamChange = (teams: string[]) => {
    onFiltersChange({ ...filters, teams });
  };

  const handleCriticalOnlyChange = (hasCriticalIssuesOnly: boolean) => {
    onFiltersChange({ ...filters, hasCriticalIssuesOnly });
  };

  const handleStarredOnlyChange = (starredOnly: boolean) => {
    onFiltersChange({ ...filters, starredOnly });
  };

  const clearFilters = () => {
    onFiltersChange({});
    setShowFilters(false);
  };

  // Loading skeleton
  if (loading && !data) {
    return (
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <Skeleton variant="text" width={200} height={32} />
            <Skeleton variant="circular" width={32} height={32} sx={{ ml: 'auto' }} />
          </Box>
          {[1, 2, 3, 4, 5].map((index) => (
            <Box key={index} sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Skeleton variant="circular" width={24} height={24} sx={{ mr: 2 }} />
              <Skeleton variant="text" width="30%" height={24} sx={{ mr: 2 }} />
              <Skeleton variant="rectangular" width={60} height={20} sx={{ mr: 2 }} />
              <Skeleton variant="rectangular" width={80} height={20} sx={{ mr: 2 }} />
              <Skeleton variant="text" width="20%" />
            </Box>
          ))}
        </CardContent>
      </Card>
    );
  }

  if (!data) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" color="text.secondary" textAlign="center">
            No repository data available
          </Typography>
        </CardContent>
      </Card>
    );
  }

  const { repositories, totalCount, filteredCount, filterOptions } = data;
  const hasActiveFilters = Object.values(filters).some(value => 
    Array.isArray(value) ? value.length > 0 : value
  );

  return (
    <Card>
      <CardContent>
        {/* Header with title and filter toggle */}
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <Typography variant="h6" sx={{ fontWeight: 600 }}>
            Repository Health Overview
          </Typography>
          <Box sx={{ ml: 'auto', display: 'flex', gap: 1 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mr: 2 }}>
              {filteredCount} of {totalCount} repositories
            </Typography>
            <IconButton 
              size="small" 
              onClick={() => setShowFilters(!showFilters)}
              color={showFilters || hasActiveFilters ? 'primary' : 'default'}
            >
              <FilterList />
            </IconButton>
            {hasActiveFilters && (
              <IconButton size="small" onClick={clearFilters}>
                <Clear />
              </IconButton>
            )}
          </Box>
        </Box>

        {/* Filters Panel */}
        {showFilters && (
          <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              {/* Health Band Filter */}
              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Health Band</InputLabel>
                <Select
                  multiple
                  value={filters.healthBands || []}
                  onChange={(e) => handleHealthBandChange(e.target.value as RepositoryHealthBand[])}
                  label="Health Band"
                >
                  {Object.values(RepositoryHealthBand).filter(value => typeof value === 'number').map((value) => (
                    <MenuItem key={value} value={value}>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Box
                          sx={{
                            width: 12,
                            height: 12,
                            borderRadius: '50%',
                            backgroundColor: HEALTH_BAND_COLORS[value as RepositoryHealthBand],
                            mr: 1
                          }}
                        />
                        {HEALTH_BAND_LABELS[value as RepositoryHealthBand]}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              {/* Language Filter */}
              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Language</InputLabel>
                <Select
                  multiple
                  value={filters.languages || []}
                  onChange={(e) => handleLanguageChange(e.target.value as string[])}
                  label="Language"
                >
                  {filterOptions.languages.map((language) => (
                    <MenuItem key={language} value={language}>
                      {language}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              {/* Team Filter */}
              <FormControl size="small" sx={{ minWidth: 120 }}>
                <InputLabel>Team</InputLabel>
                <Select
                  multiple
                  value={filters.teams || []}
                  onChange={(e) => handleTeamChange(e.target.value as string[])}
                  label="Team"
                >
                  {filterOptions.teams.map((team) => (
                    <MenuItem key={team} value={team}>
                      {team}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              {/* Quick Filter Chips */}
              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                <Chip
                  label="Critical Only"
                  variant={filters.hasCriticalIssuesOnly ? "filled" : "outlined"}
                  color={filters.hasCriticalIssuesOnly ? "error" : "default"}
                  size="small"
                  onClick={() => handleCriticalOnlyChange(!filters.hasCriticalIssuesOnly)}
                />
                <Chip
                  label="Starred Only"
                  variant={filters.starredOnly ? "filled" : "outlined"}
                  color={filters.starredOnly ? "primary" : "default"}
                  size="small"
                  onClick={() => handleStarredOnlyChange(!filters.starredOnly)}
                />
              </Box>
            </Box>
          </Box>
        )}

        {/* Repository Table */}
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell width="40">⭐</TableCell>
                <TableCell>Repository</TableCell>
                <TableCell width="100">Language</TableCell>
                <TableCell width="120">Health</TableCell>
                <TableCell width="100">Issues</TableCell>
                <TableCell width="100">Team</TableCell>
                <TableCell width="120">Last Sync</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {repositories.map((repository) => (
                <RepositoryRow
                  key={repository.id}
                  repository={repository}
                  onClick={() => onRepositoryClick(repository.id)}
                  onStarToggle={(isStarred) => onStarToggle(repository.id, isStarred)}
                />
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {repositories.length === 0 && (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <Typography variant="body1" color="text.secondary">
              No repositories match your current filters
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              Try adjusting your filters or clearing them entirely
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

// Individual Repository Row Component
interface RepositoryRowProps {
  repository: PortfolioRepository;
  onClick: () => void;
  onStarToggle: (isStarred: boolean) => void;
}

const RepositoryRow: React.FC<RepositoryRowProps> = ({
  repository,
  onClick,
  onStarToggle
}) => {
  return (
    <TableRow
      hover
      sx={{
        cursor: 'pointer',
        '&:hover': {
          backgroundColor: 'action.hover'
        }
      }}
    >
      {/* Star Column */}
      <TableCell>
        <IconButton
          size="small"
          onClick={(e) => {
            e.stopPropagation();
            onStarToggle(!repository.isStarred);
          }}
          color={repository.isStarred ? "warning" : "default"}
        >
          {repository.isStarred ? <Star /> : <StarBorder />}
        </IconButton>
      </TableCell>

      {/* Repository Name Column */}
      <TableCell onClick={onClick}>
        <Box>
          <Typography 
            variant="body2" 
            sx={{ 
              fontWeight: repository.isStarred ? 600 : 400,
              color: repository.hasCriticalIssues ? 'error.main' : 'text.primary'
            }}
          >
            {repository.name}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {repository.teamName}
          </Typography>
        </Box>
      </TableCell>

      {/* Language Column */}
      <TableCell onClick={onClick}>
        <Chip
          label={repository.primaryLanguage}
          size="small"
          variant="outlined"
          icon={<Language fontSize="small" />}
        />
      </TableCell>

      {/* Health Column */}
      <TableCell onClick={onClick}>
        <RepositoryHealthChip 
          healthScore={repository.healthScore}
          size="small"
        />
      </TableCell>

      {/* Issues Column */}
      <TableCell onClick={onClick}>
        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
          {repository.issues.critical > 0 && (
            <Chip 
              label={repository.issues.critical}
              size="small"
              color="error"
              sx={{ minWidth: 24, height: 20, fontSize: '0.75rem' }}
            />
          )}
          {repository.issues.high > 0 && (
            <Chip 
              label={repository.issues.high}
              size="small"
              color="warning"
              sx={{ minWidth: 24, height: 20, fontSize: '0.75rem' }}
            />
          )}
          {repository.issues.medium > 0 && (
            <Chip 
              label={repository.issues.medium}
              size="small"
              color="info"
              sx={{ minWidth: 24, height: 20, fontSize: '0.75rem' }}
            />
          )}
        </Box>
      </TableCell>

      {/* Team Column */}
      <TableCell onClick={onClick}>
        <Tooltip title={repository.teamName}>
          <Chip
            label={repository.teamName.replace(' Team', '')}
            size="small"
            variant="outlined"
            icon={<Group fontSize="small" />}
          />
        </Tooltip>
      </TableCell>

      {/* Last Sync Column */}
      <TableCell onClick={onClick}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <AccessTime fontSize="small" color="action" />
          <Typography variant="caption" color="text.secondary">
            {formatRelativeTime(repository.lastSync)}
          </Typography>
        </Box>
      </TableCell>
    </TableRow>
  );
};

export default RepositoryList;
