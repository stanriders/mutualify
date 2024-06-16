import Head from 'next/head'
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import User from '../components/user'
import api from '../lib/api';
import Pagination from '@mui/material/Pagination';
import UserContext from '../context/userContext';
import { useRouter } from 'next/router'
import { useContext, useEffect, useState } from 'react'
import useSWR from 'swr'
import {useTranslations} from 'next-intl';

export default function Rankings() {
  const t = useTranslations('Leaderboard');
  const tGeneric = useTranslations('Generic');

  const { user } = useContext(UserContext)

  const router = useRouter()
  const [page, setPage] = useState(1);
  const offset = (page-1) * 50;

  const {
    data: players,
    error: playersError,
    isValidating: playersValidating } = useSWR(
      `/rankings?offset=${offset}`, api
  );

  const {
    data: playerRank,
    error: playerRankError,
    isValidating: playerRankValidating } = useSWR(
      `/rankings/me`, api
  );

  // Handle direct link to page and/or filter
  useEffect(() => {
    const qs = new URLSearchParams(window.location.search)
    const qsPage = Number(qs.get('page'))

    if (qsPage > 0) {
      setPage(qsPage)
    }
  }, [])

  // Update history and url when filter/page changes
  useEffect(() => {
    router.push({
      query: {
        page
      },
    }, undefined, {
      scroll: false,
    })
  }, [page])

  const handleChange = (event, value) => {
    setPage(value);
  };

  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
      
      {playerRank && (<>
        <Typography variant="h6" sx={{mb: 1}}>
            {t("your-rank", {rank: playerRank})}
        </Typography></>)}

      {!players && playersValidating && (<>{tGeneric("loading")}</>)}
      {!players && playersError && playersError.info && playersError.info}

      {players && (<>
        <TableContainer component={Paper} elevation={2}>
          <Table aria-label="simple table">
            <TableHead>
              <TableRow>
                <TableCell width={24}>{t("table-header-rank")}</TableCell>
                <TableCell>{t("table-header-player")}</TableCell>
                <TableCell align="right">{t("table-header-followers")}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {players.users.map((row, index) => (
                <TableRow key={row.username} selected={user && user.id == row.id}>
                  <TableCell>{offset+index+1}</TableCell>
                  <TableCell component="th" scope="row">
                    <User id={row.id} username={row.username} showFriendlistButton={row.allowsFriendlistAccess} />
                  </TableCell>
                  <TableCell align="right">{row.followerCount}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
        {players.total > 50 && (<Pagination count={Math.ceil(players.total / 50)} page={page} onChange={handleChange} sx={{mt: 2}}/>)}
      </>)}
    </>
  );
}

export async function getStaticProps(context) {
    return {
      props: {
        messages: (await import(`../../locales/${context.locale}.json`)).default
      }
    };
  }