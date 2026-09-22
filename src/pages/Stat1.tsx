import { useState } from "react";

const questions = [
    "Repeated awakenings (with difficulties going back to sleep):",
    "Disturbed/restless sleep:",
    "Difficulties falling asleep:",
    "Premature (final) awakening:",
];

const options = [
    "Never",
    "Seldom",
    "Sometimes",
    "Fairly Often",
    "Most of the Time",
    "Always",
];

export default function Stat1Page() {
    const [answers, setAnswers] = useState<(number | null)[]>(
        Array(questions.length).fill(null)
    );

    const [questionnaireMessage, setQuestionnaireMessage] = useState("");
    const [averageSleepHours, setAverageSleepHours] = useState(0);
    const [sex, setSex] = useState<string>("");

    function selectAnswer(questionIndex: number, answer: number) {
        setAnswers((current) => {
            const updated = [...current];
            updated[questionIndex] = answer;
            return updated;
        });
    }

    async function finishQuestionnaire() {
        if (answers.some((answer) => answer === null)) {
            setQuestionnaireMessage("Please answer all four questions.");
            return;
        }
        if (!sex) {
            setQuestionnaireMessage("Please select your sex.");
            return;
        }


        const numericAnswers: number[] = answers.filter((answer): answer is number => answer !== null);
        const average = numericAnswers.reduce((sum, answer) => sum + answer, 0) / numericAnswers.length;

        try {
            const response = await fetch("/api/sleeppredict", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    sleepQualityIndex: average,
                    averageSleepHours: averageSleepHours,
                    sex: sex,
                }),
            });

            if (!response.ok) {
                throw new Error(`Backend returned ${response.status}`);
            }

            const data: { bdi: number } = await response.json();

            const level =
                data.bdi <= 13 ? "minimal" :
                data.bdi <= 19 ? "mild" :
                data.bdi <= 28 ? "måttlig" : "svår";

            setQuestionnaireMessage(
                `Beck Depression Inventory-II Score: ${data.bdi.toFixed(1)} of 63 - ${level} estimated depression.`
            );
        } catch (error) {
            setQuestionnaireMessage(`Backend error: ${error}`);
            console.error(error);
        }
    }

    return (
        <section id="center" className="w-full p-16">
            <div className="w-full max-w-2xl">
                <h1 className="text-2xl font-bold">Depression Predictor</h1>
                <div className="mt-4 rounded border-2 border-yellow-400 bg-yellow-50 px-4 py-3 text-sm">
                    <strong>Beware: This is not medical advice.</strong>
                    The results given here is a prediction based on a model trained on 
                    snapshot of self reported data from 4 810 youths between 12-16 and
                    may not be an accurate representation of the general population.
                    If you or anyone you know suffers from depression, please contact
                    your local healthcare provider.
                </div>
            </div>
            <div className="mt-4">
                <label htmlFor="sex" className="mr-3 font-medium">
                    Gender
                </label>
                <select
                    id="sex"
                    value={sex}
                    onChange={(e) => setSex(e.target.value)}
                    className="rounded border border-gray-300 px-2 py-1"
                >
                    <option value="">Select</option>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                </select>
            </div>

            <div className="mt-6">
                <label htmlFor="average-sleep-hours">
                    How many hours per night have you, on average, slept these last 2 weeks?
                </label>

                <input
                    id="average-sleep-hours"
                    type="number"
                    min="0"
                    max="10"
                    step="1"
                    value={averageSleepHours}
                    onChange={(event) => {
                        const value = Number(event.target.value)
                        setAverageSleepHours(Math.min(24, Math.max(0, value)))
                    }}
                    className="ml-3 w-20 rounded border border-gray-300 px-2 py-1"
                />
            </div>
            
            <label className="mb-6 text-xl font-bold">
                Please answer the following questions about the quality of your sleep:
            </label>
            <div className="w-full overflow-x-auto">
                <div className="min-w-180 mx-auto">

                    <div className="grid grid-cols-7 border-b-2 border-gray-300">
                        <div />
                        {options.map((option) => (
                            <div
                                key={option}
                                className="px-2 pb-3 text-center text-m font-semibold"
                            >
                                {option}
                            </div>
                        ))}
                    </div>

                    {questions.map((question, questionIndex) => (
                        <div
                            key={question}
                            className="grid grid-cols-7 items-center py-3"
                        >
                            <div className="pr-4">
                                {question}
                            </div>

                            {options.map((option, optionIndex) => (
                                <div
                                    key={option}
                                    className="flex justify-center"
                                >
                                    <input
                                        type="radio"
                                        name={`question-${questionIndex}`}
                                        value={optionIndex}
                                        checked={
                                            answers[questionIndex] === optionIndex
                                        }
                                        onChange={() =>
                                            selectAnswer(
                                                questionIndex,
                                                optionIndex
                                            )
                                        }
                                        aria-label={option}
                                        className="h-5 w-5"
                                    />
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            </div>

            <button
                type="button"
                onClick={finishQuestionnaire}
                className="mt-6 rounded bg-green-600 px-4 py-2 text-white"
            >
                Finish
            </button>

            {questionnaireMessage && (
                <p className="mt-4">
                    {questionnaireMessage}
                </p>
            )}
        </section>
    );
}
