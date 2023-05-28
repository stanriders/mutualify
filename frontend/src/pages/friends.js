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

export default function Friends() {

  const [sortByRank, setSortByRank] = useState(false);

  const {
    data: friends,
    error: friendsError,
    isValidating: friendsValidating 
  } = useSWR(`/friends?orderByRank=${sortByRank}`, api);

  const { user } = useContext(UserContext)
  return (
    <>
      <Head>
        <title>Mutualify - Friend list</title>
      </Head>
      {!user && (<Unauthorized/>)}
      {user && (<>
        {!friends && (<>
            {friendsValidating && (<>Loading...</>)}
            {friendsError && friendsError.info && (<>{friendsError.info}</>)}
        </>)}

        {friends && (<>
          <Typography variant="h6" sx={{mb: 1}}>
            You have {friends.length} friends.
          </Typography>
          <FormGroup sx={{mb: 1}} row={true}>
                <FormControlLabel control={<Switch checked={sortByRank} onChange={() => setSortByRank(!sortByRank)}/>} label="Sort by rank" />
          </FormGroup>
          {friends.map((data) => (
            <User id={data.id} username={data.username} showFriendlistButton={data.allowsFriendlistAccess} />
          ))}
        </>)}
      </>)}
    </>
  );
}
