import Typography from "@mui/material/Typography";
import Head from "next/head";
import User from "../../components/user";
import Link from "../../components/Link";
import Avatar from "@mui/material/Avatar";
import Box from "@mui/material/Box";
import api from "../../lib/api";
import { useTranslations } from "next-intl";

export default function Users({ data }) {
  const t = useTranslations("User");
  return (
    <>
      {data && (
        <>
          {!data.user && (
            <>
              <Head>
                <title>{`Mutualify - ${t("unknown-user-title")}`}</title>
                <meta name="robots" content="noindex" />
              </Head>
              <Typography variant="h6" align="center">
                {t("unknown-user-message")}
              </Typography>
            </>
          )}

          {data.user && (
            <>
              <Head>
                <title>{`Mutualify - ${data.user.username}`}</title>
              </Head>

              {!data.user.allowsFriendlistAccess && (
                <>
                  <Box
                    sx={{
                      display: "flex",
                      ml: 1,
                      mb: 1,
                      alignContent: "center",
                    }}
                  >
                    <Avatar
                      alt={data.user.username}
                      src={`https://s.ppy.sh/a/${data.user.id}`}
                      sx={{ mr: 1 }}
                    />
                    <Typography
                      variant="h6"
                      sx={{ height: "fit-content", flexGrow: 1, pt: 0.45 }}
                    >
                      {t.rich("private-list", {
                        user: (chunks) => (
                          <Link
                            underline="hover"
                            href={`https://osu.ppy.sh/users/${data.user.id}`}
                          >
                            {chunks}
                          </Link>
                        ),
                        username: data.user.username,
                      })}
                    </Typography>
                  </Box>
                </>
              )}

              {data.user.allowsFriendlistAccess && data.friends && (
                <>
                  <Box
                    sx={{
                      display: "flex",
                      ml: 1,
                      mb: 1,
                      alignContent: "center",
                    }}
                  >
                    <Avatar
                      alt={data.user.username}
                      src={`https://s.ppy.sh/a/${data.user.id}`}
                      sx={{ mr: 1 }}
                    />
                    <Typography
                      variant="h6"
                      sx={{ height: "fit-content", flexGrow: 1, pt: 0.45 }}
                    >
                      {t.rich("friend-count", {
                        user: (chunks) => (
                          <Link
                            underline="hover"
                            href={`https://osu.ppy.sh/users/${data.user.id}`}
                          >
                            {chunks}
                          </Link>
                        ),
                        username: data.user.username,
                        friendsCount: data.friends.length,
                      })}
                    </Typography>
                  </Box>
                  <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1 }}>
                    {data.friends.map((friend) => (
                      <User
                        id={friend.id}
                        key={friend.id}
                        username={friend.username}
                        mutual={friend.mutual}
                        showFriendlistButton={friend.allowsFriendlistAccess}
                        mutualDate={data.relationCreatedAt}
                      />
                    ))}
                  </Box>
                </>
              )}
            </>
          )}
        </>
      )}
    </>
  );
}

export async function getServerSideProps(context) {
  const data = await api(`/friends/${context.params.id}`);
  return {
    props: {
      data,
      messages: (await import(`../../../locales/${context.locale}.json`))
        .default,
    },
  };
}
