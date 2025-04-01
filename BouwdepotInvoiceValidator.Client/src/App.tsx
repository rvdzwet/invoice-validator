import React, { useState } from 'react';
import { ThemeProvider } from '@mui/material/styles';
import { CssBaseline, Container, Typography, Box, Grid, AppBar, Toolbar } from '@mui/material';
import theme from './theme';
import InvoiceUpload from './components/InvoiceUpload';
import ValidationResult from './components/ValidationResult';
import { ValidationResult as ValidationResultType } from './types/models';

/**
 * Main application component
 */
const App: React.FC = () => {
  const [validationResult, setValidationResult] = useState<ValidationResultType | null>(null);

  /**
   * Handle validation completion from the InvoiceUpload component
   */
  const handleValidationComplete = (result: ValidationResultType) => {
    setValidationResult(result);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AppBar position="static" color="primary">
        <Toolbar>
          <Typography variant="h6" component="div">
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

          <Grid container spacing={3} sx={{ mt: 2 }}>
            <Grid item xs={12} md={validationResult ? 5 : 12}>
              <InvoiceUpload onValidationComplete={handleValidationComplete} />
            </Grid>
            
            {validationResult && (
              <Grid item xs={12} md={7}>
                <ValidationResult result={validationResult} />
              </Grid>
            )}
          </Grid>
        </Box>
      </Container>
    </ThemeProvider>
  );
};

export default App;
