import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Divider,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Card,
  CardContent,
  Collapse,
  IconButton,
  Button,
  styled,
  useTheme,
  Theme
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import InfoIcon from '@mui/icons-material/Info';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import GavelIcon from '@mui/icons-material/Gavel';
import AssessmentIcon from '@mui/icons-material/Assessment';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';

// Types for the decision steps
interface DecisionFactor {
  name: string;
  impact: number; // -100 to 100, negative means against, positive means for
  confidence: number; // 0-100
  description: string;
  details?: string;
}

interface DecisionStep {
  title: string;
  description: string;
  factors: DecisionFactor[];
  conclusion: string;
  confidence: number;
  icon?: React.ReactNode;
}

interface DecisionPathwayProps {
  steps: DecisionStep[];
  finalDecision: {
    approved: boolean;
    confidence: number;
    reason: string;
  };
  title?: string;
}

// Styled components
const ExpandMore = styled((props: {
  expand: boolean;
  onClick: () => void;
  'aria-expanded': boolean;
  'aria-label': string;
  size?: 'small' | 'medium' | 'large';
  children?: React.ReactNode;
}) => {
  const { expand, ...other } = props;
  return <IconButton {...other} />;
})(({ theme, expand }) => ({
  transform: !expand ? 'rotate(0deg)' : 'rotate(180deg)',
  marginLeft: 'auto',
  transition: theme.transitions.create('transform', {
    duration: theme.transitions.duration.shortest,
  }),
}));

// Helper to get impact color
const getImpactColor = (impact: number, theme: Theme) => {
  if (impact > 50) return theme.palette.success.main;
  if (impact > 0) return theme.palette.success.light;
  if (impact === 0) return theme.palette.grey[500];
  if (impact > -50) return theme.palette.error.light;
  return theme.palette.error.main;
};

// Helper to get confidence color
const getConfidenceColor = (confidence: number, theme: Theme) => {
  if (confidence > 80) return theme.palette.success.main;
  if (confidence > 60) return theme.palette.info.main;
  if (confidence > 40) return theme.palette.warning.main;
  return theme.palette.error.main;
};

// Helper to get default step icon
const getStepIcon = (index: number) => {
  switch (index) {
    case 0:
      return <AssessmentIcon />;
    case 1:
      return <GavelIcon />;
    case 2:
      return <AccountBalanceIcon />;
    default:
      return <InfoIcon />;
  }
};

/**
 * Component that visualizes the AI's decision-making pathway
 * for an invoice validation decision
 */
