import { useEffect, useState } from "react";
import {
	CartesianGrid,
	ComposedChart,
	Line,
	ResponsiveContainer,
	Tooltip,
	XAxis,
	YAxis,
} from "recharts";
import type { TooltipContentProps } from "recharts";

type CurvePoint = { sleepQualityIndex: number; bdi: number };
type ObservedPoint = { sleepQualityIndex: number; meanBdi: number; count: number };
type SleepCurve = { sleepHours: number; predicted: CurvePoint[]; observed: ObservedPoint[] };

// One row per index on the scale, with the measured values added where there is a group
type ChartRow = { sleepQualityIndex: number; bdi: number; meanBdi?: number; count?: number };

async function getSleepCurve(): Promise<SleepCurve> {
	const response = await fetch("/api/sleepcurve");

	if (!response.ok) {
		throw new Error(`Backend returned ${response.status}`);
	}

	return await response.json();
}

function formatNumber(value: number, decimals: number) {
	return value.toLocaleString("sv-SE", {
		minimumFractionDigits: decimals,
		maximumFractionDigits: decimals,
	});
}

function ChartTooltip({ active, payload }: TooltipContentProps) {
	const row = payload?.[0]?.payload as ChartRow | undefined;
	if (!active || !row) return null;

	return (
		<div className="flex flex-col gap-1 rounded-lg border border-[var(--border)] bg-[var(--bg)] px-3 py-2 text-sm shadow-[var(--shadow)]">
			<span className="text-xs">Index {formatNumber(row.sleepQualityIndex, 2)}</span>
			<span className="flex items-center gap-2">
				<span className="inline-block h-0.5 w-3 rounded bg-[var(--series-1)]" />
				<strong className="text-[var(--text-h)]">{formatNumber(row.bdi, 1)}</strong>
				modellen
			</span>
			{row.meanBdi !== undefined && (
				<span className="flex items-center gap-2">
					<span className="inline-block size-2 rounded-full bg-[var(--series-2)]" />
					<strong className="text-[var(--text-h)]">{formatNumber(row.meanBdi, 1)}</strong>
					uppmätt (n = {row.count})
				</span>
			)}
		</div>
	);
}

export default function SleepChart() {
	const [curve, setCurve] = useState<SleepCurve | null>(null);
	const [error, setError] = useState("");

	useEffect(() => {
		getSleepCurve()
			.then(setCurve)
			.catch((error) => {
				setError(`${error}`);
				console.error(error);
			});
	}, []);

	if (!curve) {
		return (
			<p className="flex h-[300px] items-center justify-center text-sm">
				{error ? `Kunde inte hämta grafen: ${error}` : "Hämtar modellens prediktioner..."}
			</p>
		);
	}

	// Both series share the same x values, so they go in one array. Then the tooltip can show both at once.
	const rows: ChartRow[] = curve.predicted.map((p) => {
		const observed = curve.observed.find((o) => o.sleepQualityIndex === p.sleepQualityIndex);
		return { ...p, meanBdi: observed?.meanBdi, count: observed?.count };
	});

	const axisTick = { fill: "var(--text)", fontSize: 12 };

	return (
		<div className="mt-4 flex flex-col gap-4">
			<div className="flex flex-wrap gap-x-6 gap-y-1 text-sm">
				<span className="flex items-center gap-2">
					<span className="inline-block h-0.5 w-4 rounded bg-[var(--series-1)]" />
					Modellens prediktion ({formatNumber(curve.sleepHours, 1)} h sömn)
				</span>
				<span className="flex items-center gap-2">
					<span className="inline-block size-2 rounded-full bg-[var(--series-2)]" />
					Uppmätt medelvärde i datan
				</span>
			</div>

			<ResponsiveContainer width="100%" height={300}>
				<ComposedChart data={rows} margin={{ top: 8, right: 16, bottom: 24, left: -16 }}>
					<CartesianGrid vertical={false} stroke="var(--border)" />
					<XAxis
						type="number"
						dataKey="sleepQualityIndex"
						domain={[1, 6]}
						ticks={[1, 2, 3, 4, 5, 6]}
						tick={axisTick}
						stroke="var(--chart-axis)"
						label={{
							value: "Sömnkvalitetsindex (1 = sover bra, 6 = sover dåligt)",
							position: "bottom",
							fill: "var(--text)",
							fontSize: 12,
						}}
					/>
					<YAxis tick={axisTick} stroke="var(--chart-axis)" />
					<Tooltip content={ChartTooltip} cursor={{ stroke: "var(--chart-axis)" }} />
					<Line
						dataKey="bdi"
						name="Modellen"
						stroke="var(--series-1)"
						strokeWidth={2}
						dot={false}
						activeDot={{ r: 5, stroke: "var(--bg)", strokeWidth: 2 }}
						isAnimationActive={false}
					/>
					<Line
						dataKey="meanBdi"
						name="Uppmätt"
						stroke="none"
						dot={{ r: 4, fill: "var(--series-2)", stroke: "var(--bg)", strokeWidth: 2 }}
						activeDot={{ r: 5, fill: "var(--series-2)", stroke: "var(--bg)", strokeWidth: 2 }}
						isAnimationActive={false}
					/>
				</ComposedChart>
			</ResponsiveContainer>
		</div>
	);
}
