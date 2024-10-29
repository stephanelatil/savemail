'use client'

import { useAppUserData } from '@/hooks/useAppUserData';
import { useAuthentication } from '@/hooks/useAuthentication';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { DarkMode, LightMode, Logout, ManageAccounts } from '@mui/icons-material';
import { CircularProgress, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography } from "@mui/material";
import { usePathname, useRouter } from 'next/navigation';
import useSWR from 'swr';

const UserCardListItem :React.FC = () => {
    const { mode, toggleMode } = useLightDarkModeSwitch();
    const { getCurrentlyLoggedInUser } = useAppUserData();
    const { logout } = useAuthentication();
    const pathname = usePathname();
    const router = useRouter();

    const {mutate, data:user, isLoading:loading} = useSWR('/api/AppUser/me',
                                                    getCurrentlyLoggedInUser,
                                                    { fallbackData:null });

    function onLogout(){
        const doLogout = async ()=>{
            await logout();
            mutate(null, {
                rollbackOnError:false,
                revalidate:true,
                throwOnError:false,
                populateCache:true
            });
            router.push('/auth/login');
        }
        doLogout();
    }

    return (
        <List sx={{ bottom:0, flexShrink: 0 }}>
            <ListItem sx={{alignSelf:'center', px:0.5}}>
                {!loading ? 
                    <ListItemButton 
                        selected={pathname == "/me"}
                        onClick={() => {pathname != "/me" && router.push('/me');}}
                        >
                        <ListItemIcon >
                            <ManageAccounts />
                        </ListItemIcon>
                        <ListItemText primary={ `${user?.firstName??''} ${user?.lastName??''}`.trim().length > 1?
                                                    `${user?.firstName??''} ${user?.lastName??''}`.trim()
                                                    :user?.email
                        } />
                    </ListItemButton>
                    : <CircularProgress/>}
            </ListItem>
            <ListItem sx={{alignSelf:'center', px:0.5}}>
                <ListItemButton onClick={toggleMode}>
                    <ListItemIcon >
                            {mode === 'light' ? <LightMode /> : <DarkMode />}
                    </ListItemIcon>
                    <ListItemText primary={ mode === 'light' ? 'Light Mode' : 'Dark mode' } />
                </ListItemButton>
            </ListItem>
            <ListItem sx={{alignSelf:'center', px:0.5}}>
                <ListItemButton onClick={onLogout} disabled={!!loading}>
                    <ListItemIcon>
                        <Logout />
                    </ListItemIcon>
                    <ListItemText primary='Logout'/>
                </ListItemButton>
            </ListItem>
        </List>
    );
}

export default UserCardListItem;