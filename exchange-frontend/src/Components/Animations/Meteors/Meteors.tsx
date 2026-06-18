import { type IMeteor } from "../../../Interfaces/Animations/IMeteor";
import styles from "./Meteors.module.css";

export const Meteors = ({ meteors }: { meteors: IMeteor[] }) => (
  <>{meteors.map(renderMeteor)}</>
);

function renderMeteor(meteor: IMeteor) {
  return (
    <div
      key={meteor.id}
      className={`${styles.meteor} ${styles.meteorsAnimate}`}
      style={meteorStyle(meteor)}
    />
  );
}

function meteorStyle(meteor: IMeteor) {
  return {
    left: meteor.x + "%",
    top: meteor.y + "%",
    width: meteor.size + "px",
    height: meteor.size + "px",
    animationDelay: `${meteor.delay}`,
    animationDuration: `${meteor.animationDuration}s`,
  };
}
