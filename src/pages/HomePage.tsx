import { useNavigate } from "react-router-dom";
import SleepChart from "../components/SleepChart";

export default function HomePage() {
	const navigate = useNavigate();
	
	const analyses = [
	{ title: "Sömn → BDI", beskrivning: "4 frågor · ~1 min", path: "/stat1" },
	{ title: "Sömnkvalitets-prediktor", beskrivning: "Skattar din sömnkvalitet", path: "/somnpredictor" },
	{ title: "Livsstils-prediktor", beskrivning: "Sömn och skärmtid", path: "/lifestyle" },
];


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

		<section className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
			{analyses.map((a) => (
					<article
						key={a.title}
						onClick={() => a.path && navigate(a.path)}
						className={`flex items-center justify-between gap-4 rounded-xl border border-[var(--border)] p-5 ${
							a.path ? "cursor-pointer hover:border-[var(--accent-border)]" : "opacity-60"
						}`}
					>
						<div>
							<h2 className="font-semibold text-[var(--text-h)]">{a.title}</h2>
							<p className="text-sm">{a.beskrivning}</p>
						</div>
						<div className="h-12 w-20 shrink-0 rounded bg-[var(--accent-bg)]" />
					</article>
				))}

		</section>

				<section className="rounded-xl border border-[var(--border)] p-6">
			<h2 className="font-semibold text-[var(--text-h)]">
				Sömnkvalitet vs depression
			</h2>
			<p className="text-sm">
				Vad vår modell förutspår, jämfört med vad som faktiskt uppmättes. Grupper med färre än 20 personer visas inte.
			</p>
			<SleepChart />
		</section>

		
		</div>
		
	);
}
