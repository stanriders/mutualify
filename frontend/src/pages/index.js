import Typography from '@mui/material/Typography';
import Head from 'next/head'

export default function Index() {
  return (
    <>
      <Head>
        <title>Mutualify</title>
      </Head>
        <Typography variant="h6" align="center">
            Mutualify is a friend list database for osu!
        </Typography>
        <Typography variant="body1">
            You log in, website saves your friend list and then others will be able to see if they're in your friend list. It works the other way too - you can see who added you to their friend list as well!
        </Typography>
    </>
  );
}
