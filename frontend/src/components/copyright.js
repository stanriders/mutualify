import Link from '../components/Link';
import Typography from '@mui/material/Typography';

export default function Copyright() {
  return (
      <>
        <Typography
            variant="body2"
            align="center"
            color="text.secondary"
          >
           Made by <Link href="https://osu.ppy.sh/users/7217455" underline="hover">StanR</Link>, icon by <Link href="https://osu.ppy.sh/users/4411044" underline="hover">Arhella</Link>.
        </Typography>
        <Typography
            variant="body2"
            align="center"
            color="text.secondary"
            sx={{mb: 2}}
          >
           <Link href="/stats" underline="hover">Stats</Link> | <Link href="https://github.com/stanriders/mutualify" underline="hover">Source code</Link> | <Link href="https://paypal.me/stanridersnew" underline="hover">Donate ❤</Link>
        </Typography>
      </>
    );
}