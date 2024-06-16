import Typography from '@mui/material/Typography';
import Head from 'next/head'
import {useTranslations} from 'next-intl';

export default function Restricted() {
  const t = useTranslations('Restricted');
    return (
      <>
        <Head>
          <title>{`Mutualify - ${t("title")}`}</title>
        </Head>
        <Typography variant="h6" align="center">
            {t("message")}
        </Typography>
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
  