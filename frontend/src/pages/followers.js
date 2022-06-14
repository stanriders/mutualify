import useSWR from 'swr'
import api from '../lib/api'
import User from '../components/user'
import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Typography from '@mui/material/Typography';
import { useContext } from 'react';

export default function Followers() {
  const { user } = useContext(UserContext)
  const {
    data: followers,
    error: followersError,
    isValidating: followersValidating 
  } = useSWR(`/followers`, api);

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
              <Typography variant="h6" component="h6">
                Known followers: {followers.length} out of {user.followerCount}.
              </Typography>
              {followers.map((data) => (
                <User id={data.id} username={data.username} />
              ))}
            </>)}
          </>)}
    </>
  );
}
