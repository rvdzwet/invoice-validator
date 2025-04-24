import React from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Chip,
  Divider,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  LinearProgress,
  Alert,
  Stack
} from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import GavelIcon from '@mui/icons-material/Gavel';
import FactCheckIcon from '@mui/icons-material/FactCheck';
import AssignmentTurnedInIcon from '@mui/icons-material/AssignmentTurnedIn';
import ArticleIcon from '@mui/icons-material/Article';
import ScheduleIcon from '@mui/icons-material/Schedule';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import InfoIcon from '@mui/icons-material/Info';
import RuleIcon from '@mui/icons-material/Rule';
import BalanceIcon from '@mui/icons-material/Balance';
import MemoryIcon from '@mui/icons-material/Memory';
import LabelImportantIcon from '@mui/icons-material/LabelImportant';
import { AuditReport, RuleAssessment, RegulationReference, ProcessingEvent } from '../../types/models';

interface AuditReportPanelProps {
  auditReport: AuditReport;
}

const AuditReportPanel: React.FC<AuditReportPanelProps> = ({ auditReport }) => {
  // Helper to format DateTime to readable string
  const formatDateTime = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleString('nl-NL', {
        year: 'numeric',
        month: 'long',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      });
    } catch (e) {
      return dateString;
    }
  };

  // Sort processing events by timestamp
  const sortedEvents = React.useMemo(() => {
    if (!auditReport.processingEvents) return [];
    
    return [...auditReport.processingEvents].sort((a, b) => {
      const dateA = new Date(a.timestamp).getTime();
      const dateB = new Date(b.timestamp).getTime();
      return dateA - dateB;
    });
  }, [auditReport.processingEvents]);

  return (
    <Box sx={{ mt: 2 }}>
      {/* Audit Header */}
      <Paper 
        elevation={3} 
        sx={{ 
          p: 2, 
          mb: 3, 
          backgroundColor: 'secondary.main',
          color: 'white'
        }}
      >
        <Grid container spacing={2} alignItems="center">
          <Grid item>
            <GavelIcon fontSize="large" />
          </Grid>
          <Grid item xs>
            <Typography variant="h6">
              Audit Report #{auditReport.auditIdentifier?.substring(0, 8) || 'N/A'}
            </Typography>
            <Typography variant="body2">
              Generated on {formatDateTime(auditReport.timestampUtc)} • 
              Model v{auditReport.modelVersion} • 
              Validator v{auditReport.validatorVersion}
            </Typography>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Executive Summary */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <ArticleIcon color="secondary" sx={{ mr: 1 }} />
            <Typography variant="h6" color="secondary">
              Executive Summary
            </Typography>
          </Box>
          <Typography variant="body1" paragraph>
            {auditReport.executiveSummary}
          </Typography>
          
          {/* Key factors */}
          <Grid container spacing={2} sx={{ mt: 1 }}>
            {/* Approval Factors */}
            {auditReport.approvalFactors && auditReport.approvalFactors.length > 0 && (
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" color="success.main" gutterBottom>
                  Approval Factors:
                </Typography>
                <List dense disablePadding>
                  {auditReport.approvalFactors.map((factor, idx) => (
                    <ListItem key={idx} dense disableGutters>
                      <ListItemIcon sx={{ minWidth: '32px' }}>
                        <CheckCircleIcon color="success" fontSize="small" />
                      </ListItemIcon>
                      <ListItemText primary={factor} />
                    </ListItem>
                  ))}
                </List>
              </Grid>
            )}
            
            {/* Concern Factors */}
            {auditReport.concernFactors && auditReport.concernFactors.length > 0 && (
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" color="error.main" gutterBottom>
                  Concern Factors:
                </Typography>
                <List dense disablePadding>
                  {auditReport.concernFactors.map((factor, idx) => (
                    <ListItem key={idx} dense disableGutters>
                      <ListItemIcon sx={{ minWidth: '32px' }}>
                        <CancelIcon color="error" fontSize="small" />
                      </ListItemIcon>
                      <ListItemText primary={factor} />
                    </ListItem>
                  ))}
                </List>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>
      
      {/* Rule Assessments */}
      <Accordion defaultExpanded sx={{ mb: 2 }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <RuleIcon color="secondary" sx={{ mr: 1 }} />
            <Typography variant="h6" color="secondary">
              Rule Assessments
            </Typography>
          </Box>
        </AccordionSummary>
        <AccordionDetails>
          {auditReport.ruleAssessments && auditReport.ruleAssessments.length > 0 ? (
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead sx={{ bgcolor: 'secondary.light' }}>
                  <TableRow>
                    <TableCell>Rule</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="center">Score</TableCell>
                    <TableCell>Reasoning</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {auditReport.ruleAssessments.map((rule, index) => (
                    <TableRow key={index} hover>
                      <TableCell component="th" scope="row">
                        <Box>
                          <Typography variant="body2" fontWeight="medium">
                            {rule.ruleName}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {rule.description}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell align="center">
                        {rule.isSatisfied ? (
                          <CheckCircleIcon color="success" />
                        ) : (
                          <CancelIcon color="error" />
                        )}
                      </TableCell>
                      <TableCell align="center">
                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                          <Box sx={{ width: 40, mr: 1 }}>
                            <LinearProgress 
                              variant="determinate" 
                              value={rule.score} 
                              color={rule.score >= 70 ? "success" : 
                                    rule.score >= 40 ? "warning" : "error"} 
                            />
                          </Box>
                          <Typography variant="body2">
                            {rule.score}%
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {rule.reasoning}
                        </Typography>
                        {rule.evidence && (
                          <Typography variant="caption" color="text.secondary" display="block">
                            Evidence: {rule.evidence}
                          </Typography>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <Alert severity="info">No rule assessments available</Alert>
          )}
        </AccordionDetails>
      </Accordion>
      
      {/* Applied Thresholds */}
      {auditReport.appliedThresholds && (
        <Card variant="outlined" sx={{ mb: 3 }}>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <LabelImportantIcon color="secondary" sx={{ mr: 1 }} />
              <Typography variant="h6" color="secondary">
                Applied Thresholds
              </Typography>
            </Box>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  Auto-Approval Threshold:
                </Typography>
                <Typography variant="body1">
                  {auditReport.appliedThresholds.autoApprovalThreshold}%
                </Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">
                  High Risk Threshold:
                </Typography>
                <Typography variant="body1">
                  {auditReport.appliedThresholds.highRiskThreshold}%
                </Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="text.secondary">
                  Auto-Approval Status:
                </Typography>
                <Typography variant="body1">
                  {auditReport.appliedThresholds.enableAutoApproval ? 'Enabled' : 'Disabled'}
                </Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}
      
      {/* Regulations */}
      {auditReport.applicableRegulations && auditReport.applicableRegulations.length > 0 && (
        <Accordion sx={{ mb: 2 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <BalanceIcon color="secondary" sx={{ mr: 1 }} />
              <Typography variant="h6" color="secondary">
                Applicable Regulations
              </Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead sx={{ bgcolor: 'secondary.light' }}>
                  <TableRow>
                    <TableCell>Reference</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Compliance Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {auditReport.applicableRegulations.map((regulation, index) => (
                    <TableRow key={index} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {regulation.referenceCode}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {regulation.description}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Applicability: {regulation.applicabilityExplanation}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip 
                          label={regulation.complianceStatus}
                          color={
                            regulation.complianceStatus.toLowerCase().includes('compliant') ? 
                              "success" : 
                            regulation.complianceStatus.toLowerCase().includes('partial') ? 
                              "warning" : 
                              "error"
                          }
                          size="small"
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </AccordionDetails>
        </Accordion>
      )}
      
      {/* Processing Timeline */}
      {sortedEvents.length > 0 && (
        <Accordion sx={{ mb: 2 }}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <ScheduleIcon color="secondary" sx={{ mr: 1 }} />
              <Typography variant="h6" color="secondary">
                Processing Timeline
              </Typography>
            </Box>
          </AccordionSummary>
          <AccordionDetails>
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead sx={{ bgcolor: 'secondary.light' }}>
                  <TableRow>
                    <TableCell>Timestamp</TableCell>
                    <TableCell>Event Type</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Component</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {sortedEvents.map((event, index) => (
                    <TableRow key={index} hover>
                      <TableCell>
                        <Typography variant="body2">
                          {new Date(event.timestamp).toLocaleTimeString()}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center' }}>
                          <MemoryIcon fontSize="small" color="secondary" sx={{ mr: 1 }} />
                          <Typography variant="body2">{event.eventType}</Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{event.description}</Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{event.component}</Typography>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </AccordionDetails>
        </Accordion>
      )}
      
      {/* Document Integrity */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <FactCheckIcon color="secondary" sx={{ mr: 1 }} />
            <Typography variant="h6" color="secondary">
              Document Integrity
            </Typography>
          </Box>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2" color="text.secondary">
                Document Source:
              </Typography>
              <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                {auditReport.documentSource}
              </Typography>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Typography variant="subtitle2" color="text.secondary">
                Document Hash:
              </Typography>
              <Typography variant="body2" sx={{ wordBreak: 'break-all' }}>
                {auditReport.documentHash}
              </Typography>
            </Grid>
          </Grid>
          
          {/* Verification Tests */}
          {auditReport.verificationTests && auditReport.verificationTests.length > 0 && (
            <Box sx={{ mt: 2 }}>
              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Verification Tests Performed:
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {auditReport.verificationTests.map((test, idx) => (
                  <Chip 
                    key={idx}
                    label={test}
                    size="small"
                    color="secondary"
                    variant="outlined"
                  />
                ))}
              </Box>
            </Box>
          )}
        </CardContent>
      </Card>
      
      {/* Key Metrics */}
      {auditReport.keyMetrics && Object.keys(auditReport.keyMetrics).length > 0 && (
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <AssignmentTurnedInIcon color="secondary" sx={{ mr: 1 }} />
              <Typography variant="h6" color="secondary">
                Key Metrics
              </Typography>
            </Box>
            <Grid container spacing={2}>
              {Object.entries(auditReport.keyMetrics).map(([key, value], idx) => (
                <Grid item xs={6} sm={4} md={3} key={idx}>
                  <Typography variant="subtitle2" color="text.secondary">
                    {key.replace(/([A-Z])/g, ' $1')
                        .replace(/^./, (str) => str.toUpperCase())}:
                  </Typography>
                  <Typography variant="body1">
                    {typeof value === 'number' && 
                     (key.toLowerCase().includes('percentage') || 
                      key.toLowerCase().includes('score') || 
                      key.toLowerCase().includes('confidence')) ? 
                      `${value.toFixed(2)}%` : value.toString()}
                  </Typography>
                </Grid>
              ))}
            </Grid>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default AuditReportPanel;
