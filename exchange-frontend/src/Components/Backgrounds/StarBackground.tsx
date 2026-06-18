import { useEffect, useState } from "react";
import { Meteors } from "../Animations/Meteors/Meteors";
import { generateMeteors } from "../Animations/Meteors/generateMeteors";
import { Stars } from "../Animations/Stars/Stars";
import { generateStars } from "../Animations/Stars/generateStars";
import { type IStar } from "../../Interfaces/Animations/IStar";
import { type IMeteor } from "../../Interfaces/Animations/IMeteor";
import type { Dispatch, SetStateAction } from "react";

export const StarBackground = () => {
  const { stars, meteors } = useStarBackground();
  return (
    <div className="fixed inset-0 overflow-hidden pointer-events-none z-0">
      <Stars stars={stars} />
      <Meteors meteors={meteors} />
    </div>
  );
};

function useStarBackground() {
  const [stars, setStars] = useState<IStar[]>([]);
  const [meteors, setMeteors] = useState<IMeteor[]>([]);
  useEffect(() => bindSkyResize(setStars, setMeteors), []);
  return { stars, meteors };
}

function bindSkyResize(setStars: StarSetter, setMeteors: MeteorSetter) {
  refreshSky(setStars, setMeteors);
  const handleResize = () => refreshSky(setStars, setMeteors);
  window.addEventListener("resize", handleResize);
  return () => window.removeEventListener("resize", handleResize);
}

function refreshSky(setStars: StarSetter, setMeteors: MeteorSetter) {
  setStars(generateStars());
  setMeteors(generateMeteors());
}

type StarSetter = Dispatch<SetStateAction<IStar[]>>;
type MeteorSetter = Dispatch<SetStateAction<IMeteor[]>>;
