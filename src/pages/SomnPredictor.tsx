import { useState } from "react";
import { useNavigate } from "react-router-dom";

// Förenklade variabelnamn med förklaringar. "Fältet" är namnet som skickas
// till backend och motsvarar en kolumn i datasetet (se kommentaren nedan).
//   sleepHours         -> avg_sleep_hours
//   leisureScreenHours -> est_leisure_screen_hours
//   screenTimeIndex    -> screen_time_index
//   weekendMidsleep    -> midsleep_weekend_hours
//   socialJetlag       -> social_jetlag_hours
const variabler = [
    {
        fält: "sleepHours",
        namn: "Sömn per natt (timmar)",
        förklaring:
            "Hur många timmar du i genomsnitt sover per natt.",
        min: 0,
        max: 24,
    },
    {
        fält: "leisureScreenHours",
        namn: "Skärmtid på fritiden (timmar per dag)",
        förklaring:
            "Tid framför mobil, dator och TV på fritiden – alltså utanför skolarbete.",
        min: 0,
        max: 24,
    },
    {
        fält: "screenTimeIndex",
        namn: "Skärmtidsindex",
        förklaring:
            "Ett sammanvägt mått på din genomsnittliga skärmtid. " +
            "Skalan går ungefär från 0 till 7, där högre värde = mer skärmtid.",
        min: undefined,
        max: undefined,
    },
    {
        fält: "weekendMidsleep",
        namn: "Insomning på helgen (timmar efter midnatt)",
        förklaring:
            "Mittpunkten för nattens sömn på helgen, räknat i timmar efter midnatt " +
            "(t.ex. 3 = mittpunkten är kl 03:00). Kan vara negativt om du sover " +
            "innan midnatt.",
        min: undefined,
        max: undefined,
    },
    {
        fält: "socialJetlag",
        namn: "Sömnskillnad mellan vardag och helg (timmar)",
        förklaring:
            "Skillnaden i timmar på tiden du lägger dig en vardag jämfört med " +
            "en helgkväll.",
        min: 0,
        max: undefined,
    },
];

export default function SomnPredictorPage() {
    const navigate = useNavigate();

    const [kon, setKon] = useState<"flicka" | "pojke">("flicka");
    const [värden, setVärden] = useState<Record<string, string>>({});
    const [resultat, setResultat] = useState("");

    function sättVärde(fält: string, värde: string) {
        setVärden((aktuella) => ({ ...aktuella, [fält]: värde }));
    }

    async function prediktera() {
        // Kontrollera att alla fält är ifyllda, är giltiga tal och ligger
        // inom sina tillåtna intervall (t.ex. sömntid 0–24 timmar).
        for (const v of variabler) {
            const rå = värden[v.fält];
            if (rå === undefined || rå.trim() === "" || isNaN(Number(rå))) {
                setResultat(`Fyll i ett giltigt värde för "${v.namn}".`);
                return;
            }
            const tal = Number(rå);
            if (v.min !== undefined && tal < v.min) {
                setResultat(`"${v.namn}" kan inte vara under ${v.min}.`);
                return;
            }
            if (v.max !== undefined && tal > v.max) {
                setResultat(`"${v.namn}" kan inte vara över ${v.max}.`);
                return;
            }
        }

        try {
            const response = await fetch("/api/somnpredict", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    screenTimeIndex: Number(värden.screenTimeIndex),
                    leisureScreenHours: Number(värden.leisureScreenHours),
                    sleepHours: Number(värden.sleepHours),
                    weekendMidsleep: Number(värden.weekendMidsleep),
                    socialJetlag: Number(värden.socialJetlag),
                    kon,
                }),
            });

            if (!response.ok) {
                throw new Error(`Backend returned ${response.status}`);
            }

            const data: { sleepQuality: number } = await response.json();

            setResultat(
                `Predikterat sömnkvalitetsindex: ${data.sleepQuality.toFixed(2)} ` +
                `(skala ca 1–6, lägre = bättre sömn)`
            );
        } catch (error) {
            setResultat(`Backend error: ${error}`);
            console.error(error);
        }
    }

    return (
        // Fullbredds-wrapper: gör att innehållet vänsterställs trots att den
        // delade #center-stilen centrerar, och ger luft under formuläret.
        <section id="center">
            <div className="w-full max-w-2xl pb-16">
                <button
                    type="button"
                    onClick={() => navigate(-1)}
                    className="mb-6 rounded border border-gray-300 px-3 py-1 hover:bg-gray-100"
                >
                    ← Tillbaka
                </button>

                <h1 className="text-2xl font-bold">Sömnkvalitets-prediktor</h1>

                {/* Viktig disclaimer: modellen är statistik, inte medicin. */}
                <div className="mt-4 rounded border-2 border-yellow-400 bg-yellow-50 px-4 py-3 text-sm">
                    <strong>OBS – detta är inte medicinsk rådgivning.</strong>{" "}
                    Prediktorn är en statistisk modell tränad på mönster i ett dataset
                    med 4 810 ungdomar. Den förklarar bara en liten del av variationen
                    i sömnkvalitet, så resultaten är grova uppskattningar. Prata med
                    vården om du är orolig för din sömn.
                </div>

                <div className="mt-8">
                    <label className="mb-2 block text-xl font-bold">
                        Vem gäller prediktionen för?
                    </label>
                    <div className="mb-6 flex flex-col gap-2 sm:flex-row sm:gap-6">
                        <label className="flex items-center gap-2">
                            <input
                                type="radio"
                                name="kon"
                                value="flicka"
                                checked={kon === "flicka"}
                                onChange={() => setKon("flicka")}
                                className="h-5 w-5"
                            />
                            Flicka (tränad på flickor i datasetet)
                        </label>
                        <label className="flex items-center gap-2">
                            <input
                                type="radio"
                                name="kon"
                                value="pojke"
                                checked={kon === "pojke"}
                                onChange={() => setKon("pojke")}
                                className="h-5 w-5"
                            />
                            Pojke (tränad på pojkar i datasetet)
                        </label>
                    </div>

                    <label className="mb-2 block text-xl font-bold">
                        Fyll i uppgifterna (ett genomsnittligt läsår):
                    </label>

                    {variabler.map((v) => (
                        <div key={v.fält} className="mb-5">
                            <label htmlFor={v.fält} className="block font-semibold">
                                {v.namn}
                            </label>
                            <p className="text-sm text-gray-500">{v.förklaring}</p>
                            <input
                                id={v.fält}
                                type="number"
                                step="0.1"
                                min={v.min}
                                max={v.max}
                                value={värden[v.fält] ?? ""}
                                onChange={(event) =>
                                    sättVärde(v.fält, event.target.value)
                                }
                                className="mt-1 w-32 rounded border border-gray-300 px-2 py-1"
                            />
                        </div>
                    ))}

                    <button
                        type="button"
                        onClick={prediktera}
                        className="mt-2 rounded bg-green-600 px-4 py-2 text-white"
                    >
                        Prediktera sömnkvalitet
                    </button>

                    {resultat && (
                        <p className="mt-6 text-lg font-semibold">{resultat}</p>
                    )}
                </div>
            </div>
        </section>
    );
}
