'use client'

import { useAuthentication } from '@/hooks/useAuthentication';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { AppUser } from '@/models/appUser';
import { DarkMode, LightMode, Logout, ManageAccounts } from '@mui/icons-material';
import { CircularProgress, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography } from "@mui/material";
import { usePathname, useRouter } from 'next/navigation';

const UserCardListItem: React.FC<{loading?:boolean, user?:AppUser|null, onLogout?:()=>void}> = ({onLogout, loading, user}) => {
    const { mode, toggleMode } = useLightDarkModeSwitch();
    const { logout } = useAuthentication();
    const pathname = usePathname();
    const router = useRouter();

    function onLogoutCallback(){
        const doLogout = async ()=>{
            await logout();
            onLogout && onLogout();
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
                <ListItemButton onClick={onLogoutCallback} disabled={!!loading}>
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