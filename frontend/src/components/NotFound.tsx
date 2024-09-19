import { SearchOff } from '@mui/icons-material';
import { Box, Container, Typography } from '@mui/material';


const NotFound:React.FC<{objectName?:string}> = ({objectName}) => {
  return (
    <Container maxWidth="md" sx={{ textAlign: 'center', mt: 10 }}>
      <Box sx={{ mb: 5 }}>
        <Typography variant="h2" component="h1" sx={{ fontSize: '6rem', fontWeight: 'bold', color: 'primary.main' }}>
          404
        </Typography>
        <Typography variant="h4" component="h2" sx={{ mb: 2 }}>
          { objectName ?? 'Page' } {' '} Not Found
        </Typography>
        <Typography variant="body1" color="textSecondary" sx={{ mb: 4 }}>
          The {' '} { objectName?.toLocaleLowerCase() ?? 'Page' } {' '} you are looking for might have been removed, had its name changed, or is temporarily unavailable.
        </Typography>
      </Box>
      <Box sx={{ mt: 5 }}>
        <SearchOff titleAccess='404' fontSize='large' />
        {/* <img 
          src="/static/404-illustration.svg" // Replace with your illustration or image path
          alt="404 Illustration"
          style={{ maxWidth: '100%', height: 'auto' }}
        /> */}
      </Box>
    </Container>
  );
}

export default NotFound;