import useSWR from "swr";
import api from "../lib/api";
import User from "../components/user";
import Head from "next/head";
import Unauthorized from "../components/unauthorized";
import UserContext from "../context/userContext";
import Typography from "@mui/material/Typography";
import FormGroup from "@mui/material/FormGroup";
import FormControlLabel from "@mui/material/FormControlLabel";
import Switch from "@mui/material/Switch";
import { useContext, useState } from "react";
import { useTranslations } from "next-intl";
import InputLabel from "@mui/material/InputLabel";
import MenuItem from "@mui/material/MenuItem";
import FormControl from "@mui/material/FormControl";
import Select from "@mui/material/Select";
import deepmerge from "deepmerge";

export default function Followers() {
  const t = useTranslations("Followers");
  const tGeneric = useTranslations("Generic");
  const { user } = useContext(UserContext);

  const [filterMutuals, setFilterMutuals] = useState(false);
  const [sorting, setSorting] = useState("Username");

  const handleSortingChange = (event) => {
    setSorting(event.target.value);
  };

  const {
    data: followers,
    error: followersError,
    isValidating: followersValidating,
  } = useSWR(`/followers`, api);

  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
      {!user && <Unauthorized />}
      {user && (
        <>
          {!followers && (
            <>
              {followersValidating && <>{tGeneric("loading")}</>}
              {followersError && followersError.info && (
                <>{followersError.info}</>
              )}
            </>
          )}

          {followers && (
            <>
              <Typography variant="h6">
                {t("followers-count", {
                  knownCount: followers.length,
                  totalCount: user.followerCount,
                })}
              </Typography>
              <FormGroup sx={{ mb: 1, mt: 1 }} row={true}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={filterMutuals}
                      onChange={() => setFilterMutuals(!filterMutuals)}
                    />
                  }
                  label={t("hide-mutuals")}
                />
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
                    <MenuItem value={"FollowDate"}>
                      {tGeneric("sorting-followdate")}
                    </MenuItem>
                  </Select>
                </FormControl>
              </FormGroup>
              {followers
                .filter((data) => {
                  if (filterMutuals && data.mutual) return false;
                  return true;
                })
                .sort((a, b) => {
                  switch (sorting) {
                    case "Username":
                      return ("" + a.username).localeCompare(b.username);
                    case "Rank": {
                      if (a.rank === b.rank) return 0;
                      if (a.rank === null) return 1;
                      if (b.rank === null) return -1;
                      return a.rank > b.rank ? 1 : -1;
                    }
                    case "FollowDate":
                      if (a.relationCreatedAt === b.relationCreatedAt) return 0;
                      if (a.relationCreatedAt === null) return 1;
                      if (b.relationCreatedAt === null) return -1;
                      return new Date(b.relationCreatedAt).getUTCDate() >
                        new Date(a.relationCreatedAt).getUTCDate()
                        ? 1
                        : -1;
                  }
                })
                .map((data) => (
                  <User
                    id={data.id}
                    key={data.id}
                    username={data.username}
                    mutual={data.mutual}
                    showFriendlistButton={data.allowsFriendlistAccess}
                    mutualDate={data.relationCreatedAt}
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
