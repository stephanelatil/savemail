'use client'

import { usePathname, useRouter } from "next/navigation";
import { PropsWithChildren } from "react";
import Sidebar from "./SideBar";
import { Stack } from "@mui/material";

const PageBaseWithSidebar :React.FC<PropsWithChildren> = ({children}) => {
    const path = usePathname();

    return ['/auth/login', '/auth/register'].includes(path) ?
            (<>{children}</>)
                :
            (<Stack direction={'row'}>
                <Sidebar />
                <span style={{width:'1em'}}/>
                {children}
            </Stack>);
}

export default PageBaseWithSidebar;