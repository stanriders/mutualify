import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Button from '@mui/material/Button';
import { useContext, useState } from 'react';
import { apiNoResponse } from '../lib/api';

export default function Settings() {
  const { user } = useContext(UserContext)

  const [updated, setUpdated] = useState(false);
  const [updatedLabel, setUpdatedLabel] = useState("Refresh your friend list");
  async function handleClick() {
    setUpdated(true);
    setUpdatedLabel("Updated!");
    await apiNoResponse('/friends/refresh', {method: 'POST'});
  }

  return (
    <>
      <Head>
        <title>Mutualify - Settings</title>
      </Head>
        {!user && (<Unauthorized/>)}
        {user && (<>
        <Button variant="outlined" onClick={handleClick} disabled={updated} children={updatedLabel} />
        {/*
          <FormGroup>
            <FormControlLabel control={<Switch defaultChecked />} label="Allow access to your friend list." />
          </FormGroup>*/}
        </>)}
    </>
  );
}
