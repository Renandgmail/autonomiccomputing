import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Alert,
  Chip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  InputAdornment
} from '@mui/material';
import { Search, FilterList } from '@mui/icons-material';
import QualityHotspotRow from '../../QualityHotspotRow';
import apiService from '../../../services/apiService';

interface Repository {
  id: number;
  name: string;
  url: string;
}

interface FilesTabProps {
  repositoryId: number;
  repository: Repository;
}

const FilesTab: React.FC<FilesTabProps> = ({ repositoryId, repository }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');

  // Mock data for FilesTab - following L3_ANALYTICS spec
  const mockFiles = Array.from({ length: 234 }, (_, i) => ({
    id: `file-${i + 1}`,
    filePath: `src/components/File${i + 1}.tsx`,
    complexityScore: Math.floor(Math.random() * 100),
    churnRate: Math.floor(Math.random() * 100),
    qualityDeficit: Math.floor(Math.random() * 100),
    urgencyScore: Math.floor(Math.random() * 100),
    language: ['TypeScript', 'JavaScript', 'C#', 'Python'][Math.floor(Math.random() * 4)],
    lineCount: Math.floor(Math.random() * 1000) + 100,
    lastModified: new Date(Date.now() - Math.random() * 90 * 24 * 60 * 60 * 1000).toISOString(),
    issues: {
      security: Math.floor(Math.random() * 3),
      bugs: Math.floor(Math.random() * 5),
      codeSmells: Math.floor(Math.random() * 8)
    }
  }));

  useEffect(() => {
    // Simulate loading
    const timer = setTimeout(() => setLoading(false), 500);
    return () => clearTimeout(timer);
  }, [repositoryId]);

  const filteredFiles = mockFiles.filter(file =>
    !searchQuery || file.filePath.toLowerCase().includes(searchQuery.toLowerCase())
  ).slice(0, 50); // Show first 50 for pagination

  const getFileTypes = () => ['ts', 'tsx', 'js', 'jsx', 'cs', 'py'];

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Repository Files Analysis
      </Typography>
      <Typography variant="body2" color="text.secondary" paragraph>
        Find and sort every file by quality metrics - identify which files to prioritize
      </Typography>

      {/* Filters according to L3_ANALYTICS spec */}
      <Box display="flex" gap={2} mb={3} flexWrap="wrap" alignItems="center">
        <TextField
          size="small"
          placeholder="Search files..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search fontSize="small" />
              </InputAdornment>
            ),
          }}
          sx={{ minWidth: 200 }}
        />

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>File Type</InputLabel>
          <Select value="" label="File Type">
            <MenuItem value="">All</MenuItem>
            {getFileTypes().map(type => (
              <MenuItem key={type} value={type}>.{type}</MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Complexity ≥</InputLabel>
          <Select value="" label="Complexity ≥">
            <MenuItem value="">Any</MenuItem>
            <MenuItem value="5">≥ 5 (High)</MenuItem>
            <MenuItem value="7">≥ 7 (Very High)</MenuItem>
            <MenuItem value="9">≥ 9 (Critical)</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Coverage ≤</InputLabel>
          <Select value="" label="Coverage ≤">
            <MenuItem value="">Any</MenuItem>
            <MenuItem value="80">≤ 80%</MenuItem>
            <MenuItem value="60">≤ 60%</MenuItem>
            <MenuItem value="40">≤ 40%</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 140 }}>
          <InputLabel>Security Issues</InputLabel>
          <Select value="" label="Security Issues">
            <MenuItem value="">Any</MenuItem>
            <MenuItem value="true">Has Issues</MenuItem>
            <MenuItem value="false">No Issues</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {/* Active Filters */}
      {searchQuery && (
        <Box display="flex" gap={1} mb={2} flexWrap="wrap">
          <FilterList fontSize="small" sx={{ color: 'text.secondary', mt: 0.5 }} />
          <Chip
            size="small"
            label={`Search: ${searchQuery}`}
            onDelete={() => setSearchQuery('')}
          />
        </Box>
      )}

      {/* Results Count */}
      <Typography variant="body2" color="text.secondary" mb={2}>
        Showing 50 of {filteredFiles.length} files (default sort: debt descending)
      </Typography>

      {/* Files Table - Using QualityHotspotRow component */}
      <Box>
        {filteredFiles.map((file, index) => (
          <QualityHotspotRow
            key={file.id}
            hotspot={file}
            repositoryId={repositoryId}
            rank={index + 1}
            onViewFile={(filePath) => {
              // Navigate to file detail (L4)
              window.location.href = `/repos/${repositoryId}/files/${encodeURIComponent(filePath)}`;
            }}
          />
        ))}
      </Box>

      {/* Pagination */}
      <Box display="flex" justifyContent="center" mt={3}>
        <Typography variant="body2" color="text.secondary">
          Page 1 of {Math.ceil(234 / 50)} • 50 rows per page
        </Typography>
      </Box>
    </Box>
  );
};

export default FilesTab;
