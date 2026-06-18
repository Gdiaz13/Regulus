import { type IStar } from "../../../Interfaces/Animations/IStar";
import styles from "./Stars.module.css";

export const Stars = ({ stars }: { stars: IStar[] }) => (
    <>
        {stars.map((star) => (
            <div
                key={star.id}
                className={`${styles.star} ${styles.starsPulseSubtle}`}
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
