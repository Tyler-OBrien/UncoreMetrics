import "../styles/globals.css";
import type { AppProps } from "next/app";
import { Link, Toolbar } from "@mui/material";
import { styled, alpha } from "@mui/material/styles";
import Box from "@mui/material/Box";
import AppBar from "@mui/material/AppBar";
import InputBase from "@mui/material/InputBase";
import SearchIcon from "@mui/icons-material/Search";
import { useRouter } from "next/router";

const Search = styled("div")(({ theme }) => ({
  position: "relative",
  borderRadius: theme.shape.borderRadius,
  backgroundColor: alpha(theme.palette.common.white, 0.15),
  "&:hover": {
    backgroundColor: alpha(theme.palette.common.white, 0.25),
  },
  marginLeft: 0,
  width: "100%",
  [theme.breakpoints.up("sm")]: {
    marginLeft: theme.spacing(1),
    width: "auto",
  },
}));

const SearchIconWrapper = styled("div")(({ theme }) => ({
  padding: theme.spacing(0, 2),
  height: "100%",
  position: "absolute",
  pointerEvents: "none",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
}));

const StyledInputBase = styled(InputBase)(({ theme }) => ({
  color: "inherit",
  "& .MuiInputBase-input": {
    padding: theme.spacing(1, 1, 1, 0),
    // vertical padding + font size from searchIcon
    paddingLeft: `calc(1em + ${theme.spacing(4)})`,
    transition: theme.transitions.create("width"),
    width: "100%",
    [theme.breakpoints.up("sm")]: {
      width: "12ch",
      "&:focus": {
        width: "20ch",
      },
    },
  },
}));

function MyApp({ Component, pageProps }: AppProps) {
  const router = useRouter();
  return (
    <div>
      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static">
          <Toolbar sx={{ flexWrap: "wrap" }}>
            <Link
              href="/#"
              variant="h6"
              color="inherit"
              noWrap
              sx={{ flexGrow: 1, textDecoration: "none" }}
            >
              Uncore Metrics
            </Link>
            <nav>
              <Link
                variant="button"
                color="text.primary"
                href="/jobs"
                sx={{ my: 1, mx: 1.5 }}
              >
                Scrape Jobs
              </Link>
              <Link
                variant="button"
                color="text.primary"
                href="/stats"
                sx={{ my: 1, mx: 1.5 }}
              >
                Statistics
              </Link>
            </nav>
            <Search>
              <SearchIconWrapper>
                <SearchIcon />
              </SearchIconWrapper>
              <StyledInputBase
                placeholder="Search Servers…"
                inputProps={{ "aria-label": "search" }}
                onChange={(e) => {
                  // @ts-ignore - Really messy, should use something like Redux in the future, or higher order components
                  if (globalThis.onSearchChange) {
                    // @ts-ignore
                    globalThis.onSearchChange(e.target.value);
                  } else {
                    router.push({
                      pathname: "/",
                      query: { search: e.target.value },
                    });
                  }
                }}
              />
            </Search>
          </Toolbar>
        </AppBar>
      </Box>
      <Component {...pageProps} />
    </div>
  );
}

export default MyApp;
