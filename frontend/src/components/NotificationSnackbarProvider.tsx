'use client'

import React, { PropsWithChildren, useCallback, useContext, useState } from 'react';
import { Alert, AlertColor, IconButton, Snackbar } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { NotificationContext } from './context/NotificationContext';

const NotificationSnackbar : React.FC<PropsWithChildren> = ({children}) => {

  const [notification, setNotification] = useState<{message:string, severity:AlertColor, isShown:boolean}>({
    message: '',
    severity: 'info',
    isShown: false,
  });

  const showNotification = useCallback((message:string='', severity:AlertColor = 'info') => {
    setNotification({ message, severity, isShown: true });
  }, []);

  const handleClose = useCallback(()=>setNotification({ message:'', severity:'info', isShown: false }),[]);

    const action = (
          <IconButton
            size="small"
            aria-label="close"
            color="inherit"
            onClick={handleClose} 
          >
            <CloseIcon fontSize="small"/>
          </IconButton>
      );
    
  return (
    <NotificationContext.Provider value={showNotification}>
      {children}
      <Snackbar
        open={notification.isShown}
        autoHideDuration={4000}
        onClose={handleClose}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        action={action}
      >
        <Alert onClose={handleClose} severity={notification.severity} sx={{ width: {
            xs: '100%', // 100% width for extra-small screens
            sm: '25em', // 20em width for small screens and up
            xl: '40em', // 20em width for small screens and up
          } }}>
          {notification.message}
        </Alert>
      </Snackbar>
    </NotificationContext.Provider>
  );
};

export default NotificationSnackbar;