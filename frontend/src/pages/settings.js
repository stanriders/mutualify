import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Button from '@mui/material/Button';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import { useContext, useEffect, useState } from 'react';
import { apiNoResponse } from '../lib/api';
import useAuth from '../hooks/useAuth';

export default function Settings() {
  const { user } = useContext(UserContext)
  const { invalidateUserCache } = useAuth();

  const [updated, setUpdated] = useState(false);
  const [allowFriendsAccess, setAllowFriendsAccess] = useState(false);
  const [updatedLabel, setUpdatedLabel] = useState("Refresh your friend list");

  async function handleRefresh() {
    await apiNoResponse('/friends/refresh', {method: 'POST'});
    setUpdated(true);
    setUpdatedLabel("Updated!");
    await invalidateUserCache();
  }

  async function handleShare(event) {
    await apiNoResponse('/friends/access/toggle', {method: 'POST', body: event.target.checked});
    await invalidateUserCache();
    setAllowFriendsAccess(event.target.checked);
  }

  useEffect(() => {
    if (user)
      setAllowFriendsAccess(user.allowsFriendlistAccess);
 }, [setAllowFriendsAccess, user]);

  return (
    <>
      <Head>
        <title>Mutualify - Settings</title>
      </Head>
        {!user && (<Unauthorized/>)}
        {user && (<>
          <Box>
            <Button variant="outlined" onClick={handleRefresh} disabled={updated} children={updatedLabel} />
          </Box>
          
          <Box sx={{ display: 'flex', flexWrap: 'wrap', mt: 2 }}>
            <FormControlLabel 
            control={<Switch defaultChecked={user.allowsFriendlistAccess} onChange={handleShare}/>} 
            label="Allow other users to access your friend list" 
            sx={{ mb: 1}}/>
            
            {(<TextField
              disabled
              label="Your profile link"
              sx={{ flexGrow: 1, display: allowFriendsAccess ? 'inherit' : 'none' }}
              defaultValue={`https://mutualify.stanr.info/users/${user.id}`}
            />)}
          </Box>
        </>)}
    </>
  );
}
