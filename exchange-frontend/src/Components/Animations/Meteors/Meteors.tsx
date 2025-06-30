import { type IMeteor } from "../../../Interfaces/Animations/IMeteor";
import styles from "./Meteors.module.css";

export const generateMeteors = (): IMeteor[] => {
  const numberOfMeteors = 5;
  const newMeteors: IMeteor[] = [];
  for (let i = 0; i < numberOfMeteors; i++) {
    newMeteors.push({
      id: i,
      size: Math.random() * 2 + 1,
      x: Math.random() * 50,
      y: Math.random() * 50 , 
      delay: Math.random() * 15,
      animationDuration: Math.random() * 3 + 3,
    });
  }
  return newMeteors;
};

export const Meteors = ({ meteors }: { meteors: IMeteor[] }) => (
    <>
        {meteors.map((meteor) => (
            <div
                key={meteor.id}
                className={`${styles.meteor} ${styles.meteorsAnimate}`}
                style={{
                    left: meteor.x + "%",
                    top: meteor.y + "%",
                    width: meteor.size  + "px",
                    height: meteor.size + "px",
                    animationDelay: `${meteor.delay}`,
                    animationDuration: `${meteor.animationDuration}s`,
                }}
            />
        ))}
    </>
);
