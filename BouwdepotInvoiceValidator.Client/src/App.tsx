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
import { WithdrawalProofUpload } from './components/WithdrawalProofUpload';
import ValidationResultAdapter from './components/ValidationResultAdapter';
import { ValidationResult as ValidationResultType } from './types/models';
import { apiService } from './services/api';

/**
 * Main application component
 */
const App: React.FC = () => {
  const [validationResult, setValidationResult] = useState<any>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Handle validation context from WithdrawalProofUpload component
   */

  /**
   * Handle validation context from WithdrawalProofUpload component
   */
  const handleValidationContextReceived = (validationContext: any) => {
    console.log('Withdrawal proof validation complete:', validationContext);
    setValidationResult(validationContext);
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
      <AppBar position="static" sx={{ bgcolor: '#ffffff', color: 'text.primary' }}> {/* Changed bgcolor to white and set text color */}
        <Toolbar>
          {/* Logo removed */}
          <Typography variant="h6" component="div" sx={{ flexGrow: 1, fontWeight: 600 }}>
            Withdrawal Validation Tool
          </Typography>
        </Toolbar>
      </AppBar>
      <Container maxWidth="lg">
        <Box sx={{ my: 4 }}>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Warning: Uploaded information will be sent to Google Gemini for analysis.
          </Alert>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Grid container spacing={3}>
            <Grid item xs={12} md={validationResult ? 4 : 12}>
              <WithdrawalProofUpload 
                onValidationComplete={handleValidationContextReceived}
                onValidationStart={handleValidationStart}
                onValidationError={handleValidationError}
              />
            </Grid>
            
            <Grid item xs={12} md={validationResult ? 8 : 12}>
              {isLoading && !validationResult && (
                <ValidationResultAdapter 
                  validationResult={null}
                  isLoading={true}
                />
              )}
              
              {validationResult && (
                <ValidationResultAdapter 
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
