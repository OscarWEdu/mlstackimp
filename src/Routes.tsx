import type { RouteObject } from "react-router-dom";
import HomePage from "./pages/HomePage.tsx";

const routes: RouteObject[] = [
	{ path: "/", element: <HomePage /> }
];

export default routes;
