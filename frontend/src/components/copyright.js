import Link from '../components/Link';
import Typography from '@mui/material/Typography';
import {useTranslations} from 'next-intl';
import NextLink from 'next/link';
import MuiLink from '@mui/material/Link';

export default function Copyright() {
  const t = useTranslations('Footer');
  return (
      <>
        <Typography
            variant="body2"
            align="center"
            color="text.secondary"
          >
           {t.rich("made-by", {
              stanr: (chunks) => <Link href="https://osu.ppy.sh/users/7217455" underline="hover">{chunks}</Link>, 
              arhella: (chunks) => <Link href="https://osu.ppy.sh/users/4411044" underline="hover">{chunks}</Link> })}
        </Typography>
        <Typography
            variant="body2"
            align="center"
            color="text.secondary"
            sx={{mb: 2}}
          >
           <MuiLink href="/stats" underline="hover" component={NextLink}>{t("stats")}</MuiLink> | <Link href="https://github.com/stanriders/mutualify" underline="hover">{t("source")}</Link> | <Link href="https://paypal.me/stanridersnew" underline="hover">{t("donate")}</Link>
        </Typography>
      </>
    );
}