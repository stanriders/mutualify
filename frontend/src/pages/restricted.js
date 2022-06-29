import Typography from '@mui/material/Typography';
import Head from 'next/head'

export default function Restricted() {
    return (
      <>
        <Head>
          <title>Mutualify - Not Allowed</title>
        </Head>
        <Typography variant="h6" align="center">
            Sorry, but restricted players are not allowed to log in!
        </Typography>
      </>
    );
  }
  