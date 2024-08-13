'use client'

import { USER_SETTINGS_PAGE } from '@/constants/NavRoutes';
import { useAppUserData } from '@/hooks/useAppUserData';
import { useLightDarkModeSwitch } from '@/hooks/useLightDarkModeSwitch';
import { DarkMode, LightMode, ManageAccounts } from '@mui/icons-material';
import { CircularProgress, Container, IconButton, Typography } from "@mui/material";
import { Suspense } from 'react';

const UserNameOrEmail:React.FC = () => {
    const { getCurrentlyLoggedInUser } = useAppUserData();

    const getName = async() =>
        {
            const {email, firstName, lastName} = await getCurrentlyLoggedInUser() ?? {email:"ERROR", firstName:null, lastName:null};
            const name = `${firstName??''} ${lastName??''}`.trim();
            return name.length > 1 ? name : email;
        };
    
    return (
        <Typography noWrap gutterBottom variant="h6">
            { getName() }
        </Typography>);
}

const UserCard :React.FC = () => {
    const { mode, toggleMode } = useLightDarkModeSwitch();


    return (
        <Container >

            <IconButton href={USER_SETTINGS_PAGE}>
                <ManageAccounts />
            </IconButton>
            <Suspense fallback={<CircularProgress />}>
                <UserNameOrEmail/>
            </Suspense>
            <IconButton onClick={toggleMode}>
                {mode === 'light' ? <LightMode /> : <DarkMode />}
            </IconButton>
        </Container>
    );
}

export default UserCard;