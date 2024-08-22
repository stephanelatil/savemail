'use client'

import { useAppUserData } from '@/hooks/useAppUserData';
import { useAuthentication } from '@/hooks/useAuthentication';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { DarkMode, LightMode, Logout, ManageAccounts } from '@mui/icons-material';
import { CircularProgress, List, ListItem, ListItemButton, ListItemIcon, ListItemText, Typography } from "@mui/material";
import { useRouter } from 'next/navigation';
import { useCallback, useEffect, useState } from 'react';

const UserCardListItem :React.FC = () => {
    const { mode, toggleMode } = useLightDarkModeSwitch();
    const { getCurrentlyLoggedInUser } = useAppUserData();
    const { logout } = useAuthentication();
    const router = useRouter();
    const [username, setUsername] = useState(<CircularProgress key='USERNAME_LOADING'/>)

    useEffect(() =>{
        async function populateUserNameOrEmail() {
            const {email, firstName, lastName} = await getCurrentlyLoggedInUser() ?? {email:"ERROR", firstName:null, lastName:null};
            let name = `${firstName??''} ${lastName??''}`.trim();
            name = name.length > 1 ? name : email;
            setUsername(<Typography key='USERNAME'>{ name }</Typography>);
        }
        if (username.key === 'USERNAME_LOADING')
            populateUserNameOrEmail();
    },[]);

    function onLogout(){
        const doLogout = async ()=>{
            await logout();
            router.push('/auth/login');
        }
        doLogout();
    }

    return (
        <List sx={{ bottom:0, flexShrink: 0 }}>
            <ListItem sx={{alignSelf:'center', px:0.5}}>
                <ListItemButton href={'/me'}>
                    <ListItemIcon >
                        <ManageAccounts />
                    </ListItemIcon>
                    <ListItemText primary={ username } />
                </ListItemButton>
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
                <ListItemButton onClick={onLogout}>
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