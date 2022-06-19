import Avatar from '@mui/material/Avatar';
import Chip from '@mui/material/Chip';

export default function User({id, username, mutual = false}) {
  return (
    <>
        <Chip
            avatar={<Avatar alt={username} src={`https://s.ppy.sh/a/${id}`} />}
            label={username}
            variant="outlined"
            sx={{ mt: 1, ml: 1, color: '#fff'}}
            component="a"
            href={`https://osu.ppy.sh/users/${id}`}
            clickable
            target="_blank"
            color={mutual ? "primary" : "default"}
        />
    </>
    );
}