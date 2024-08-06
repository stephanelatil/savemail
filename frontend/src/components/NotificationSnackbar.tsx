'use client'

import React from 'react';
import useSnackbar from '@/hooks/useSnackBar';
import { Alert, AlertColor, IconButton, Snackbar } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

interface DefaultNotification{
  defaultMessage?:string,
  defaultSeverity?:AlertColor
}

const NotificationSnackbar : React.FC<DefaultNotification> = ({defaultMessage, defaultSeverity}) => {
    let { snackbar:{isOpen, message, severity}, hideSnackbar } = useSnackbar();

    message = defaultMessage || '';
    severity = defaultSeverity || 'info'


    const action = (
          <IconButton
            size="small"
            aria-label="close"
            color="inherit"
            onClick={hideSnackbar}
          >
            <CloseIcon fontSize="small"/>
          </IconButton>
      );
  return (
    <Snackbar
      open={isOpen}
      autoHideDuration={5000}
      onClose={hideSnackbar}
      anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      action={action}
    >
      <Alert onClose={hideSnackbar} severity={severity} sx={{ width: '100%' }}>
        {message}
      </Alert>
    </Snackbar>
  );
};

export default NotificationSnackbar;