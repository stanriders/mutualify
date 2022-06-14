import Head from 'next/head'
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Link from '../components/Link';
import User from '../components/user'
import api from '../lib/api';

export default function Rankings({data}) {
  return (
    <>
      <Head>
        <title>Mutualify - Follower rankings</title>
      </Head>
      <TableContainer component={Paper} elevation={2}>
      <Table aria-label="simple table">
        <TableHead>
          <TableRow>
            <TableCell width={24}>#</TableCell>
            <TableCell>Player</TableCell>
            <TableCell align="right">Followers</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {data.map((row, index) => (
            <TableRow key={row.username}>
              <TableCell>{index+1}</TableCell>
              <TableCell component="th" scope="row">
                <User id={row.id} username={row.username} />
              </TableCell>
              <TableCell align="right">{row.followerCount}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
    </>
  );
}

export async function getServerSideProps(context) {
  const data = await api(`/rankings`);
  return {
    props: {data}
  }
}