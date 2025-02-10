import PropTypes from "prop-types";
import Head from "next/head";
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import { CacheProvider } from "@emotion/react";
import theme from "../components/theme";
import createEmotionCache from "../components/createEmotionCache";
import useAuth from "../hooks/useAuth";
import UserContext from "../context/userContext";
import Layout from "../components/layout";
import { useState, useEffect } from "react";
import { NextIntlClientProvider } from "next-intl";
import { useRouter } from "next/router";

// Client-side cache, shared for the whole session of the user in the browser.
const clientSideEmotionCache = createEmotionCache({ key: "next" });

export default function MyApp(props) {
  const { Component, emotionCache = clientSideEmotionCache, pageProps } = props;
  const { user } = useAuth();
  const router = useRouter();

  // FOUC hack
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);

  return (
    // TODO: figure out why emotion doesnt want to append tags when SSR
    <div style={{ visibility: !mounted ? "hidden" : undefined }}>
      <CacheProvider value={emotionCache}>
        <NextIntlClientProvider
          timeZone="Etc/UTC"
          locale={router.locale}
          messages={pageProps.messages}
        >
          <Head>
            <meta
              name="viewport"
              content="initial-scale=1, width=device-width"
            />
            <meta
              name="description"
              content="Mutualify is a friend list database for osu!"
            />
            <link
              rel="apple-touch-icon"
              sizes="180x180"
              href="/apple-touch-icon.png"
            />
            <link
              rel="icon"
              type="image/png"
              sizes="32x32"
              href="/favicon-32x32.png"
            />
            <link
              rel="icon"
              type="image/png"
              sizes="16x16"
              href="/favicon-16x16.png"
            />
            <link rel="manifest" href="/site.webmanifest" />
          </Head>
          <ThemeProvider theme={theme}>
            {/* CssBaseline kickstart an elegant, consistent, and simple baseline to build upon. */}
            <CssBaseline />
            <UserContext.Provider value={{ user }}>
              <Layout>
                <Component {...pageProps} />
              </Layout>
            </UserContext.Provider>
          </ThemeProvider>
        </NextIntlClientProvider>
      </CacheProvider>
    </div>
  );
}

MyApp.propTypes = {
  Component: PropTypes.elementType.isRequired,
  emotionCache: PropTypes.object,
  pageProps: PropTypes.object.isRequired,
};
