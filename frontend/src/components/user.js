import Avatar from "@mui/material/Avatar";
import Chip from "@mui/material/Chip";
import ListIcon from "@mui/icons-material/List";
import { useRouter } from "next/router";
import { Tooltip } from "@mui/material";
import { formatDistance } from "date-fns";

export default function User({
  id,
  username,
  mutual = false,
  showFriendlistButton = false,
  mutualDate = null,
}) {
  const router = useRouter();

  var bgColor = mutual ? "#2f223366" : "transparent";
  var deleteIcon = showFriendlistButton ? <ListIcon /> : <></>;

  const handleClick = () => {
    window.open(`https://osu.ppy.sh/users/${id}`, "_blank");
  };

  const handleDelete = () => {
    if (showFriendlistButton) {
      if (router.locale != "en-US")
        router.push(`/${router.locale}/users/${id}`);
      else router.push(`/users/${id}`);
    }
  };

  let mutualDateTooltip = "";
  if (mutualDate) {
    mutualDateTooltip = formatDistance(new Date(mutualDate), new Date(), {
      addSuffix: true,
    });
  }

  return (
    <>
      <Tooltip title={mutualDateTooltip}>
        <Chip
          avatar={<Avatar alt={username} src={`https://s.ppy.sh/a/${id}`} />}
          label={username}
          variant="outlined"
          sx={{ color: "#fff", bgcolor: bgColor }}
          clickable
          target="_blank"
          onClick={handleClick}
          onDelete={handleDelete}
          deleteIcon={deleteIcon}
          color={mutual ? "primary" : "default"}
        />
      </Tooltip>
    </>
  );
}
