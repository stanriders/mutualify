import * as React from 'react';
import useSWR from 'swr'
import api from '../lib/api'
import User from '../components/user'
import Head from 'next/head'
import Layout from '../components/layout'
import UserContext from '../context/userContext';
import { useContext } from 'react';

export default function Followers() {
  const {
    data: followers,
    error: followersError,
    isValidating: followersValidating 
  } = useSWR(`/followers`, api);

  const { user } = useContext(UserContext)

  return (
    <>
        <Head>
            <title>Follower list</title>
        </Head>
        <Layout title="Follower list">
          {!user && (<>Log in first!</>)}
          {user && (<>

            {!followers && (<>
                {followersValidating && (<>Loading...</>)}
                {followersError && followersError.info && (<>{followersError.info}</>)}
            </>)}

            {followers && (<>
                Known followers: {followers.length} out of {user.followerCount}.
                {followers.map((data) => (
                    <User id={data.id} username={data.username} />
                ))}
            </>)}
          </>)}
        </Layout>
    </>
  );
}
