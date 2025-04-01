import React from 'react';
import { 
  Box, 
  Card, 
  CardContent, 
  CardHeader, 
  Chip, 
  List, 
  ListItem, 
  ListItemText, 
  Typography, 
  Divider 
} from '@mui/material';
import VerifiedIcon from '@mui/icons-material/Verified';
import ErrorIcon from '@mui/icons-material/Error';
import WarningIcon from '@mui/icons-material/Warning';
import HomeIcon from '@mui/icons-material/Home';
import { ValidationResult as ValidationResultType, ValidationSeverity } from '../types/models';
import InvoiceDetails from './InvoiceDetails';

interface ValidationResultProps {
  result: ValidationResultType;
}

/**
 * Component for displaying validation results
 */
const ValidationResult: React.FC<ValidationResultProps> = ({ result }) => {
  /**
   * Get the color for the status card based on validation result
   */
  const getHeaderColor = () => {
    if (result.isValid) {
      return 'success.main';
    }
    if (result.possibleTampering) {
      return 'warning.main';
    }
    return 'error.main';
  };

  /**
   * Get the color for issue severity
   */
  const getIssueSeverityColor = (severity: ValidationSeverity) => {
    switch (severity) {
      case ValidationSeverity.Error:
        return 'error.main';
      case ValidationSeverity.Warning:
        return 'warning.main';
      case ValidationSeverity.Info:
        return 'info.main';
      default:
        return 'text.secondary';
    }
  };

  return (
    <Card 
      elevation={3} 
      sx={{ 
        mt: { xs: 2, md: 0 },
        borderTop: 6, 
        borderColor: getHeaderColor() 
      }}
    >
      <CardHeader 
        title="Validation Result" 
        sx={{ bgcolor: getHeaderColor(), color: 'white' }}
      />
      <CardContent>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mb: 2 }}>
          {result.isValid ? (
            <Chip 
              icon={<VerifiedIcon />} 
              label="Valid" 
              color="success" 
              variant="filled" 
            />
          ) : (
            <Chip 
              icon={<ErrorIcon />} 
              label="Invalid" 
              color="error" 
              variant="filled" 
            />
          )}
          
          {result.possibleTampering && (
            <Chip 
              icon={<WarningIcon />} 
              label="Possible Tampering" 
              color="warning" 
              variant="filled" 
            />
          )}
          
          {result.isHomeImprovement ? (
            <Chip 
              icon={<HomeIcon />} 
              label="Home Improvement" 
              color="info" 
              variant="filled" 
            />
          ) : (
            <Chip 
              label="Not Home Improvement" 
              color="default" 
              variant="filled" 
            />
          )}
        </Box>
        
        {result.issues.length > 0 && (
          <>
            <Typography variant="h6" gutterBottom>
              Issues:
            </Typography>
            <List>
              {result.issues.map((issue, index) => (
                <ListItem key={index} disablePadding sx={{ py: 0.5 }}>
                  <ListItemText 
                    primary={issue.message}
                    primaryTypographyProps={{
                      color: getIssueSeverityColor(issue.severity),
                      fontWeight: issue.severity === ValidationSeverity.Error ? 'medium' : 'normal'
                    }}
                  />
                </ListItem>
              ))}
            </List>
            <Divider sx={{ my: 2 }} />
          </>
        )}
        
        {result.extractedInvoice && (
          <InvoiceDetails invoice={result.extractedInvoice} />
        )}
      </CardContent>
    </Card>
  );
};

export default ValidationResult;
