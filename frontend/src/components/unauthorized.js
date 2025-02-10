import Typography from "@mui/material/Typography";
import { useTranslations } from "next-intl";

export default function Unauthorized() {
  return (
    <Typography variant="h4" align="center">
      {useTranslations("Unauthorized")("message")}
    </Typography>
  );
}
