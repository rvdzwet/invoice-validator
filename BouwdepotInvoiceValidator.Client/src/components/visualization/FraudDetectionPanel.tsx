import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Chip,
  Divider,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  LinearProgress,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Card,
  CardContent,
  Avatar,
  Alert
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import SecurityIcon from '@mui/icons-material/Security';
import DocumentScannerIcon from '@mui/icons-material/DocumentScanner';
import DescriptionIcon from '@mui/icons-material/Description';
import { FraudDetection, FraudRiskLevel, FraudIndicator, FraudIndicatorCategory } from '../../types/models';

interface FraudDetectionPanelProps {
  fraudDetection: FraudDetection;
}

const FraudDetectionPanel: React.FC<FraudDetectionPanelProps> = ({ fraudDetection }) => {
  // Helper functions for formatting and display
  const getRiskLevelColor = (riskLevel: FraudRiskLevel) => {
    switch (riskLevel) {
      case FraudRiskLevel.Critical:
        return 'error.main';
      case FraudRiskLevel.High:
        return 'error.light';
      case FraudRiskLevel.Medium:
        return 'warning.main';
      case FraudRiskLevel.Low:
      default:
        return 'success.main';
    }
  };

  const getRiskLevelLabel = (riskLevel: FraudRiskLevel) => {
    switch (riskLevel) {
      case FraudRiskLevel.Critical:
        return 'Critical Risk';
      case FraudRiskLevel.High:
        return 'High Risk';
      case FraudRiskLevel.Medium:
        return 'Medium Risk';
      case FraudRiskLevel.Low:
      default:
        return 'Low Risk';
    }
  };

  const getRiskIcon = (riskLevel: FraudRiskLevel) => {
    switch (riskLevel) {
      case FraudRiskLevel.Critical:
      case FraudRiskLevel.High:
        return <ErrorIcon />;
      case FraudRiskLevel.Medium:
        return <WarningIcon />;
      case FraudRiskLevel.Low:
      default:
        return <CheckCircleIcon />;
    }
  };

  const getCategoryIcon = (category: FraudIndicatorCategory) => {
    switch (category) {
      case FraudIndicatorCategory.DocumentManipulation:
        return <DocumentScannerIcon />;
      case FraudIndicatorCategory.ContentInconsistency:
        return <DescriptionIcon />;
      default:
        return <WarningIcon />;
    }
  };

  const getCategoryLabel = (category: FraudIndicatorCategory) => {
    switch (category) {
      case FraudIndicatorCategory.DocumentManipulation:
        return 'Document Manipulation';
      case FraudIndicatorCategory.ContentInconsistency:
        return 'Content Inconsistency';
      case FraudIndicatorCategory.AnomalousPricing:
        return 'Anomalous Pricing';
      case FraudIndicatorCategory.VendorIssue:
        return 'Vendor Issue';
      case FraudIndicatorCategory.HistoricalPattern:
        return 'Historical Pattern';
      case FraudIndicatorCategory.DigitalArtifact:
        return 'Digital Artifact';
      case FraudIndicatorCategory.ContextualMismatch:
        return 'Contextual Mismatch';
      case FraudIndicatorCategory.BehavioralFlag:
        return 'Behavioral Flag';
      default:
        return 'Unknown Category';
    }
  };

  // Group indicators by category for better organization
  const groupedIndicators = React.useMemo(() => {
    const grouped: Record<FraudIndicatorCategory, FraudIndicator[]> = {} as Record<FraudIndicatorCategory, FraudIndicator[]>;
    
    fraudDetection.detectedIndicators.forEach(indicator => {
      if (!grouped[indicator.category]) {
        grouped[indicator.category] = [];
      }
      grouped[indicator.category].push(indicator);
    });
    
    return grouped;
  }, [fraudDetection.detectedIndicators]);

  // No indicators case
  if (fraudDetection.detectedIndicators.length === 0) {
    return (
      <Alert severity="success" sx={{ mt: 2 }}>
        No fraud indicators were detected in this invoice.
      </Alert>
    );
  }

  return (
    <Box sx={{ mt: 2 }}>
      {/* Risk Score Header */}
      <Paper 
        elevation={3} 
        sx={{ 
          p: 2, 
          mb: 3, 
          backgroundColor: getRiskLevelColor(fraudDetection.riskLevel),
          color: 'white'
        }}
      >
        <Grid container alignItems="center" spacing={2}>
          <Grid item>
            <Avatar sx={{ bgcolor: 'rgba(255, 255, 255, 0.2)', color: 'white' }}>
              <SecurityIcon />
            </Avatar>
          </Grid>
          <Grid item xs>
            <Typography variant="h6">
              Fraud Risk Assessment: {getRiskLevelLabel(fraudDetection.riskLevel)}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
              <Box sx={{ width: '100%', mr: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={fraudDetection.fraudRiskScore} 
                  sx={{ 
                    height: 10, 
                    borderRadius: 5,
                    backgroundColor: 'rgba(255, 255, 255, 0.3)',
                    '& .MuiLinearProgress-bar': {
                      backgroundColor: 'rgba(255, 255, 255, 0.8)'
                    }
                  }}
                />
              </Box>
              <Typography variant="body2" color="inherit">
                {fraudDetection.fraudRiskScore}%
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Recommended Action */}
      {fraudDetection.recommendedAction && (
        <Alert 
          severity={fraudDetection.riskLevel <= FraudRiskLevel.Low ? "success" : 
                 fraudDetection.riskLevel <= FraudRiskLevel.Medium ? "warning" : "error"}
          sx={{ mb: 3 }}
        >
          <Typography variant="subtitle1" gutterBottom>
            Recommended Action
          </Typography>
          <Typography variant="body2">
            {fraudDetection.recommendedAction}
          </Typography>
        </Alert>
      )}

      {/* Fraud Indicators by Category */}
      <Typography variant="h6" gutterBottom>
        Detected Indicators
      </Typography>
      
      {Object.entries(groupedIndicators).map(([categoryKey, indicators]) => {
        const category = Number(categoryKey) as FraudIndicatorCategory;
        return (
          <Accordion key={categoryKey} sx={{ mb: 2 }}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Box sx={{ display: 'flex', alignItems: 'center' }}>
                {getCategoryIcon(category)}
                <Typography variant="subtitle1" sx={{ ml: 1 }}>
                  {getCategoryLabel(category)} ({indicators.length})
                </Typography>
              </Box>
            </AccordionSummary>
            <AccordionDetails>
              <Grid container spacing={2}>
                {indicators.map((indicator, index) => (
                  <Grid item xs={12} key={index}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                          <Typography variant="subtitle1" fontWeight="medium">
                            {indicator.indicatorName}
                          </Typography>
                          <Chip 
                            label={`${Math.round(indicator.severity * 100)}% Severity`}
                            color={indicator.severity > 0.7 ? "error" : 
                                  indicator.severity > 0.4 ? "warning" : "success"}
                            size="small"
                          />
                        </Box>
                        <Typography variant="body2" color="textSecondary" paragraph>
                          {indicator.description}
                        </Typography>
                        <Typography variant="subtitle2" gutterBottom>
                          Evidence:
                        </Typography>
                        <Paper variant="outlined" sx={{ p: 1, bgcolor: 'background.default' }}>
                          <Typography variant="body2">
                            {indicator.evidence}
                          </Typography>
                        </Paper>
                        
                        {indicator.affectedElements && indicator.affectedElements.length > 0 && (
                          <Box sx={{ mt: 2 }}>
                            <Typography variant="subtitle2" gutterBottom>
                              Affected Elements:
                            </Typography>
                            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                              {indicator.affectedElements.map((element, i) => (
                                <Chip key={i} label={element} size="small" />
                              ))}
                            </Box>
                          </Box>
                        )}
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            </AccordionDetails>
          </Accordion>
        );
      })}

      {/* Verification Steps */}
      {fraudDetection.suggestedVerificationSteps && fraudDetection.suggestedVerificationSteps.length > 0 && (
        <>
          <Divider sx={{ my: 3 }} />
          <Typography variant="h6" gutterBottom>
            Suggested Verification Steps
          </Typography>
          <List>
            {fraudDetection.suggestedVerificationSteps.map((step, index) => (
              <ListItem key={index}>
                <ListItemIcon>
                  <SecurityIcon color="primary" />
                </ListItemIcon>
                <ListItemText primary={step} />
              </ListItem>
            ))}
          </List>
        </>
      )}
    </Box>
  );
};

export default FraudDetectionPanel;
