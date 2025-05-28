import { type IStar } from "../../Interfaces/IStar";

export const generateStars = (): IStar[] => {
    const numberOfStars = Math.floor(
        (window.innerWidth * window.innerHeight) / 10000
    );
    const newStars: IStar[] = [];
    for (let i = 0; i < numberOfStars; i++) {
        newStars.push({
            id: i,
            size: Math.random() * 3 + 1,
            x: Math.random() * 100,
            y: Math.random() * 100,
            opacity: Math.random() * 0.5 + 0.5,
            animationDuration: Math.random() * 4 + 2,
        });
    }
    return newStars;
};

export const Stars = ({ stars }: { stars: IStar[] }) => (
    <>
        {stars.map((star) => (
            <div
                key={star.id}
                className="star animate-pulse-subtle"
                style={{
                    width: star.size + "px",
                    height: star.size + "px",
                    left: star.x + "%",
                    top: star.y + "%",
                    opacity: star.opacity,
                    animationDuration: star.animationDuration + "s",
                    position: "absolute",
                }}
            />
        ))}
    </>
);