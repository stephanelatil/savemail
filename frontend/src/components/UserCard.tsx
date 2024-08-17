'use client'

import { useAppUserData } from '@/hooks/useAppUserData';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { DarkMode, LightMode, ManageAccounts } from '@mui/icons-material';
import { CircularProgress, Container, IconButton, Typography } from "@mui/material";
import { Suspense, useEffect, useMemo, useState } from 'react';

const UserCard :React.FC = () => {
    const { mode, toggleMode } = useLightDarkModeSwitch();
    const { getCurrentlyLoggedInUser } = useAppUserData();
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

    return (
        <Container >
            <IconButton href={'/settings'}>
                <ManageAccounts />
            </IconButton>
            { username }
            <IconButton onClick={toggleMode}>
                {mode === 'light' ? <LightMode /> : <DarkMode />}
            </IconButton>
        </Container>
    );
}

export default UserCard;