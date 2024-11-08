'use client'

import { styled, useTheme, Theme, CSSObject } from '@mui/material/styles';
import Box from '@mui/material/Box';
import MuiDrawer from '@mui/material/Drawer';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import { Dispatch, SetStateAction, Suspense, useState } from 'react';
import { Skeleton, Stack, useMediaQuery } from '@mui/material';
import UserCardListItem from '@/components/UserCard';
import MailBoxList from './MailBoxList';
import { Refresh } from '@mui/icons-material';
import useSWR from 'swr';
import { useMailboxes } from '@/hooks/useMailboxes';
import { useAppUserData } from '@/hooks/useAppUserData';

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

// Drawer content component to avoid duplication
const DrawerContent: React.FC<{
	theme:Theme, isDesktop?:boolean, open:boolean, setOpen:Dispatch<SetStateAction<boolean>>}
					> = ({theme, isDesktop, open, setOpen}) => {
    const { getMailboxList } = useMailboxes();
    const { getCurrentlyLoggedInUser } = useAppUserData();

    const {mutate:mutateUser, data:user, isLoading:loadingUser} = useSWR('/api/AppUser/me',
                                                    getCurrentlyLoggedInUser,
                                                    { fallbackData:null });
    const {mutate: mutateMb, data:mailboxes, isLoading:loadingMb} = useSWR('/api/MailBox',
                                                                getMailboxList,
                                                                { fallbackData:[] });

	function onRefresh() {
		async function refresh(){
			await mutateMb();
			await mutateUser();
		}
		refresh();
	};

	return (<>
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
						<MailBoxList loading={loadingMb} mailboxes={mailboxes}/>
						<IconButton onClick={onRefresh} disabled={!!loadingMb}>
								<Refresh/>
						</IconButton>
						<UserCardListItem   user={user} loading={loadingUser}
											onLogout={() => {mutateUser(null, {
												rollbackOnError:false,
												revalidate:true,
												throwOnError:false,
												populateCache:true
											});}}
						/>
					</Stack>
				</DrawerBlock>
			</>);
}

const SidebarBase: React.FC<{isDesktop?:boolean}> = ({isDesktop}) => {
	const theme:Theme = useTheme();
	const [open, setOpen] = useState(false);

	//TODO study why sidebar color is different for desktop & mobile
	return !!isDesktop ? // Permanent drawer for desktop (md+ AND landscape)
					<StyledDrawer variant="permanent" open={true}>
						<DrawerContent theme={theme} open={open} setOpen={setOpen} isDesktop={isDesktop}/>
					</StyledDrawer>

				:
				<div>
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
								width: drawerWidth
							},
						}}
					>
						<DrawerContent theme={theme} open={open} setOpen={setOpen} isDesktop={isDesktop}/>
					</MuiDrawer>
				</div>
};

const DrawerContentSkeleton:React.FC<{isDesktop?:boolean, open:boolean, setOpen:Dispatch<SetStateAction<boolean>>}> = ({isDesktop, open, setOpen}) => (
	<Box sx={{ 
	  display: 'flex', 
	  flexDirection: 'column', 
	  height: '100%', 
	  p: 2,
	  gap: 2 
	}}>
		{!isDesktop && (
			<><IconButton
				color="inherit"
				aria-label={open ? "Close Drawer" : "Open Drawer"}
				onClick={() => setOpen((prev) => !prev)}
				edge="end"
				sx={{ marginRight: 0 }}
			>
				{open ? <ChevronLeftIcon /> : <MenuIcon />}
			</IconButton>
			<Divider/>
			</>
		)}
	  
	  {/* Mailbox list skeletons */}
	  <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 1 }}>
		{[...Array(5)].map((_, i) => (
		  <Skeleton key={i} variant="rectangular" height={48} sx={{ borderRadius: 1 }} />
		))}
	  </Box>
	  
	  {/* Refresh button skeleton */}
	  <Skeleton variant="circular" width={40} height={40} sx={{ alignSelf: 'center' }} />
	  
	  {/* User card skeleton */}
	  <Skeleton 
		variant="rounded" 
		height={60}
	  />
	  {/* TODO add logout (skeleton) and theme change button */}
	  <Skeleton 
		variant="rounded" 
		height={60}
	  />
	</Box>
  );
  
  const SidebarSkeleton: React.FC<{isDesktop?: boolean}> = ({isDesktop}) => {
	const [open, setOpen] = useState(false);
  
	return !!isDesktop ? (
	  // Desktop version
		<StyledDrawer variant="permanent" open={true}>
			<DrawerContentSkeleton open={open} setOpen={setOpen} isDesktop={isDesktop}/>
		</StyledDrawer>) : (
	  // Mobile version
	<div>
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
			<DrawerContentSkeleton open={open} setOpen={setOpen} isDesktop={isDesktop}/>
		</MuiDrawer>
	  </div>
	);
};

const Sidebar: React.FC = () => {
	//check for both screen size (md and up) AND landscape orientation
	//this avoids small screens being unreadable due to a large sidebar
	const isDesktop = useMediaQuery((theme:Theme) => `${theme.breakpoints.up('md')} and (orientation: landscape)`);

	return (<Suspense fallback={<SidebarSkeleton isDesktop={isDesktop}/>}>
				<SidebarBase isDesktop={isDesktop}/>
			</Suspense>);
};

export default Sidebar;
