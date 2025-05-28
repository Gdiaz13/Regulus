import { type IStar } from "../../Interfaces/IStar";

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