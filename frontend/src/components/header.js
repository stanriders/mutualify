
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Avatar from '@mui/material/Avatar';
import Button from '@mui/material/Button';
import { useContext } from 'react';
import UserContext from '../context/userContext';
import useAuth from '../hooks/useAuth';

export default function Header({title}) {

    const { user } = useContext(UserContext)
    const { logout } = useAuth()

    return (
      <>
        <Paper elevation={2} sx={{
            marginTop: 1,
            padding: 1.5
          }}>
          <Toolbar>
            <Typography
                component="h1"
                variant="h4"
                color="inherit"
                noWrap
                sx={{ flexGrow: 1 }}
            >
              {title}
            </Typography>

            {user && (
              <Button 
                size="large"
                variant="outlined" 
                onClick={logout}
                color="secondary"
                startIcon={<Avatar alt={user.username} src={`https://s.ppy.sh/a/${user.id}`} />}>
                    {user.username}
              </Button>
            ) || (
              <Button 
                size="large"
                variant="outlined" 
                href="/api/oauth/auth"
                color="secondary"
                startIcon={<Avatar alt="a" src={`https://s.ppy.sh/a/-1`} />}>
                    Login
              </Button>
            )}
          </Toolbar>
        </Paper>
      </>
    )
  }