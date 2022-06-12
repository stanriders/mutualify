import * as React from 'react';
import Typography from '@mui/material/Typography';
import Head from 'next/head'
import Layout from '../components/layout'

export default function Index() {
  return (
    <>
      <Head>
        <title>Mutualify</title>
      </Head>
      <Layout title="Mutualify">
        <Typography
            variant="h6"
            sx={{fontWeight: 100,}}
          >
            something something describing how it works and such 
        </Typography>
      </Layout>
    </>
  );
}
