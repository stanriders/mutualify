import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Typography from '@mui/material/Typography';
import { useContext } from 'react';

export default function Settings() {
  const { user } = useContext(UserContext)

  return (
    <>
      <Head>
        <title>Mutualify - Settings</title>
      </Head>
        {!user && (<Unauthorized/>)}
        {user && (<>{/*
          <FormGroup>
            <FormControlLabel control={<Switch defaultChecked />} label="Allow access to your friend list." />
          </FormGroup>*/}
          <Typography variant="body1">
            Nothing here yet, but that's temporary!
          </Typography>
        </>)}
    </>
  );
}
