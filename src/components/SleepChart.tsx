import { useEffect, useRef, useState } from "react";

type CurvePoint = { sleepQualityIndex: number; bdi: number };
type ObservedPoint = { sleepQualityIndex: number; meanBdi: number; count: number };
type SleepCurve = { sleepHours: number; predicted: CurvePoint[]; observed: ObservedPoint[] };

const HEIGHT = 280;
const MARGIN = { top: 28, right: 16, bottom: 48, left: 40 };
const X_MIN = 1;
const X_MAX = 6;

async function getSleepCurve(): Promise<SleepCurve> {
	const response = await fetch("/api/sleepcurve");

	if (!response.ok) {
		throw new Error(`Backend returned ${response.status}`);
	}

	return await response.json();
}

export default function SleepChart() {
	const containerRef = useRef<HTMLDivElement>(null);
	const [width, setWidth] = useState(0);
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

	// The chart is drawn in real pixels, so it needs to know how wide its container is
	useEffect(() => {
		const container = containerRef.current;
		if (!container) return;

		const observer = new ResizeObserver(([entry]) => setWidth(entry.contentRect.width));
		observer.observe(container);
		return () => observer.disconnect();
	}, []);

	const plotWidth = Math.max(width - MARGIN.left - MARGIN.right, 0);
	const plotHeight = HEIGHT - MARGIN.top - MARGIN.bottom;

	// The y axis fits both series, so it doesn't jump when the observed points are added
	const values = curve
		? [...curve.predicted.map((p) => p.bdi), ...curve.observed.map((p) => p.meanBdi)]
		: [];
	const yMin = Math.min(0, Math.floor(Math.min(...values, 0) / 5) * 5);
	const yMax = Math.max(5, Math.ceil(Math.max(...values, 0) / 5) * 5);
	const yTicks = Array.from({ length: (yMax - yMin) / 5 + 1 }, (_, i) => yMin + i * 5);
	const xTicks = [1, 2, 3, 4, 5, 6];

	// Converts data values to pixel positions inside the svg
	const x = (value: number) => MARGIN.left + ((value - X_MIN) / (X_MAX - X_MIN)) * plotWidth;
	const y = (value: number) => MARGIN.top + plotHeight - ((value - yMin) / (yMax - yMin)) * plotHeight;

	const linePath = curve?.predicted
		.map((p, i) => `${i === 0 ? "M" : "L"}${x(p.sleepQualityIndex)},${y(p.bdi)}`)
		.join(" ");

	return (
		<div className="mt-4 flex flex-col gap-4">
			<div ref={containerRef} className="relative" style={{ height: HEIGHT }}>
				{!curve && (
					<p className="flex h-full items-center justify-center text-sm">
						{error ? `Kunde inte hämta grafen: ${error}` : "Hämtar modellens prediktioner..."}
					</p>
				)}

				{curve && width > 0 && (
					<svg
						width={width}
						height={HEIGHT}
						role="img"
						aria-label="Linjediagram: predicerad BDI-poäng mot sömnkvalitetsindex."
						className="block"
					>
						<text x={0} y={12} fontSize={12} fill="var(--text)">
							BDI
						</text>

						{yTicks.map((tick) => (
							<g key={tick}>
								<line
									x1={MARGIN.left}
									x2={MARGIN.left + plotWidth}
									y1={y(tick)}
									y2={y(tick)}
									stroke={tick === yMin ? "var(--chart-axis)" : "var(--border)"}
									strokeWidth={1}
								/>
								<text
									x={MARGIN.left - 8}
									y={y(tick)}
									fontSize={12}
									fill="var(--text)"
									textAnchor="end"
									dominantBaseline="middle"
									style={{ fontVariantNumeric: "tabular-nums" }}
								>
									{tick}
								</text>
							</g>
						))}

						{xTicks.map((tick) => (
							<text
								key={tick}
								x={x(tick)}
								y={MARGIN.top + plotHeight + 18}
								fontSize={12}
								fill="var(--text)"
								textAnchor="middle"
								style={{ fontVariantNumeric: "tabular-nums" }}
							>
								{tick}
							</text>
						))}

						<text
							x={MARGIN.left + plotWidth / 2}
							y={HEIGHT - 6}
							fontSize={12}
							fill="var(--text)"
							textAnchor="middle"
						>
							Sömnkvalitetsindex (1 = sover bra, 6 = sover dåligt)
						</text>

						<path
							d={linePath}
							fill="none"
							stroke="var(--series-1)"
							strokeWidth={2}
							strokeLinejoin="round"
							strokeLinecap="round"
						/>
					</svg>
				)}
			</div>
		</div>
	);
}
