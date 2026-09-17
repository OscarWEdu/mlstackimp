import type { RouteObject } from "react-router-dom";
import HomePage from "./pages/HomePage.tsx";
import Stat1Page from "./pages/Stat1.tsx";
import SomnPredictorPage from "./pages/SomnPredictor.tsx";

const routes: RouteObject[] = [
	{ path: "/", element: <HomePage /> },
	{ path: "/stat1", element: <Stat1Page /> },
	{ path: "/somnpredictor", element: <SomnPredictorPage /> }
];

export default routes;
