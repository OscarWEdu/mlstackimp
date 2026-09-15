import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import App from "./App";
import Routes from "./Routes.tsx";

const router = createBrowserRouter([
	{
		path: "/",
		element: <App />,
		children: Routes,
	},
]);

// Create the React root element
createRoot(document.querySelector("#root")!).render(
	<StrictMode>
		<RouterProvider router={router} />
	</StrictMode>,
);
