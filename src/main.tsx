import './style.css'

async function getBackendResponse(): Promise<string> {
    const response = await fetch("/api");

    if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
    }

    return await response.text();
}

document.querySelector<HTMLDivElement>('#app')!.innerHTML = `
<section id="center">
    <p id="backend-response">Connecting...</p>
</section>
`;

const output = document.querySelector<HTMLParagraphElement>('#backend-response')!;

getBackendResponse()
    .then(data => {
        output.textContent = `Backend response: ${data}`;
    })
    .catch(error => {
        output.textContent = `Backend error: ${error}`;
        console.error(error);
    });

