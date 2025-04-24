import React, { useState } from 'react';
import { ThemeProvider } from '@mui/material/styles';
import { 
  CssBaseline, 
  Container, 
  Typography, 
  Box, 
  Grid, 
  AppBar, 
  Toolbar,
  Alert
} from '@mui/material';
import theme from './theme';
import InvoiceUpload from './components/InvoiceUpload';
import ComprehensiveValidationView from './components/ComprehensiveValidationView';
import { ValidationResult as ValidationResultType } from './types/models';
import { apiService } from './services/api';

/**
 * Main application component
 */
const App: React.FC = () => {
  const [validationResult, setValidationResult] = useState<ValidationResultType | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Handle validation completion from the InvoiceUpload component
   */
  const handleValidationComplete = async (response: {
    validationResult: ValidationResultType;
    auditReport: any;
  }) => {
    setValidationResult(response.validationResult);
    setIsLoading(false);
    setError(null);
  };

  /**
   * Handle validation in progress
   */
  const handleValidationStart = () => {
    setIsLoading(true);
    setError(null);
  };

  /**
   * Handle validation error
   */
  const handleValidationError = (errorMessage: string) => {
    setError(errorMessage);
    setIsLoading(false);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AppBar position="static" sx={{ bgcolor: '#562178' }}>
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 600 }}>
            Bouwdepot Invoice Validator
          </Typography>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg">
        <Box sx={{ my: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Invoice Validation Tool
          </Typography>
          <Typography variant="subtitle1" color="text.secondary" gutterBottom>
            Upload an invoice PDF to validate it for home improvement expenses
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Grid container spacing={3}>
            <Grid item xs={12} md={validationResult ? 4 : 12}>
              <InvoiceUpload 
                onValidationComplete={handleValidationComplete}
                onValidationStart={handleValidationStart}
                onValidationError={handleValidationError}
              />
            </Grid>
            
            <Grid item xs={12} md={validationResult ? 8 : 12}>
              {isLoading && !validationResult && (
                <ComprehensiveValidationView 
                  validationResult={null as any}
                  isLoading={true}
                />
              )}
              
              {validationResult && (
                <ComprehensiveValidationView 
                  validationResult={validationResult}
                  isLoading={isLoading}
                />
              )}
            </Grid>
          </Grid>
        </Box>
      </Container>
    </ThemeProvider>
  );
};

export default App;
