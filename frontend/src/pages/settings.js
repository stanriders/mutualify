import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import LoadingButton from '@mui/lab/LoadingButton';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import { useContext, useEffect, useState } from 'react';
import { apiNoResponse } from '../lib/api';
import useAuth from '../hooks/useAuth';
import Tooltip from '@mui/material/Tooltip';
import { formatDistance } from 'date-fns'
import {useTranslations} from 'next-intl';

export default function Settings() {
  const t = useTranslations('Settings');
  const { user } = useContext(UserContext)
  const { invalidateUserCache } = useAuth();

  const [updated, setUpdated] = useState(false);
  const [loading, setLoading] = useState(false);
  const [allowFriendsAccess, setAllowFriendsAccess] = useState(false);
  const [updatedLabel, setUpdatedLabel] = useState(t("refresh"));

  async function handleRefresh() {
    setLoading(true);
    await apiNoResponse('/friends/refresh', {method: 'POST'});
    await invalidateUserCache();
    setLoading(false);
    setUpdated(true);
    setUpdatedLabel(t("refresh-success"));
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

  let tooltipTitle = "";
  if (user)
    tooltipTitle = t("refresh-tooltip", {updatedAgo: formatDistance(new Date(user.updatedAt), new Date(), { addSuffix: true })})

  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
        {!user && (<Unauthorized/>)}
        {user && (<>
          <Box>
            <Tooltip title={tooltipTitle}>
              <LoadingButton variant="outlined" 
                loading={loading} 
                onClick={handleRefresh} 
                disabled={updated} 
                children={updatedLabel} />
              </Tooltip>
          </Box>
          
          <Box sx={{ display: 'flex', flexWrap: 'wrap', mt: 2 }}>
            <FormControlLabel 
            control={<Switch defaultChecked={user.allowsFriendlistAccess} onChange={handleShare}/>} 
            label={t("allow-friendlist-access")}
            sx={{ mb: 1}}/>
            
            {(<TextField
              disabled
              label={t("profile-link")}
              sx={{ flexGrow: 1, display: allowFriendsAccess ? 'inherit' : 'none' }}
              defaultValue={`https://mutualify.stanr.info/users/${user.id}`}
            />)}
          </Box>
        </>)}
    </>
  );
}

export async function getStaticProps(context) {
    return {
      props: {
        messages: (await import(`../../locales/${context.locale}.json`)).default
      }
    };
  }
