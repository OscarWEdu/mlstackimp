import { useNavigate } from "react-router-dom";

export default function HomePage() {
	const navigate = useNavigate();

	return (
		<div className="flex flex-col gap-6 py-6">
			<section className="grid items-center gap-8 rounded-xl border-2 p-8 md:grid-cols-2 md:p-12">
			<div className="flex flex-col items-start gap-5">
					<h1 className="text-4xl font-bold md:text-5xl">
						Din mentala hälsa,<br />visualiserat.
					</h1>
					<p className="max-w-xl">
						Få insikter om din sömn och depression, baserat på data
						från 4 810 ungdomar.
					</p>
					<button
						type="button"
						onClick={() => navigate("/stat1")}
						className="rounded-lg bg-purple-600 px-6 py-3 font-medium text-white transition-opacity hover:opacity-90"
					>
						Starta analysen
					</button>
				</div>

			<div className="p-4">
		</div>
		</section>
		
		</div>
		
	);
}
