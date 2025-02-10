import { createTheme } from "@mui/material/styles";
import { red } from "@mui/material/colors";

// Create a theme instance.
const theme = createTheme({
  palette: {
    mode: "dark",
    primary: {
      main: "#ce93d8",
    },
    error: {
      main: red.A400,
    },
  },
});

export default theme;
