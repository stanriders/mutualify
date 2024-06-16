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

export default function Followers() {
  const t = useTranslations('Followers');
  const tGeneric = useTranslations('Generic');
  const { user } = useContext(UserContext)

  const [filterMutuals, setFilterMutuals] = useState(false);
  const [sortByRank, setSortByRank] = useState(false);

  const {
    data: followers,
    error: followersError,
    isValidating: followersValidating 
  } = useSWR(`/followers`, api);

  return (
    <>
        <Head>
            <title>{`Mutualify - ${t("title")}`}</title>
        </Head>
          {!user && (<Unauthorized/>)}
          {user && (<>

            {!followers && (<>
                {followersValidating && (<>{tGeneric("loading")}</>)}
                {followersError && followersError.info && (<>{followersError.info}</>)}
            </>)}

            {followers && (<>
              <Typography variant="h6">
                {t("followers-count", {knownCount: followers.length, totalCount: user.followerCount})}
              </Typography>
              <FormGroup sx={{mb: 1}} row={true}>
                <FormControlLabel control={<Switch checked={filterMutuals} onChange={() => setFilterMutuals(!filterMutuals)}/>} label={t("hide-mutuals")} />
                <FormControlLabel control={<Switch checked={sortByRank} onChange={() => setSortByRank(!sortByRank)}/>} label={tGeneric("sort-by-rank")} />
              </FormGroup>
              {followers.filter((data) => {
                if (filterMutuals && data.mutual)
                  return false;
                return true;
              }).sort((a, b) => {
                if (!sortByRank)
                  return ('' + a.username).localeCompare(b.username);
                
                // always put null ranked players at the end
                if (a.rank == null)
                  return 1;

                return a.rank - b.rank;
              }).map((data) => (
                <User id={data.id} key={data.id} username={data.username} mutual={data.mutual} showFriendlistButton={data.allowsFriendlistAccess} mutualDate={data.relationCreatedAt}/>
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
