import { Outlet, useLocation } from "react-router-dom";
import "./style.css";
import logo from "./assets/MLSTACK.png";

export default function App() {
	useLocation();

	return (
		<div className="flex min-h-screen flex-col overflow-x-hidden">
			<header className="flex items-center justify-between px-6  border-b-2">
				<img src={logo} alt="MlStack Logo" className="h-20 w-auto" />
			</header>
		
			<main className="container mx-auto flex flex-1 flex-col px-4">
				<Outlet />
			</main>
		</div>
	);
}
