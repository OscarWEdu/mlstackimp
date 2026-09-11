import { useEffect, useState } from "react";
import "./../style.css";

async function getBackendResponse(): Promise<string> {
	const response = await fetch("/api");
	
	if (!response.ok) {
		throw new Error(`Backend returned ${response.status}`);
	}
	
	return await response.text();
}

export default function HomePage() {
	const [message, setMessage] = useState("Connecting...");
	
	useEffect(() => {
		getBackendResponse()
		.then((data) => {
			setMessage(`Backend response: ${data}`);
		})
		.catch((error) => {
			setMessage(`Backend error: ${error}`);
			console.error(error);
		});
	}, []);
	
	return (
		<section id="center">
		<p id="backend-response">{message}</p>
		</section>
	);
}
