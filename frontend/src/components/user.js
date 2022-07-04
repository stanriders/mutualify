import Avatar from '@mui/material/Avatar';
import Chip from '@mui/material/Chip';
import ListIcon from '@mui/icons-material/List';
import { useRouter } from 'next/router';

export default function User({id, username, mutual = false, showFriendlistButton = false}) {
  const router = useRouter();

  var bgColor = mutual ? '#2f223366' : "transparent";
  var deleteIcon = showFriendlistButton ? <ListIcon /> : <></>;

  const handleClick = () => {
    window.open(`https://osu.ppy.sh/users/${id}`, '_blank');
  };

  const handleDelete = () => {
    if (showFriendlistButton)
      router.push(`/users/${id}`);
  };

  return (
    <>
        <Chip
            avatar={<Avatar alt={username} src={`https://s.ppy.sh/a/${id}`} />}
            label={username}
            variant="outlined"
            sx={{ mt: 1, ml: 1, color: '#fff', bgcolor: bgColor}}
            clickable
            target="_blank"
            onClick={handleClick}
            onDelete={handleDelete}
            deleteIcon={deleteIcon}
            color={mutual ? "primary" : "default"}
        />
    </>
    );
}