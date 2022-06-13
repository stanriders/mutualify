import Link from '../components/Link';
import Typography from '@mui/material/Typography';

export default function Copyright() {
  return (
        <Typography
            variant="body2"
            align="center"
            color="text.secondary"
          >
           Made by <Link href="https://osu.ppy.sh/users/7217455">StanR</Link>, icon by <Link href="https://osu.ppy.sh/users/4411044">Arhella</Link>.
        </Typography>
    );
}