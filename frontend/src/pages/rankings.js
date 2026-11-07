import Head from "next/head";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import User from "../components/user";
import api from "../lib/api";
import Pagination from "@mui/material/Pagination";
import UserContext from "../context/userContext";
import { useRouter } from "next/router";
import { useContext } from "react";
import useSWR from "swr";
import { useTranslations } from "next-intl";

export default function Rankings() {
  const t = useTranslations("Leaderboard");
  const tGeneric = useTranslations("Generic");

  const { user } = useContext(UserContext);

  const router = useRouter();

  // The URL decides the page, so direct links and the back button stay in sync
  const queryPage = Number(router.query.page);
  const page = Number.isInteger(queryPage) && queryPage > 0 ? queryPage : 1;
  const offset = (page - 1) * 50;

  // router.query is empty until the router is ready on statically generated pages
  const { data: players, error: playersError } = useSWR(
    router.isReady ? `/rankings?offset=${offset}` : null,
    api,
  );

  const { data: playerRank } = useSWR(user ? `/rankings/me` : null, api);

  const handleChange = (event, value) => {
    router.push(
      { pathname: router.pathname, query: { page: value } },
      undefined,
      { scroll: false, shallow: true },
    );
  };

  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>

      {playerRank && (
        <>
          <Typography variant="h6" sx={{ mb: 1 }}>
            {t("your-rank", { rank: playerRank })}
          </Typography>
        </>
      )}

      {!players && !playersError && <>{tGeneric("loading")}</>}
      {!players && playersError && playersError.info && playersError.info}

      {players && (
        <>
          <TableContainer component={Paper} elevation={3}>
            <Table aria-label="simple table" size="small">
              <TableHead>
                <TableRow>
                  <TableCell align="center" width={1}>
                    {t("table-header-rank")}
                  </TableCell>
                  <TableCell sx={{ py: 2 }}>
                    {t("table-header-player")}
                  </TableCell>
                  <TableCell align="right">
                    {t("table-header-followers")}
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {players.users.map((row, index) => (
                  <TableRow
                    hover
                    key={row.username}
                    selected={user && user.id == row.id}
                  >
                    <TableCell align="center">{offset + index + 1}</TableCell>
                    <TableCell sx={{ py: 2 }}>
                      <User
                        id={row.id}
                        username={row.username}
                        showFriendlistButton={row.allowsFriendlistAccess}
                      />
                    </TableCell>
                    <TableCell align="right">{row.followerCount}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          {players.total > 50 && (
            <Pagination
              count={Math.ceil(players.total / 50)}
              page={page}
              onChange={handleChange}
              sx={{ mt: 2 }}
            />
          )}
        </>
      )}
    </>
  );
}

export async function getStaticProps(context) {
  return {
    props: {
      messages: (await import(`../../locales/${context.locale}.json`)).default,
    },
  };
}
