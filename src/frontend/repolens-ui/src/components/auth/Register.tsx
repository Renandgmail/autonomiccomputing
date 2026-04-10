import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  Paper,
  Divider,
  CircularProgress
} from '@mui/material';
import { Lock, Person, Email } from '@mui/icons-material';

import { RegisterRequest } from '../../types/api';
import apiService from '../../services/apiService';

interface RegisterFormData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

const Register: React.FC = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors }
  } = useForm<RegisterFormData>();

  const password = watch('password');

  const onSubmit = async (data: RegisterFormData) => {
    console.log('[RepoLens Register] 🚀 Registration form submitted', { 
      email: data.email, 
      firstName: data.firstName, 
      lastName: data.lastName 
    });
    
    setIsLoading(true);
    setError(null);

    try {
      const registerRequest: RegisterRequest = {
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword
      };

      console.log('[RepoLens Register] 📤 Sending registration request to API', { 
        email: data.email 
      });

      const response = await apiService.register(registerRequest);

      console.log('[RepoLens Register] 📥 Received registration response', { 
        success: response.success,
        hasToken: !!response.token,
        hasUser: !!response.user,
        errorMessage: response.errorMessage
      });

      if (response.success) {
        console.log('[RepoLens Register] ✅ Registration successful, navigating to dashboard');
        alert('Registration successful! Welcome to RepoLens.');
        navigate('/dashboard');
      } else {
        const errorMsg = response.errorMessage || 'Registration failed';
        console.error('[RepoLens Register] ❌ Registration failed:', errorMsg);
        alert(`Registration failed: ${errorMsg}`);
        setError(errorMsg);
      }
    } catch (err) {
      console.error('[RepoLens Register] 💥 Registration error caught:', err);
      const errorMsg = 'An unexpected error occurred. Please try again.';
      alert(`Error: ${errorMsg}`);
      setError(errorMsg);
    } finally {
      setIsLoading(false);
      console.log('[RepoLens Register] 🏁 Registration process completed');
    }
  };

  return (
    <Container component="main" maxWidth="sm">
      <Box
        sx={{
          marginTop: 8,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        {/* Logo and Title */}
        <Paper
          elevation={0}
          sx={{
            display: 'flex',
            alignItems: 'center',
            mb: 4,
            p: 2,
            backgroundColor: 'transparent'
          }}
        >
          <Lock sx={{ fontSize: 40, color: 'primary.main', mr: 2 }} />
          <Typography component="h1" variant="h4" color="primary">
            RepoLens
          </Typography>
        </Paper>

        {/* Register Card */}
        <Card sx={{ width: '100%', maxWidth: 500 }}>
          <CardContent sx={{ p: 4 }}>
            <Box sx={{ textAlign: 'center', mb: 3 }}>
              <Typography component="h2" variant="h5" gutterBottom>
                Create Account
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Join RepoLens for powerful repository analytics
              </Typography>
            </Box>

            {error && (
              <Alert severity="error" sx={{ mb: 3 }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 1 }}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <TextField
                    {...register('firstName', {
                      required: 'First name is required'
                    })}
                    fullWidth
                    id="firstName"
                    label="First Name"
                    name="firstName"
                    autoComplete="given-name"
                    autoFocus
                    error={!!errors.firstName}
                    helperText={errors.firstName?.message}
                    InputProps={{
                      startAdornment: <Person sx={{ color: 'action.active', mr: 1 }} />
                    }}
                  />
                  <TextField
                    {...register('lastName', {
                      required: 'Last name is required'
                    })}
                    fullWidth
                    id="lastName"
                    label="Last Name"
                    name="lastName"
                    autoComplete="family-name"
                    error={!!errors.lastName}
                    helperText={errors.lastName?.message}
                    InputProps={{
                      startAdornment: <Person sx={{ color: 'action.active', mr: 1 }} />
                    }}
                  />
                </Box>
                <TextField
                  {...register('email', {
                    required: 'Email is required',
                    pattern: {
                      value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                      message: 'Invalid email address'
                    }
                  })}
                  fullWidth
                  id="email"
                  label="Email Address"
                  name="email"
                  autoComplete="email"
                  error={!!errors.email}
                  helperText={errors.email?.message}
                  InputProps={{
                    startAdornment: <Email sx={{ color: 'action.active', mr: 1 }} />
                  }}
                />
                <TextField
                  {...register('password', {
                    required: 'Password is required',
                    minLength: {
                      value: 8,
                      message: 'Password must be at least 8 characters'
                    }
                  })}
                  fullWidth
                  name="password"
                  label="Password"
                  type="password"
                  id="password"
                  autoComplete="new-password"
                  error={!!errors.password}
                  helperText={errors.password?.message}
                  InputProps={{
                    startAdornment: <Lock sx={{ color: 'action.active', mr: 1 }} />
                  }}
                />
                <TextField
                  {...register('confirmPassword', {
                    required: 'Please confirm your password',
                    validate: (value) =>
                      value === password || 'Passwords do not match'
                  })}
                  fullWidth
                  name="confirmPassword"
                  label="Confirm Password"
                  type="password"
                  id="confirmPassword"
                  autoComplete="new-password"
                  error={!!errors.confirmPassword}
                  helperText={errors.confirmPassword?.message}
                  InputProps={{
                    startAdornment: <Lock sx={{ color: 'action.active', mr: 1 }} />
                  }}
                />
              </Box>

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2, py: 1.5 }}
                disabled={isLoading}
              >
                {isLoading ? (
                  <CircularProgress size={24} color="inherit" />
                ) : (
                  'Create Account'
                )}
              </Button>

              <Divider sx={{ my: 2 }} />

              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  Already have an account?{' '}
                  <Link
                    to="/login"
                    style={{
                      color: '#1976d2',
                      textDecoration: 'none',
                      fontWeight: 600
                    }}
                  >
                    Sign in here
                  </Link>
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>

        {/* Footer */}
        <Typography variant="body2" color="text.secondary" align="center" sx={{ mt: 4 }}>
          © 2026 RepoLens. Autonomous Repository Analytics.
        </Typography>
      </Box>
    </Container>
  );
};

export default Register;
