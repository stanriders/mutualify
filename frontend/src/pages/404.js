import Typography from "@mui/material/Typography";
import Head from "next/head";
import { useTranslations } from "next-intl";

export default function Custom404() {
  return (
    <>
      <Head>
        <title>{`Mutualify - ${useTranslations("Generic")("page-not-found")}`}</title>
      </Head>
      <Typography variant="h6" align="center">
        {useTranslations("Generic")("page-not-found")}
      </Typography>
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
