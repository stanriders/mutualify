import useSWR from 'swr'
import api from '../lib/api'
import User from '../components/user'
import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Typography from '@mui/material/Typography';
import FormGroup from '@mui/material/FormGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import { useContext, useState } from 'react';

export default function Followers() {
  const { user } = useContext(UserContext)

  const [filterMutuals, setFilterMutuals] = useState(false);

  const {
    data: followers,
    error: followersError,
    isValidating: followersValidating 
  } = useSWR(`/followers?filterMutuals=${filterMutuals}`, api);

  return (
    <>
        <Head>
            <title>Mutualify - Follower list</title>
        </Head>
          {!user && (<Unauthorized/>)}
          {user && (<>

            {!followers && (<>
                {followersValidating && (<>Loading...</>)}
                {followersError && followersError.info && (<>{followersError.info}</>)}
            </>)}

            {followers && (<>
              <Typography variant="h6">
                Known followers: {followers.length} out of {user.followerCount}.
              </Typography>
              <FormGroup sx={{mb: 1}}>
                <FormControlLabel control={<Switch checked={filterMutuals} onChange={() => setFilterMutuals(!filterMutuals)}/>} label="Filter mutuals" />
              </FormGroup>
              {followers.map((data) => (
                <User id={data.id} username={data.username} />
              ))}
            </>)}
          </>)}
    </>
  );
}
