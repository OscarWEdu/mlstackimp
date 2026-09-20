import { useState } from "react";
import { useNavigate } from "react-router-dom";

export default function LifestylePage() {
    const navigate = useNavigate();

    const [leisureScreenHours, setLeisureScreenHours] = useState("");
    const [sleepQualityIndex, setSleepQualityIndex] = useState("");
    const [averageSleepHours, setAverageSleepHours] = useState("");
    const [sex, setSex] = useState("female");
    const [result, setResult] = useState("");

    async function predict() {
        if (leisureScreenHours === "" || sleepQualityIndex === "" || averageSleepHours === "") {
            setResult("Fyll i alla fält.");
            return;
        }

        try {
            const response = await fetch("/api/lifestylepredict", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    leisureScreenHours: Number(leisureScreenHours),
                    sleepQualityIndex: Number(sleepQualityIndex),
                    averageSleepHours: Number(averageSleepHours),
                    sex,
                }),
            });

            const data: { bdi: number; } = await response.json();

            const level =
                data.bdi < 14 ? "minimal" :
                    data.bdi < 20 ? "mild" :
                        data.bdi < 29 ? "måttlig" : "svår";

            setResult(`Predikterad BDI-poäng: ${data.bdi.toFixed(1)} av 63 – ${level} nivå av depressiva symptom`);
        } catch (error) {
            setResult(`Backend error: ${error}`);
        }
    }

    return (
        <section id="center">
            <div className="w-full max-w-2xl pb-16">
                <button
                    type="button"
                    onClick={() => navigate("/")}
                    className="mb-6 rounded border border-gray-300 px-3 py-1 hover:bg-gray-100"
                >
                    ← Tillbaka
                </button>

                <h1 className="text-2xl font-bold">Livsstils-prediktor</h1>

                <div className="mt-4 rounded border-2 border-yellow-400 bg-yellow-50 px-4 py-3 text-sm">
                    <strong>OBS – detta är inte medicinsk rådgivning.</strong>{" "}
                    Modellen är tränad på ett dataset med 4 810 ungdomar och förklarar
                    bara ungefär 15 % av variationen i depressionspoäng. Resultatet är
                    en grov uppskattning. Prata med vården om du mår dåligt.
                </div>

                <div className="mt-8">
                    <div className="mb-5">
                        <label htmlFor="screen" className="block font-semibold">
                            Skärmtid på fritiden (timmar per dag)
                        </label>
                        <p className="text-sm text-gray-500">
                            Tid framför mobil, dator och TV.
                        </p>
                        <input
                            id="screen"
                            type="number"
                            step="0.1"
                            value={leisureScreenHours}
                            onChange={(e) => setLeisureScreenHours(e.target.value)}
                            className="mt-1 w-32 rounded border border-gray-300 px-2 py-1"
                        />
                    </div>

                    <div className="mb-5">
                        <label htmlFor="quality" className="block font-semibold">
                            Sömnkvalitet (1–6)
                        </label>
                        <p className="text-sm text-gray-500">
                            Hur mycket sömnproblem du har. 1 = sover bra, 6 = sover mycket dåligt.
                        </p>
                        <input
                            id="quality"
                            type="number"
                            step="0.1"
                            value={sleepQualityIndex}
                            onChange={(e) => setSleepQualityIndex(e.target.value)}
                            className="mt-1 w-32 rounded border border-gray-300 px-2 py-1"
                        />
                    </div>

                    <div className="mb-5">
                        <label htmlFor="hours" className="block font-semibold">
                            Sömn per natt (timmar)
                        </label>
                        <p className="text-sm text-gray-500">
                            Hur många timmar du i genomsnitt sover per natt.
                        </p>
                        <input
                            id="hours"
                            type="number"
                            step="0.5"
                            value={averageSleepHours}
                            onChange={(e) => setAverageSleepHours(e.target.value)}
                            className="mt-1 w-32 rounded border border-gray-300 px-2 py-1"
                        />
                    </div>

                    <div className="mb-6">
                        <label htmlFor="sex" className="block font-semibold">
                            Kön
                        </label>
                        <p className="text-sm text-gray-500">
                        </p>
                        <select
                            id="sex"
                            value={sex}
                            onChange={(e) => setSex(e.target.value)}
                            className="mt-1 rounded border border-gray-300 px-2 py-1"
                        >
                            <option value="female">Flicka</option>
                            <option value="male">Pojke</option>
                        </select>
                    </div>

                    <button
                        type="button"
                        onClick={predict}
                        className="mt-2 rounded bg-green-600 px-4 py-2 text-white"
                    >
                        Prediktera BDI
                    </button>

                    {result && <p className="mt-6 text-lg font-semibold">{result}</p>}
                </div>
            </div>
        </section>
    );
};