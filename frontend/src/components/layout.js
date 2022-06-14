import Header from './header'
import Copyright from './copyright'
import Container from '@mui/material/Container';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';

export default function Layout({ children }) {
  return (
    <>
      <Container maxWidth="lg" sx={{
        marginTop: { xs: 0, md: 4 },
        padding: 2
      }}>
        <Paper>
          <Box>
            <Header/>
            <Box
            sx={{
                marginTop: 1,
                padding: 2
            }}
            >
            {children}
            </Box>
          </Box>
        </Paper>
      </Container>
      <Copyright />
    </>
  )
}