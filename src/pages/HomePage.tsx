import { useNavigate } from "react-router-dom";

export default function HomePage() {
	const navigate = useNavigate();

	return (
		<div className="flex flex-col items-start gap-4">
			<button onClick={() => navigate("/stat1")}>
				Analysis 1
			</button>

			<button onClick={() => navigate("/somnpredictor")}>
				Sömnkvalitets-prediktor
			</button>

			<button onClick={() => navigate("/lifestyle")}>
				Livsstils-prediktor
			</button>
		</div>
	);
}
