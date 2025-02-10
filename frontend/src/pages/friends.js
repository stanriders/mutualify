import useSWR from "swr";
import api from "../lib/api";
import User from "../components/user";
import Head from "next/head";
import Unauthorized from "../components/unauthorized";
import UserContext from "../context/userContext";
import Typography from "@mui/material/Typography";
import FormGroup from "@mui/material/FormGroup";
import InputLabel from "@mui/material/InputLabel";
import MenuItem from "@mui/material/MenuItem";
import FormControl from "@mui/material/FormControl";
import Select from "@mui/material/Select";
import { useContext, useState } from "react";
import { useTranslations } from "next-intl";
import deepmerge from "deepmerge";

export default function Friends() {
  const t = useTranslations("Friends");
  const tGeneric = useTranslations("Generic");

  const [sorting, setSorting] = useState("Username");

  const handleSortingChange = (event) => {
    setSorting(event.target.value);
  };

  const {
    data: friends,
    error: friendsError,
    isValidating: friendsValidating,
  } = useSWR(`/friends`, api);

  const { user } = useContext(UserContext);
  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
      {!user && <Unauthorized />}
      {user && (
        <>
          {!friends && (
            <>
              {friendsValidating && <>{tGeneric("loading")}</>}
              {friendsError && friendsError.info && <>{friendsError.info}</>}
            </>
          )}

          {friends && (
            <>
              <Typography variant="h6" sx={{ mb: 1 }}>
                {t("friend-count", { friendCount: friends.length })}
              </Typography>
              <FormGroup sx={{ mb: 1 }} row={true}>
                <FormControl size="small" sx={{ minWidth: 130 }}>
                  <InputLabel id="sorting-label">
                    {tGeneric("sort-by")}
                  </InputLabel>
                  <Select
                    labelId="sorting-label"
                    id="sorting-select"
                    value={sorting}
                    label={tGeneric("sort-by")}
                    onChange={handleSortingChange}
                  >
                    <MenuItem value={"Username"}>
                      {tGeneric("sorting-username")}
                    </MenuItem>
                    <MenuItem value={"Rank"}>
                      {tGeneric("sorting-rank")}
                    </MenuItem>
                  </Select>
                </FormControl>
              </FormGroup>
              {friends
                .sort((a, b) => {
                  switch (sorting) {
                    case "Username":
                      return ("" + a.username).localeCompare(b.username);
                    case "Rank": {
                      // always put null ranked players at the end
                      if (a.rank === b.rank) return 0;
                      if (a.rank === null) return 1;
                      if (b.rank === null) return -1;
                      return a.rank > b.rank ? 1 : -1;
                    }
                  }
                })
                .map((data) => (
                  <User
                    id={data.id}
                    username={data.username}
                    showFriendlistButton={data.allowsFriendlistAccess}
                    mutualDate={data.relationCreatedAt}
                    key={data.id}
                  />
                ))}
            </>
          )}
        </>
      )}
    </>
  );
}

export async function getStaticProps(context) {
  const userMessages = (await import(`../../locales/${context.locale}.json`))
    .default;
  const defaultMessages = (await import(`../../locales/en-US.json`)).default;
  const messages = deepmerge(defaultMessages, userMessages);

  return {
    props: {
      messages: messages
    },
  };
}
