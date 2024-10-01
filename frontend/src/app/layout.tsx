import './globals.css'
import './tailwind.css'
import type { Metadata } from "next";
import LightDarkMode from '@/components/LightDarkMode';
import NotificationSnackbar from '@/components/NotificationSnackbarProvider';
import { AppRouterCacheProvider } from '@mui/material-nextjs/v14-appRouter'
import PageBaseWithSidebar from '@/components/PageBase';

export const metadata: Metadata = {
  title: 'SaveMail',
  description: 'Saving you emails, one at a time',
  publisher:'Stephane Latil',
  applicationName:'SaveMail',
  keywords:['mail', 'email', 'archive'],
  creator:'Stephane Latil'
}

export default function RootLayout({children}: {children: React.ReactNode}) {
  return (
    <html lang="en">
      <meta name="format-detection" content="telephone=no, date=no, email=no, address=no" />
        <body>
          <AppRouterCacheProvider>
            <LightDarkMode>
              <NotificationSnackbar>
                  {children}
              </NotificationSnackbar>
            </LightDarkMode>
          </AppRouterCacheProvider>
        </body>
    </html>
  )
}
