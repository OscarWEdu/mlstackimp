import { useEffect, useState } from "react";
import "./../style.css";

async function getBackendResponse(): Promise<string> {
    const response = await fetch("/api");

    if (!response.ok) {
        throw new Error(`Backend returned ${response.status}`);
    }

    return await response.text();
}

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
        }, []
    );

    const [answers, setAnswers] = useState<(number | null)[]>(
        Array(questions.length).fill(null)
    );

    const [questionnaireMessage, setQuestionnaireMessage] = useState("");

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

        const numericAnswers: number[] = answers.filter(
            (answer): answer is number => answer !== null
        );

        const total = numericAnswers.reduce(
            (sum, answer) => sum + answer,
            0
        );

        const average = total / numericAnswers.length;

        try {
            const response = await fetch("/api/predict", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    sleepQualityIndex: average,
                }),
            });

            if (!response.ok) {
                throw new Error(`Backend returned ${response.status}`);
            }

            const data: { sleepQualityIndex: number } =
                await response.json();

            setQuestionnaireMessage(
                `Sleep Quality Index: ${data.sleepQualityIndex.toFixed(2)}`
            );
        } catch (error) {
            setQuestionnaireMessage(`Backend error: ${error}`);
            console.error(error);
        }
    }

    return (
        <section id="center">
            <p id="backend-response">{message}</p>
            <hr className="my-6" />

            <h1 className="mb-6 text-l font-bold">
                Sleep Quality Index
            </h1>

            <div className="w-full overflow-x-auto">
                <div className="min-w-200">

                    <div className="grid grid-cols-7 border-b-2 border-gray-300">
                        <div />

                        {options.map((option) => (
                            <div
                                key={option}
                                className="px-2 pb-3 text-center text-sm font-semibold"
                            >
                                {option}
                            </div>
                        ))}
                    </div>

                    {questions.map((question, questionIndex) => (
                        <div
                            key={question}
                            className="grid grid-cols-7 items-center py-4"
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
                Finish questionnaire
            </button>

            {questionnaireMessage && (
                <p className="mt-4">
                    {questionnaireMessage}
                </p>
            )}
        </section>
    );
}
