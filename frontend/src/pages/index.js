import * as React from 'react';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Head from 'next/head'
import Layout from '../components/layout'

export default function Index() {
  return (
    <>
      <Head>
        <title>Mutualify</title>
      </Head>
      <Layout title="Mutualify">
        <Button 
          size="large"
          variant="outlined" 
          href="/friends"
          color="secondary"
          sx={{
            margin: 1
          }}>
            friend list
        </Button>
        <Button 
          size="large"
          variant="outlined" 
          href="/followers"
          color="secondary"
          sx={{
            margin: 1
          }}>
            follower list
        </Button>
      </Layout>
    </>
  );
}
