'use client'

import { Box, Button, CircularProgress, Divider, IconButton, TextField, Typography } from "@mui/material";
import { SubmitHandler, useForm } from "react-hook-form";
import { useRouter, useSearchParams } from 'next/navigation'
import { useAuthentication } from "@/hooks/useAuthentication";
import { PasswordElement, PasswordRepeatElement, TextFieldElement } from "react-hook-form-mui";
import { PasswordReset } from "@/models/credentials";
import { DarkMode, LightMode } from "@mui/icons-material";
import { useLightDarkModeSwitch } from "@/hooks/useLightDarkModeSwitch";

const ResetPasswordForm: React.FC = () => {
    const { loading, resetPassword } = useAuthentication();
    const params = useSearchParams();
    const { mode, toggleMode } = useLightDarkModeSwitch();
    const { control, handleSubmit } = useForm<PasswordReset>({defaultValues:{
                                                                  resetCode:params.get('resetCode') ?? '',
                                                                  email:params.get('email') ?? ''
                                                                }});
    const router = useRouter();

    const onSubmit: SubmitHandler<PasswordReset> = async (data) => {
      await resetPassword(data);
      router.push('/auth/login');
    }

    return (<>
              <Box
                component="form"
                onSubmit={handleSubmit(onSubmit)}
                sx={{
                  maxWidth: '400px',
                  margin: '0 auto',
                  padding: '2rem',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: '1rem',
                }}>

                  <Typography variant="h3" textAlign="center">
                    Password Reset
                  </Typography>

                  <TextFieldElement
                    label='Email'
                    name='email'
                    required
                    control={control}
                    disabled={!!params.has('email')}
                    type='email'
                    fullWidth
                  />

                  <TextFieldElement
                    name="resetCode"
                    label="Reset Code"
                    control={control}
                    required
                    disabled={!!params.has('resetCode')}
                    fullWidth
                  />

                  <PasswordElement
                    name='newPassword'
                    label="New Password"
                    control={control}
                    rules={{ minLength: 6 }}
                    required
                    fullWidth
                  />

                  <PasswordRepeatElement
                    name='newPasswordRepeat'
                    passwordFieldName="newPassword"
                    label="Confirm Password"
                    control={control}
                    required
                    fullWidth
                  />

                  <Button
                    type="submit"
                    variant="contained"
                    color="primary"
                    fullWidth
                    disabled={loading}
                    aria-busy={loading}
                    sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                  >
                    {loading ? <CircularProgress size={24} color="inherit" /> : 'Reset Password'}
                  </Button>

                  <Divider   variant='middle' sx={{my:'2rem'}}/>

                  <Typography variant='body2' textAlign="center">
                    {"Remember your password? "}
                    <Button onClick={()=>{router.push('/auth/login');}}>
                      Back to Login
                    </Button>
                  </Typography>
                </Box>
                <IconButton onClick={toggleMode} sx={{
                  position: 'fixed',
                  bottom: 16, left: 16,
                  zIndex: 1000, /* ensure it stays on top */}}>
                  {mode === 'light' ? <LightMode /> : <DarkMode />}
                </IconButton>
              </>);
};

export default ResetPasswordForm;