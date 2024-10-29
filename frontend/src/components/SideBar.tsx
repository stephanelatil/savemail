'use client'

import { styled, useTheme, Theme, CSSObject } from '@mui/material/styles';
import Box from '@mui/material/Box';
import MuiDrawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import { useState } from 'react';
import { Stack, useMediaQuery } from '@mui/material';
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
	width: `calc(${theme.spacing(6)} + 1px)`,
	[theme.breakpoints.up('sm')]: {
		width: `calc(${theme.spacing(7)} + 1px)`,
	},
});

const DrawerBlock = styled('div')(({ theme }) => ({
	display: 'contents',
	alignItems: 'center',
	justifyContent: 'space-between',
	alignSelf:'center',
	overflowY:'hidden'
}));

const StyledDrawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(
	({ theme, open }) => ({
		width: drawerWidth,
		flexShrink: 0,
		maxHeight: '100vh',
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

const Sidebar : React.FC = () => {
	const theme = useTheme();
	const [open, setOpen] = useState(false);
	
	//check for both screen size (md and up) AND landscape orientation
	//this avoids small screens being unreadable due to a large sidebar
	const isDesktop = useMediaQuery((theme:Theme) => `${theme.breakpoints.up('md')} and (orientation: landscape)`);

	// Drawer content component to avoid duplication
	const DrawerContent = () => (
		<>
			<DrawerBlock theme={theme}>
				{!isDesktop && (
					<IconButton
						color="inherit"
						aria-label={open ? "Close Drawer" : "Open Drawer"}
						onClick={() => setOpen((prev) => !prev)}
						edge="end"
						sx={{ marginRight: 0 }}
					>
						{open ? <ChevronLeftIcon /> : <MenuIcon />}
					</IconButton>
				)}
			</DrawerBlock>
			<Divider />
			<DrawerBlock theme={theme}>
				<Stack 
					display='contents' 
					alignItems='center' 
					justifyContent='space-between' 
					direction='column' 
					alignContent='center'
				>
					<MailBoxList />
					<UserCardListItem />
				</Stack>
			</DrawerBlock>
		</>
	);

	return isDesktop ? // Permanent drawer for desktop (md+ AND landscape)
				<Box sx={{ display: 'grid', height: '100%' }}>
					<StyledDrawer variant="permanent" open={true}>
						<DrawerContent />
					</StyledDrawer>
				</Box>
				:
				<Box sx={{ display: 'grid', height: '100%' }}>
					<IconButton
						color="inherit"
						aria-label="Open Drawer"
						onClick={() => setOpen(true)}
						edge="start"
						sx={{ 
							margin: 1,
							width: 'fit-content',
							display: open ? 'none' : 'flex'
						}}
					>
						<MenuIcon />
					</IconButton>
					<MuiDrawer
						variant="temporary"
						open={open}
						onClose={() => setOpen(false)}
						ModalProps={{
							keepMounted: true, // Better mobile performance
						}}
						sx={{
							'& .MuiDrawer-paper': {
								width: drawerWidth,
							},
						}}
					>
						<DrawerContent />
					</MuiDrawer>
				</Box>;
}

export default Sidebar;
