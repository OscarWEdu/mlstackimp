import { Outlet, useLocation } from "react-router-dom";

export default function App() {
	useLocation();

	return (
		<div className="flex min-h-screen flex-col overflow-x-hidden">
			<header className="flex items-center justify-between px-6 py-4">
				<span className="font-mono text-lg font-bold tracking-tight text-primary-foreground">
					MlStack
				</span>
			</header>
		
			<main className="container mx-auto flex flex-1 flex-col px-4">
				<Outlet />
			</main>
		</div>
	);
}
