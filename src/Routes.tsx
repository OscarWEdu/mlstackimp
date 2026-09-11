import type { RouteObject } from "react-router-dom";
import HomePage from "./pages/HomePage.tsx";
import Stat1Page from "./pages/Stat1.tsx";

const routes: RouteObject[] = [
	{ path: "/", element: <HomePage /> },
	{ path: "/stat1", element: <Stat1Page /> }
];

export default routes;
