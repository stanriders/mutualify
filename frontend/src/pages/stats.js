import Typography from "@mui/material/Typography";
import Head from "next/head";
import api from "../lib/api";
import { useTranslations } from "next-intl";

export default function Stats({ data }) {
  const t = useTranslations("Stats");
  return (
    <>
      <Head>
        <title>{`Mutualify - ${t("title")}`}</title>
      </Head>
      <Typography variant="body1">
        {t("registered", {
          registered: data.registeredCount,
          lastDayRegistered: data.lastDayRegisteredCount,
        })}
      </Typography>
      <Typography variant="body1">
        {t("relations", { relations: data.relationCount })}
      </Typography>
      <Typography variant="body1">
        {t("autoupdate", { eligible: data.eligibleForUpdateCount })}
      </Typography>
    </>
  );
}

export async function getServerSideProps(context) {
  const data = await apiServerside(`/stats`);
  return {
    props: {
      data,
      messages: (await import(`../../locales/${context.locale}.json`)).default,
    },
  };
}
