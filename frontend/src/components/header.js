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
import {useTranslations} from 'next-intl';
import {useRouter} from 'next/router';
import Cookies from "js-cookie";
import Locale from "./locale";
import NextLink from 'next/link';

export default function Header({title}) {
    const { user } = useContext(UserContext)
    const { logout } = useAuth()
    const t = useTranslations('Header');
    const router = useRouter();

    const [anchorElNav, setAnchorElNav] = useState(null);
    const [anchorElUser, setAnchorElUser] = useState(null);
    const [anchorElLocale, setAnchorElLocale] = useState(null);
  
    const handleOpenNavMenu = (event) => {
      setAnchorElNav(event.currentTarget);
    };
    const handleOpenUserMenu = (event) => {
      setAnchorElUser(event.currentTarget);
    };
    const handleOpenLocale = (event) => {
      setAnchorElLocale(event.currentTarget);
    };
  
    const handleCloseNavMenu = () => {
      setAnchorElNav(null);
    };
    const handleCloseUserMenu = () => {
      setAnchorElUser(null);
    };
    const handleCloseLocale = () => {
      setAnchorElLocale(null);
    };

    const menuItems = [
      {
        title: t("friends"),
        link: "/friends"
      },
      {
        title: t("followers"),
        link: "/followers"
      },
      {
        title: t("leaderboard"),
        link: "/rankings"
      }
    ];
    
    function changeLocale(locale) {
      router.push(
        {
          pathname: router.pathname,
          query: router.query
        },
        router.asPath,
        { locale }
      );

      // Override the accept language header to persist chosen language
      // @see https://nextjs.org/docs/advanced-features/i18n-routing#leveraging-the-next_locale-cookie
      Cookies.set("NEXT_LOCALE", locale);
      handleCloseLocale();
    }

    return (
      <>
        <Paper elevation={2} sx={{
            marginTop: 1,
            padding: 1.5
          }}>

          <Container sx={{px: 0}}>
            <Toolbar disableGutters>
              {/* Full size logo */}
              <Box sx={{ flexGrow: 0, display: { xs: 'none', sm: 'none', md: 'flex' } }}>
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
                  display: { xs: 'none', sm: 'none', md: 'flex' },
                  fontFamily: 'monospace',
                  fontWeight: 700,
                  letterSpacing: '.2rem',
                  color: 'inherit',
                  textDecoration: 'none',
                }}
              >
                 Mutualify
              </Typography>

              {/* Small/medium size menu */}
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
                  {menuItems.map((items) => (
                    <MenuItem component={NextLink} key={items.title} href={items.link}>
                      <Typography textAlign="center">{items.title}</Typography>
                    </MenuItem>
                  ))}
                </Menu>
              </Box>
              
              {/* Small size logo */}
              <Box sx={{ flexGrow: 1, display: { xs: 'flex', sm: 'none', md: 'none' } }}>
                <Image src="/logo.svg" width={40} height={40}/>
              </Box>

              {/* Medium size logo */}
              <Typography
                variant="h6"
                noWrap
                component="a"
                href="/"
                sx={{
                  ml: 2,
                  mr: 2,
                  flexGrow: 1,
                  display: { xs: 'none', sm: 'flex', md: 'none' },
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
                {menuItems.map((items) => (
                  <Button href={items.link} key={items.title} color="secondary" sx={{ my: 2, display: 'block' }} component={NextLink}>
                      {items.title}
                  </Button>
                  ))}
              </Box>

              {/* Any size settings */}
              <Box sx={{ flexGrow: 0 }}>
                <Button sx={{ mr: 1, maxWidth: 35, minWidth: 35 }}
                        size="large"
                        variant="outlined" 
                        color="secondary" 
                        onClick={handleOpenLocale}>
                          <Locale locale={router.locale}/>
                </Button>
                <Menu
                    sx={{ mt: '45px' }}
                    id="menu-appbar"
                    anchorEl={anchorElLocale}
                    anchorOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    keepMounted
                    transformOrigin={{
                      vertical: 'top',
                      horizontal: 'right',
                    }}
                    open={Boolean(anchorElLocale)}
                    onClose={handleCloseLocale}
                  >
                  {router.locales.map((locale) => (
                    <MenuItem onClick={() => changeLocale(locale)}><Locale locale={locale}/></MenuItem>
                  ))}
                </Menu>
                {user && (
                <>
                  <Tooltip title={t("user-tooltip")}>
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
                    <MenuItem component={NextLink} href="/settings">
                      <Typography textAlign="center">{t("settings")}</Typography>
                    </MenuItem>
                    <MenuItem onClick={logout}>
                      <Typography textAlign="center">{t("logout")}</Typography>
                    </MenuItem>
                  </Menu>
                </>
                ) || (
                  <Button 
                    size="large"
                    variant="outlined" 
                    href="/api/oauth/auth"
                    color="secondary"
                    startIcon={<Avatar alt="avatar" src={`https://s.ppy.sh/a/-1`} />}>
                        {t("login")}
                  </Button>
                )}
              </Box>
            </Toolbar>
          </Container>
        </Paper>
      </>
    )
  }