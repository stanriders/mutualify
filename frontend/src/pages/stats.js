import Typography from '@mui/material/Typography';
import Head from 'next/head'
import api from '../lib/api'

export default function Stats({data}) {
  return (
    <>
      <Head>
        <title>Mutualify - Stats</title>
      </Head>
        <Typography variant="body1">
            Registered users: {data.registeredCount}
        </Typography>
        <Typography variant="body1">
            Relation count: {data.relationCount}
        </Typography>
    </>
  );
}

export async function getServerSideProps(context) {
    const data = await api(`/stats`);
    return {
      props: {data}
    }
  }