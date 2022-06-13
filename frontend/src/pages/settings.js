import Head from 'next/head'
import Layout from '../components/layout'
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
      <Layout title="Mutualify">
        {!user && (<>Log in first!</>)}
        {user && (<>{/*
          <FormGroup>
            <FormControlLabel control={<Switch defaultChecked />} label="Allow access to your friend list." />
          </FormGroup>*/}
          <Typography
            variant="h6"
            sx={{fontWeight: 100,}}
          >
            Nothing here yet, but that's temporary!
          </Typography>
        </>)}
      </Layout>
    </>
  );
}
