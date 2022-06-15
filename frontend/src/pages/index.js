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
        <Typography variant="body1" mb={0.5}>
            <b>Q</b>: What is this?
        </Typography>
        <Typography variant="body1" mb={2}>
            <b>A</b>: Mutualify is a database for osu! players' friend lists. It stores friend list of every player that logged in as well as some other relevant data.
        </Typography>
        <Typography variant="body1" mb={0.5}>
            <b>Q</b>: How does it work?
        </Typography>
        <Typography variant="body1" mb={2}>
            <b>A</b>: osu! API allows websites to get player's friend list. Mutualify cross-checks friend lists of all registered players to see who follows who.
        </Typography>
        <Typography variant="body1" mb={0.5}>
            <b>Q</b>: Can it steal my account/password/private data? 
        </Typography>
        <Typography variant="body1" mb={2}>
            <b>A</b>: No, it can only access what osu! website shows you when you log in which is your friend list, your public profile data and you friend's public profile data. It will never ask for your login/password or anything like that.
        </Typography>
        <Typography variant="body1" mb={0.5}>
            <b>Q</b>: Why can I see only some of my followers?
        </Typography>
        <Typography variant="body1" mb={1}>
            <b>A</b>: Mutualify can only find people that logged in before, so spread the word! More registered people means more information about followers.
        </Typography>
    </>
  );
}
