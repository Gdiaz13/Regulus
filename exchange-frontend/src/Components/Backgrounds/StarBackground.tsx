import { useEffect, useState } from "react";
import { Meteors } from "../Animations/Meteors/Meteors";
import { generateMeteors } from "../Animations/Meteors/generateMeteors";
import { Stars } from "../Animations/Stars/Stars";
import { generateStars } from "../Animations/Stars/generateStars";
import { type IStar } from "../../Interfaces/Animations/IStar";
import { type IMeteor } from "../../Interfaces/Animations/IMeteor";

export const StarBackground = () => {
  const [stars, setStars] = useState<IStar[]>([]);
  const [meteors, setMeteors] = useState<IMeteor[]>([]);

  useEffect(() => {
    setStars(generateStars());
    setMeteors(generateMeteors());

    const handleResize = () => {
      setStars(generateStars());
      setMeteors(generateMeteors());
    };

    window.addEventListener("resize", handleResize);

    // Regenerate meteors after their animation duration
    // const meteorInterval = setInterval(() => {
    //   setMeteors(generateMeteors());
    // }, 4000); 

    return () => {
      window.removeEventListener("resize", handleResize);
    //   clearInterval(meteorInterval);
    };
  }, []);

  return (
    <div className="fixed inset-0 overflow-hidden pointer-events-none z-0">
      <Stars stars={stars} />
      <Meteors meteors={meteors} />
    </div>
  );
};