const DecisionPathway: React.FC<DecisionPathwayProps> = ({
  steps,
  finalDecision,
  title = "AI Decision Pathway"
}) => {
  const theme = useTheme();
  const [activeStep, setActiveStep] = useState<number>(steps.length);
  const [expandedFactors, setExpandedFactors] = useState<Record<string, boolean>>({});

  // Handle step click
  const handleStepClick = (step: number) => {
    setActiveStep(step === activeStep ? steps.length : step);
  };

  // Toggle factor expansion
  const handleToggleFactor = (stepIndex: number, factorIndex: number) => {
    const key = `${stepIndex}-${factorIndex}`;
    setExpandedFactors(prev => ({
      ...prev,
      [key]: !prev[key]
    }));
  };

  // Get factor impact label
  const getImpactLabel = (impact: number) => {
    if (impact > 75) return 'Strongly Supports';
    if (impact > 25) return 'Supports';
    if (impact > -25) return 'Neutral';
    if (impact > -75) return 'Contradicts';
    return 'Strongly Contradicts';
  };

  // Render confidence indicator
  const renderConfidence = (confidence: number) => {
    return (
      <Box sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        color: getConfidenceColor(confidence, theme) 
      }}>
        <Typography variant="caption" sx={{ mr: 1 }}>
          Confidence: {confidence}%
        </Typography>
        {confidence > 80 ? <CheckCircleIcon fontSize="small" /> : 
         confidence > 60 ? <InfoIcon fontSize="small" /> : 
         confidence > 40 ? <WarningIcon fontSize="small" /> : 
         <ErrorIcon fontSize="small" />}
      </Box>
    );
  };

  // Render impact indicator
  const renderImpact = (impact: number) => {
    return (
      <Box sx={{ 
        display: 'flex',
        alignItems: 'center',
        color: getImpactColor(impact, theme) 
      }}>
        <Typography variant="body2" fontWeight="medium">
          {getImpactLabel(impact)}
        </Typography>
      </Box>
    );
  };

  // Render the decision factors for a step
  const renderDecisionFactors = (step: DecisionStep, stepIndex: number) => {
    return (
      <Box sx={{ mt: 2 }}>
        <Typography variant="subtitle2" gutterBottom>
          Decision Factors:
        </Typography>
        
        {step.factors.map((factor, factorIndex) => {
          const factorKey = `${stepIndex}-${factorIndex}`;
          const isExpanded = expandedFactors[factorKey];
          
          return (
            <Card 
              key={factorKey} 
              variant="outlined" 
              sx={{ 
                mb: 1, 
                borderLeft: 4, 
                borderColor: getImpactColor(factor.impact, theme) 
              }}
            >
              <CardContent sx={{ py: 1, px: 2, '&:last-child': { pb: 1 } }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2" fontWeight="medium">
                    {factor.name}
                  </Typography>
                  
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    {renderImpact(factor.impact)}
                    
                    <ExpandMore
                      expand={isExpanded}
                      onClick={() => handleToggleFactor(stepIndex, factorIndex)}
                      aria-expanded={isExpanded}
                      aria-label="show more"
                      size="small"
                    >
                      <ExpandMoreIcon fontSize="small" />
                    </ExpandMore>
                  </Box>
                </Box>
                
                <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="body2" paragraph sx={{ mt: 1 }}>
                      {factor.description}
                    </Typography>
                    
                    {factor.details && (
                      <Typography variant="caption" color="text.secondary" paragraph>
                        {factor.details}
                      </Typography>
                    )}
                    
                    {renderConfidence(factor.confidence)}
                  </Box>
                </Collapse>
              </CardContent>
            </Card>
          );
        })}
      </Box>
    );
  };

  // Render the final decision card
  const renderFinalDecision = () => {
    const borderColor = finalDecision.approved ? theme.palette.success.main : theme.palette.error.main;
    const icon = finalDecision.approved ? <CheckCircleIcon color="success" /> : <ErrorIcon color="error" />;
    const decision = finalDecision.approved ? "APPROVED" : "REJECTED";
    
    return (
      <Card 
        sx={{ 
          mt: 2, 
          mb: 2, 
          borderTop: 4, 
          borderColor,
          backgroundColor: finalDecision.approved ? 'success.50' : 'error.50'
        }}
      >
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
            <Box sx={{ mr: 1 }}>
              {icon}
            </Box>
            <Typography variant="h6">
              Final Decision: {decision}
            </Typography>
          </Box>
          
          <Typography variant="body1" paragraph>
            {finalDecision.reason}
          </Typography>
          
          {renderConfidence(finalDecision.confidence)}
        </CardContent>
      </Card>
    );
  };

  return (
    <Box>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Typography variant="h5" gutterBottom>
          {title}
        </Typography>
        
        <Divider sx={{ mb: 2 }} />
        
        <Stepper orientation="vertical" activeStep={activeStep} nonLinear>
          {steps.map((step, index) => (
            <Step key={index} expanded={activeStep === index}>
              <StepLabel 
                StepIconComponent={() => (
                  <Box sx={{ mr: 1 }}>
                    {step.icon || getStepIcon(index)}
                  </Box>
                )}
                onClick={() => handleStepClick(index)}
                sx={{ cursor: 'pointer' }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Typography variant="subtitle1">
                    {step.title}
                  </Typography>
                  {renderConfidence(step.confidence)}
                </Box>
              </StepLabel>
              
              <StepContent>
                <Typography variant="body2" sx={{ mt: 1, mb: 2 }}>
                  {step.description}
                </Typography>
                
                {renderDecisionFactors(step, index)}
                
                <Box sx={{ mt: 2 }}>
                  <Typography variant="subtitle2">
                    Conclusion:
                  </Typography>
                  <Typography variant="body2" sx={{ mt: 0.5 }}>
                    {step.conclusion}
                  </Typography>
                </Box>
              </StepContent>
            </Step>
          ))}
        </Stepper>
        
        {renderFinalDecision()}
      </Paper>
    </Box>
  );
};

export default DecisionPathway;
