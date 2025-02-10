import Typography from "@mui/material/Typography";
import Head from "next/head";
import { useTranslations } from "next-intl";

export default function Index() {
  const t = useTranslations("Home");
  return (
    <>
      <Head>
        <title>Mutualify</title>
      </Head>
      <Typography variant="h6" align="center">
        {t("faq-title")}
      </Typography>
      <Typography variant="body1" mb={0.5}>
        <b>{t("faq-q")}</b>: {t("faq-q1")}
      </Typography>
      <Typography variant="body1" mb={2}>
        <b>{t("faq-a")}</b>: {t("faq-a1")}
      </Typography>
      <Typography variant="body1" mb={0.5}>
        <b>{t("faq-q")}</b>: {t("faq-q2")}
      </Typography>
      <Typography variant="body1" mb={2}>
        <b>{t("faq-a")}</b>: {t("faq-a2")}
      </Typography>
      <Typography variant="body1" mb={0.5}>
        <b>{t("faq-q")}</b>: {t("faq-q3")}
      </Typography>
      <Typography variant="body1" mb={2}>
        <b>{t("faq-a")}</b>: {t("faq-a3")}
      </Typography>
      <Typography variant="body1" mb={0.5}>
        <b>{t("faq-q")}</b>: {t("faq-q4")}
      </Typography>
      <Typography variant="body1" mb={1}>
        <b>{t("faq-a")}</b>: {t("faq-a4")}
      </Typography>
    </>
  );
}

export async function getServerSideProps(context) {
  return {
    props: {
      messages: (await import(`../../locales/${context.locale}.json`)).default,
    },
  };
}
