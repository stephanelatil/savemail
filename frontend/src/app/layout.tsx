import '@/app/global.css'
import '@/app/tailwind.css'
import NotificationSnackbar from '@/components/NotificationSnackbar'
import type { Metadata } from "next";

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
      <body>
        {children}
        <NotificationSnackbar />
      </body>
    </html>
  )
}
