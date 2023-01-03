import type { NextPage } from "next";
import Head from "next/head";
import Link from "next/link";
import ServerDisplay from "../components/servers/server-display";

const Home: NextPage = () => {
  return (
    <div>
      <div className="flex min-h-screen flex-col items-center justify-center py-2">
        <Head>
          <title>Uncore Metrics</title>
        </Head>

        <main className="flex w-full flex-1 flex-col items-center justify-center px-20 text-center">
          <ServerDisplay></ServerDisplay>
        </main>

        <footer className="flex h-24 w-full items-center justify-center border-t space-x-2">
          <a
            className="flex items-center justify-center gap-2"
            href="https://github.com/Tyler-OBrien/UncoreMetrics"
            target="_blank"
            rel="noopener noreferrer"
          >
            Powered by Uncore Metrics
          </a>
          <span className="flex">|</span>
          <Link className="flex  justify-center gap-2 " href="/about">
            About
          </Link>
        </footer>
      </div>
    </div>
  );
};

export default Home;
