import NotFound from "@/components/NotFound";
import Sidebar from "@/components/SideBar";
import { Stack } from "@mui/material";

const Error404:React.FC = () => {
    return (
        <Stack flexDirection='row'>
          <Sidebar />
          <NotFound />
      </Stack>);
}

export default Error404;