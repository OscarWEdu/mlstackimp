import { useNavigate } from "react-router-dom";

export default function HomePage() {
	const navigate = useNavigate();

	return (
		<div>
			<button onClick={() => navigate("/stat1")}>
				Analysis 1
			</button>
		</div>
	);
}
