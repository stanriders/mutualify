import { useContext, useState } from 'react';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Avatar from '@mui/material/Avatar';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuIcon from '@mui/icons-material/Menu';
import Container from '@mui/material/Container';
import Tooltip from '@mui/material/Tooltip';
import MenuItem from '@mui/material/MenuItem';
import UserContext from '../context/userContext';
import useAuth from '../hooks/useAuth';
import Image from 'next/image';

export default function Header({title}) {
    const { user } = useContext(UserContext)
    const { logout } = useAuth()

    const [anchorElNav, setAnchorElNav] = useState(null);
    const [anchorElUser, setAnchorElUser] = useState(null);
  
    const handleOpenNavMenu = (event) => {
      setAnchorElNav(event.currentTarget);
    };
    const handleOpenUserMenu = (event) => {
      setAnchorElUser(event.currentTarget);
    };
  
    const handleCloseNavMenu = () => {
      setAnchorElNav(null);
    };
  
    const handleCloseUserMenu = () => {
      setAnchorElUser(null);
    };
    
    return (
      <>
        <Paper elevation={2} sx={{
            marginTop: 1,
            padding: 1.5
          }}>

          <Container sx={{px: 0}}>
            <Toolbar disableGutters>

              {/* Full size logo */}
              <Box sx={{ flexGrow: 0, display: { xs: 'none', md: 'flex' } }}>
                <Image src="/logo.svg" width={40} height={40}/>
              </Box>
              <Typography
                variant="h6"
                noWrap
                component="a"
                href="/"
                sx={{
                  ml: 2,
                  mr: 2,
                  display: { xs: 'none', md: 'flex' },
                  fontFamily: 'monospace',
                  fontWeight: 700,
                  letterSpacing: '.2rem',
                  color: 'inherit',
                  textDecoration: 'none',
                }}
              >
                 Mutualify
              </Typography>

              {/* Small size menu */}
              <Box sx={{ flexGrow: 1, display: { xs: 'flex', md: 'none' } }}>
                <IconButton
                  size="large"
                  aria-label="account of current user"
                  aria-controls="menu-appbar"
                  aria-haspopup="true"
                  onClick={handleOpenNavMenu}
                  color="inherit"
                >
                  <MenuIcon />
                </IconButton>
                <Menu
                  id="menu-appbar"
                  anchorEl={anchorElNav}
                  anchorOrigin={{
                    vertical: 'bottom',
                    horizontal: 'left',
                  }}
                  keepMounted
                  transformOrigin={{
                    vertical: 'top',
                    horizontal: 'left',
                  }}
                  open={Boolean(anchorElNav)}
                  onClose={handleCloseNavMenu}
                  sx={{
                    display: { xs: 'block', md: 'none' },
                  }}
                >
                    <MenuItem component="a" href="/friends">
                      <Typography textAlign="center">Friends</Typography>
                    </MenuItem>
                    <MenuItem component="a" href="/followers">
                      <Typography textAlign="center">Followers</Typography>
                    </MenuItem>
                </Menu>
              </Box>

              {/* Small size logo */}
              <Typography
                variant="h6"
                noWrap
                component="a"
                href="/"
                sx={{
                  mr: 2,
                  display: { xs: 'flex', md: 'none' },
                  flexGrow: 1,
                  fontFamily: 'monospace',
                  fontWeight: 700,
                  letterSpacing: '.2rem',
                  color: 'inherit',
                  textDecoration: 'none',
                }}
              >
                Mutualify
              </Typography>

              {/* Full size menu */}
              <Box sx={{ flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
                <Button
                  href="/friends"
                  color="secondary"
                    sx={{ my: 2, display: 'block' }}
                  >
                    Friends
                </Button>
                <Button
                  href="/followers"
                  color="secondary"
                    sx={{ my: 2, display: 'block' }}
                  >
                    Followers
                </Button>
              </Box>

              {/* Any size user */}
              <Box sx={{ flexGrow: 0 }}>
                {user && (
                <>
                  <Tooltip title="Open settings">
                    <Button onClick={handleOpenUserMenu}
                      size="large"
                      variant="outlined" 
                      color="secondary"
                      startIcon={<Avatar alt={user.username} src={`https://s.ppy.sh/a/${user.id}`} />}>
                        {user.username}
                    </Button>
                  </Tooltip>
                  <Menu
                    sx={{ mt: '45px' }}
                    id="menu-appbar"
                    anchorEl={anchorElUser}
                    anchorOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    keepMounted
                    transformOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    open={Boolean(anchorElUser)}
                    onClose={handleCloseUserMenu}
                  >
                    <MenuItem component="a" href="/settings">
                      <Typography textAlign="center">Settings</Typography>
                    </MenuItem>
                    <MenuItem onClick={logout}>
                      <Typography textAlign="center">Logout</Typography>
                    </MenuItem>
                  </Menu>
                </>
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
              </Box>
            </Toolbar>
          </Container>
        </Paper>
      </>
    )
  }