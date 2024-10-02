import Sidebar from "@/components/SideBar";
import React from 'react';
import { Box, Stack, Typography } from "@mui/material";


export default function Home() {
  return (
    <Stack flexDirection='row'>
      <Sidebar/>
      <Box alignSelf='center' alignContent='center' display='flex' flexDirection='column' sx={{
        margin: '0 auto',
        padding: '2rem'
        }}>
        <Typography variant="h6" align="center" paddingTop='2em'>
          Select, create a mailbox on the left to start fetching new emails
        </Typography>
        <Typography variant="h6" paddingTop='2em' align="center">
          Or open a mailbox folder to view saved emails
        </Typography>
      </Box>
    </Stack>
  );
}
