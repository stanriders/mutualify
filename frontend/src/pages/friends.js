import useSWR from 'swr'
import api from '../lib/api'
import User from '../components/user'
import Head from 'next/head'
import Unauthorized from '../components/unauthorized'
import UserContext from '../context/userContext';
import Typography from '@mui/material/Typography';
import FormGroup from '@mui/material/FormGroup';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import { useContext, useState } from 'react';
import {useTranslations} from 'next-intl';

export default function Friends() {
  const t = useTranslations('Friends');
  const tGeneric = useTranslations('Generic');

  const [sortByRank, setSortByRank] = useState(false);

  const {
    data: friends,
    error: friendsError,
    isValidating: friendsValidating 
  } = useSWR(`/friends`, api);

  const { user } = useContext(UserContext)
  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
      {!user && (<Unauthorized/>)}
      {user && (<>
        {!friends && (<>
            {friendsValidating && (<>{tGeneric("loading")}</>)}
            {friendsError && friendsError.info && (<>{friendsError.info}</>)}
        </>)}

        {friends && (<>
          <Typography variant="h6" sx={{mb: 1}}>
            {t("friend-count", {friendCount: friends.length})}
          </Typography>
          <FormGroup sx={{mb: 1}} row={true}>
                <FormControlLabel control={<Switch checked={sortByRank} onChange={() => setSortByRank(!sortByRank)}/>} label={tGeneric("sort-by-rank")} />
          </FormGroup>
          {friends.sort((a, b) => {
                if (!sortByRank)
                  return ('' + a.username).localeCompare(b.username);
                // always put null ranked players at the end
                if (a.rank == null)
                  return 1;
                return a.rank - b.rank;
              })
            .map((data) => (
            <User id={data.id} username={data.username} showFriendlistButton={data.allowsFriendlistAccess} mutualDate={data.relationCreatedAt}/>
          ))}
        </>)}
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
