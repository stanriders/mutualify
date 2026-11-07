import Typography from "@mui/material/Typography";
import Head from "next/head";

export default function Custom500() {
  return (
    <>
      <Head>
        <title>Mutualify - Something went wrong</title>
      </Head>
      <Typography variant="h6" align="center" sx={{ mt: 4 }}>
        Something went wrong. Please try again later.
      </Typography>
    </>
  );
}

// Layout's header and footer are translated, and this page has no messages.
Custom500.noLayout = true;
