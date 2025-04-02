import { createTheme } from '@mui/material/styles';

// Stater colors - based on provided website
const staterColors = {
  primary: '#562178', // Deep purple from Stater
  secondary: '#6c757d',
  success: '#28a745',
  warning: '#ffc107',
  error: '#dc3545',
  info: '#17a2b8',
  lightPurple: '#f4ebfa',
  background: '#ffffff'
};

// Create a theme instance
const theme = createTheme({
  palette: {
    primary: {
      main: staterColors.primary,
    },
    secondary: {
      main: staterColors.secondary,
    },
    error: {
      main: staterColors.error,
    },
    warning: {
      main: staterColors.warning,
    },
    info: {
      main: staterColors.info,
    },
    success: {
      main: staterColors.success,
    },
    background: {
      default: '#f8f9fa',
      paper: staterColors.background,
    },
  },
  typography: {
    fontFamily: [
      'Inter',
      'Roboto',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif'
    ].join(','),
    h4: {
      fontWeight: 600,
      color: staterColors.primary,
    },
    h6: {
      fontWeight: 600,
      color: staterColors.primary,
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: 'none',
          fontWeight: 500,
        },
        contained: {
          boxShadow: 'none',
        }
      }
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        }
      }
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        }
      }
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 4,
        }
      }
    },
    MuiAlert: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        }
      }
    }
  }
});

export default theme;
