import Typography from '@mui/material/Typography';
import Head from 'next/head'
import User from '../../components/user'
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import api from '../../lib/api'

export default function Users({data}) {

  return (
    <>
      {data && <>

        {!data.user && <>
          <Head>
            <title>Mutualify -  Unknown user</title>
            <meta name="robots" content="noindex" />
          </Head>
          <Typography variant="h6" align="center">
            This user never logged in! You might wanna tell them to ;)
          </Typography>
        </>}

        {data.user && <>
          <Head>
            <title>Mutualify -  {data.user.username}</title>
          </Head>

          {!data.user.allowsFriendlistAccess && <>
            <Box sx={{display: 'flex', ml: 1, mb: 1, alignContent: 'center' }}>
              <Avatar alt={data.user.username} src={`https://s.ppy.sh/a/${data.user.id}`} sx={{mr: 1}}/>
              <Typography variant="h6" sx={{height: 'fit-content', flexGrow: 1}}>
                {data.user.username}'s friend list is private.
              </Typography>
            </Box>
          </>}

          {data.user.allowsFriendlistAccess && data.friends && <>
            <Box sx={{display: 'flex', ml: 1, mb: 1, alignContent: 'center' }}>
              <Avatar alt={data.user.username} src={`https://s.ppy.sh/a/${data.user.id}`} sx={{mr: 1}}/>
              <Typography variant="h6" sx={{height: 'fit-content', flexGrow: 1}}>
                {data.user.username} has {data.friends.length} friends.
              </Typography>
            </Box>

            {data.friends.map((friend) => (
                <User id={friend.id} key={friend.id} username={friend.username} mutual={friend.mutual} showFriendlistButton={friend.allowsFriendlistAccess} />
            ))}
          </>}
        </>}
      </>}
    </>
  );
}

export async function getServerSideProps(context) {
    const data = await api(`/friends/${context.params.id}`);
    return {
      props: {data}
    }
  }