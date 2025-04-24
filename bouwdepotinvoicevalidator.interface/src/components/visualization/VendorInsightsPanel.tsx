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
  LinearProgress,
  Stack,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Alert
} from '@mui/material';
import BusinessIcon from '@mui/icons-material/Business';
import StarIcon from '@mui/icons-material/Star';
import StarHalfIcon from '@mui/icons-material/StarHalf';
import StarOutlineIcon from '@mui/icons-material/StarOutline';
import LocalOfferIcon from '@mui/icons-material/LocalOffer';
import PriceCheckIcon from '@mui/icons-material/PriceCheck';
import HistoryIcon from '@mui/icons-material/History';
import WarningIcon from '@mui/icons-material/Warning';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import ConstructionIcon from '@mui/icons-material/Construction';
import { VendorInsights } from '../../types/models';

interface VendorInsightsPanelProps {
  vendorInsights: VendorInsights;
}

const VendorInsightsPanel: React.FC<VendorInsightsPanelProps> = ({ vendorInsights }) => {
  // Format date to be more readable
  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('nl-NL', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric'
      });
    } catch (e) {
      return dateString;
    }
  };
  
  // Convert score (0-1) to star display
  const renderStarRating = (score: number) => {
    // Normalize score to 0-5 stars
    const normalizedScore = Math.round(score * 5);
    const fullStars = Math.floor(normalizedScore);
    const halfStar = normalizedScore % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);
    
    return (
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        {[...Array(fullStars)].map((_, i) => (
          <StarIcon key={`full-${i}`} color="primary" />
        ))}
        {halfStar && <StarHalfIcon color="primary" />}
        {[...Array(emptyStars)].map((_, i) => (
          <StarOutlineIcon key={`empty-${i}`} color="action" />
        ))}
        <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
          ({Math.round(score * 100)}%)
        </Typography>
      </Box>
    );
  };
  
  // Get color for score
  const getScoreColor = (score: number) => {
    if (score >= 0.7) return 'success.main';
    if (score >= 0.5) return 'warning.main';
    return 'error.main';
  };

  return (
    <Box sx={{ mt: 2 }}>
      {/* Vendor Header */}
      <Paper 
        elevation={3} 
        sx={{ 
          p: 2, 
          mb: 3, 
          backgroundColor: 'primary.main',
          color: 'white'
        }}
      >
        <Grid container alignItems="center" spacing={2}>
          <Grid item>
            <BusinessIcon fontSize="large" />
          </Grid>
          <Grid item xs>
            <Typography variant="h6">
              {vendorInsights.vendorName}
            </Typography>
            <Typography variant="body2">
              Invoice History: {vendorInsights.invoiceCount} invoices since {formatDate(vendorInsights.firstSeen)}
            </Typography>
          </Grid>
        </Grid>
      </Paper>
      
      {/* Vendor Metrics */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} md={4}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="subtitle1" color="primary" gutterBottom>
                Reliability
              </Typography>
              {renderStarRating(vendorInsights.reliabilityScore)}
              <Box sx={{ mt: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={vendorInsights.reliabilityScore * 100} 
                  color={vendorInsights.reliabilityScore >= 0.7 ? "success" : 
                        vendorInsights.reliabilityScore >= 0.5 ? "warning" : "error"} 
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="subtitle1" color="primary" gutterBottom>
                Price Stability
              </Typography>
              {renderStarRating(vendorInsights.priceStabilityScore)}
              <Box sx={{ mt: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={vendorInsights.priceStabilityScore * 100} 
                  color={vendorInsights.priceStabilityScore >= 0.7 ? "success" : 
                        vendorInsights.priceStabilityScore >= 0.5 ? "warning" : "error"} 
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={4}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="subtitle1" color="primary" gutterBottom>
                Document Quality
              </Typography>
              {renderStarRating(vendorInsights.documentQualityScore)}
              <Box sx={{ mt: 1 }}>
                <LinearProgress 
                  variant="determinate" 
                  value={vendorInsights.documentQualityScore * 100} 
                  color={vendorInsights.documentQualityScore >= 0.7 ? "success" : 
                        vendorInsights.documentQualityScore >= 0.5 ? "warning" : "error"} 
                  sx={{ height: 8, borderRadius: 4 }}
                />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Warning Alerts */}
      {(vendorInsights.unusualServicesDetected || 
        vendorInsights.unreasonablePricesDetected || 
        vendorInsights.totalAnomalyCount > 0) && (
        <Alert 
          severity="warning" 
          icon={<WarningIcon />}
          sx={{ mb: 3 }}
        >
          <Typography variant="subtitle1" gutterBottom>
            Vendor Anomalies Detected
          </Typography>
          <List dense disablePadding>
            {vendorInsights.unusualServicesDetected && (
              <ListItem disablePadding>
                <ListItemIcon sx={{ minWidth: '36px' }}>
                  <ConstructionIcon fontSize="small" color="warning" />
                </ListItemIcon>
                <ListItemText 
                  primary="Unusual services detected for this vendor's typical business profile"
                />
              </ListItem>
            )}
            {vendorInsights.unreasonablePricesDetected && (
              <ListItem disablePadding>
                <ListItemIcon sx={{ minWidth: '36px' }}>
                  <AttachMoneyIcon fontSize="small" color="warning" />
                </ListItemIcon>
                <ListItemText 
                  primary="Prices significantly differ from expected market rates"
                />
              </ListItem>
            )}
            {vendorInsights.totalAnomalyCount > 0 && (
              <ListItem disablePadding>
                <ListItemIcon sx={{ minWidth: '36px' }}>
                  <WarningIcon fontSize="small" color="warning" />
                </ListItemIcon>
                <ListItemText 
                  primary={`${vendorInsights.totalAnomalyCount} anomalies detected in vendor history`}
                />
              </ListItem>
            )}
          </List>
        </Alert>
      )}
      
      {/* Business Categories and Specialties */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <LocalOfferIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6" color="primary">
                  Business Categories
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {vendorInsights.businessCategories.length > 0 ? (
                  vendorInsights.businessCategories.map((category, index) => (
                    <Chip 
                      key={index}
                      label={category}
                      color="primary"
                      variant="outlined"
                      size="medium"
                      sx={{ mb: 1 }}
                    />
                  ))
                ) : (
                  <Typography variant="body2" color="text.secondary">
                    No business categories available
                  </Typography>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <ConstructionIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6" color="primary">
                  Specialties
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {vendorInsights.vendorSpecialties.length > 0 ? (
                  vendorInsights.vendorSpecialties.map((specialty, index) => (
                    <Chip 
                      key={index}
                      label={specialty}
                      color="secondary"
                      variant="outlined"
                      size="medium"
                      sx={{ mb: 1 }}
                    />
                  ))
                ) : (
                  <Typography variant="body2" color="text.secondary">
                    No specialties information available
                  </Typography>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* History Timeline */}
      <Card variant="outlined" sx={{ mt: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
            <HistoryIcon color="primary" sx={{ mr: 1 }} />
            <Typography variant="h6" color="primary">
              Vendor History
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', px: 2 }}>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="subtitle2" color="text.secondary">
                First Seen
              </Typography>
              <Typography variant="body1">
                {formatDate(vendorInsights.firstSeen)}
              </Typography>
            </Box>
            
            <Box sx={{ flex: 1, mx: 2 }}>
              <Divider>
                <Chip 
                  label={`${vendorInsights.invoiceCount} Invoices`}
                  color="primary"
                  size="small"
                />
              </Divider>
            </Box>
            
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="subtitle2" color="text.secondary">
                Last Seen
              </Typography>
              <Typography variant="body1">
                {formatDate(vendorInsights.lastSeen)}
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default VendorInsightsPanel;
