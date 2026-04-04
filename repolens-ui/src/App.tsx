import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, Box, Typography } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

// Import RepoLens Design System
import repoLensTheme from './theme/design-system';

// Components
import MainLayout from './components/layout/MainLayout';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import L1PortfolioDashboard from './components/portfolio/L1PortfolioDashboard';
import L2RepositoryDashboard from './components/repository/L2RepositoryDashboard';
import L3Analytics from './components/analytics/L3Analytics';
import L3UniversalSearch from './components/search/L3UniversalSearch';
import L3CodeGraph from './components/codegraph/L3CodeGraph';
import L4FileDetail from './components/files/L4FileDetail';
import Dashboard from './components/dashboard/Dashboard';
import Repositories from './components/repositories/Repositories';
import RepositoryDetails from './components/repositories/RepositoryDetails';
import Search from './components/search/Search';
import Analytics from './components/analytics/Analytics';
import CodeGraphVisualization from './components/codegraph/CodeGraphVisualization';
import NaturalLanguageSearch from './components/search/NaturalLanguageSearch';
import { WinFormsModernizationDashboard } from './components/winforms/WinFormsModernizationDashboard';

// Services
import apiService from './services/apiService';

// Use RepoLens Design System theme
// Replaces old Material-UI theme with RepoLens specification-compliant styling

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

// Protected Route wrapper with enhanced debugging
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isAuthenticated = apiService.isAuthenticated();
  const token = localStorage.getItem('repolens_token');
  
  // Debug logging for authentication checks
  console.log('[ProtectedRoute] Authentication check:', {
    isAuthenticated,
    hasToken: !!token,
    tokenLength: token?.length,
    currentPath: window.location.pathname
  });
  
  if (!isAuthenticated) {
    console.log('[ProtectedRoute] Not authenticated, redirecting to login');
    return <Navigate to="/login" replace />;
  }
  
  console.log('[ProtectedRoute] Authenticated, rendering protected content');
  return <>{children}</>;
};

// Public Route wrapper (redirect to dashboard if already authenticated)
const PublicRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isAuthenticated = apiService.isAuthenticated();
  
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }
  
  return <>{children}</>;
};

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={repoLensTheme}>
        <CssBaseline />
        <Router>
          <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <Routes>
              {/* Public routes */}
              <Route
                path="/login"
                element={
                  <PublicRoute>
                    <Login />
                  </PublicRoute>
                }
              />
              <Route
                path="/register"
                element={
                  <PublicRoute>
                    <Register />
                  </PublicRoute>
                }
              />

              {/* Demo route (no authentication required) */}
              <Route
                path="/demo/codegraph"
                element={
                  <Box sx={{ p: 3 }}>
                    <Box sx={{ mb: 3 }}>
                      <Typography variant="h4" gutterBottom>
                        Code Graph Visualization Demo
                      </Typography>
                      <Typography variant="body1" color="text.secondary" gutterBottom>
                        Interactive code architecture visualization with clickable drill-down functionality
                      </Typography>
                    </Box>
                    <CodeGraphVisualization repositoryId={1} />
                  </Box>
                }
              />

              {/* Demo Search Route */}
              <Route
                path="/demo/search"
                element={
                  <Box sx={{ p: 3 }}>
                    <Box sx={{ mb: 3 }}>
                      <Typography variant="h4" gutterBottom>
                        Natural Language Code Search Demo
                      </Typography>
                      <Typography variant="body1" color="text.secondary" gutterBottom>
                        Ask questions about your code in plain English - find functions, classes, patterns and more
                      </Typography>
                    </Box>
                    <NaturalLanguageSearch repositoryId={1} />
                  </Box>
                }
              />

              {/* Protected routes */}
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <MainLayout />
                  </ProtectedRoute>
                }
              >
                {/* Portfolio Dashboard - L1 Engineering Manager Focus */}
                <Route index element={<L1PortfolioDashboard />} />
                <Route path="portfolio" element={<Navigate to="/" replace />} />
                
                {/* Legacy Dashboard - L3 Technical Details */}
                <Route path="dashboard" element={<Dashboard />} />

                {/* Repositories - L2 Repository Dashboard */}
                <Route path="repositories" element={<Repositories />} />
                <Route path="repositories/:repositoryId" element={<L2RepositoryDashboard />} />
                <Route path="repositories/:id" element={<RepositoryDetails />} />

                {/* Repository-specific L2 and L3 Routes */}
                <Route path="repos/:repoId" element={<L2RepositoryDashboard />} />
                <Route path="repos/:repoId/analytics" element={<L3Analytics />} />
                <Route path="repos/:repoId/analytics/:tab" element={<L3Analytics />} />
                <Route path="repos/:repoId/search" element={<L3UniversalSearch />} />
                <Route path="repos/:repoId/graph" element={<L3CodeGraph />} />
                <Route path="repos/:repoId/files/:fileId" element={<L4FileDetail />} />

                {/* Global Search & Discovery */}
                <Route path="search" element={<L3UniversalSearch />} />
                <Route path="search/:tab" element={<L3UniversalSearch />} />

                {/* Legacy Analytics */}
                <Route path="analytics" element={<Analytics />} />

                {/* WinForms Modernization */}
                <Route path="winforms" element={<WinFormsModernizationDashboard />} />

                {/* Settings & Admin */}
                <Route path="settings" element={<div>Settings (Coming Soon)</div>} />
                <Route path="admin" element={<div>Admin (Coming Soon)</div>} />
              </Route>

              {/* Catch all route */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Box>
        </Router>
      </ThemeProvider>
    </QueryClientProvider>
  );
}

export default App;
