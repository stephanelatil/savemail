'use client'

import { styled, useTheme, Theme, CSSObject } from '@mui/material/styles';
import Box from '@mui/material/Box';
import MuiDrawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import { useState } from 'react';
import { Stack } from '@mui/material';
import UserCardListItem from '@/components/UserCard';
import MailBoxList from './MailBoxList';

const drawerWidth = 300;

const openedMixin = (theme: Theme): CSSObject => ({
  width: drawerWidth,
  transition: theme.transitions.create('width', {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.enteringScreen,
  }),
  overflowX: 'hidden',
});

const closedMixin = (theme: Theme): CSSObject => ({
  transition: theme.transitions.create('width', {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  overflowX: 'hidden',
  width: `calc(${theme.spacing(7)} + 1px)`,
  [theme.breakpoints.up('sm')]: {
    width: `calc(${theme.spacing(8)} + 1px)`,
  },
});

const DrawerBlock = styled('div')(({ theme }) => ({
  display: 'contents',
  alignItems: 'center',
  justifyContent: 'space-between',
  alignSelf:'center'
}));

const Drawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(
  ({ theme, open }) => ({
    width: drawerWidth,
    flexShrink: 0,
    whiteSpace: 'nowrap',
    boxSizing: 'border-box',
    ...(open && {
      ...openedMixin(theme),
      '& .MuiDrawer-paper': openedMixin(theme),
    }),
    ...(!open && {
      ...closedMixin(theme),
      '& .MuiDrawer-paper': closedMixin(theme),
    }),
  }),
);

const Sidebar :React.FC = () => {
  const theme = useTheme();
  const [open, setOpen] = useState(false);

  return (
    <Box sx={{ display: 'grid' }}>
      <Drawer variant="permanent" open={open}>
        <DrawerBlock theme={theme}>
          <IconButton
              color="inherit"
              aria-label={open ? "Close Drawer" : "Open Drawer"}
              onClick={() => setOpen((prev) => !prev)}
              edge="end"
              sx={{ marginRight: 0 }}>
            {open ? <ChevronLeftIcon /> : <MenuIcon />}
          </IconButton>
        </DrawerBlock>
        <Divider />
        <DrawerBlock theme={theme}>
          <Stack display='contents' alignItems='center' height='100vh' useFlexGap justifyContent='space-between' direction='column' alignContent='center'>
            <MailBoxList />
            <UserCardListItem />
          </Stack>
        </DrawerBlock>
      </Drawer>
    </Box>
  );
}

export default Sidebar