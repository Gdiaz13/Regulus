import { type IMeteor } from "../../Interfaces/IMeteor";

export const generateMeteors = (): IMeteor[] => {
  const numberOfMeteors = 2;
  const newMeteors: IMeteor[] = [];
  for (let i = 0; i < numberOfMeteors; i++) {
    newMeteors.push({
      id: i,
      size: Math.random() * 2 + 1,
      x: Math.random() * 100,
      y: Math.random() * 100, 
      delay: Math.random() * 1,
      animationDuration: Math.random() * 3 + 3,
      angle: Math.random() * 120 - 60, // random angle between -60 and +60 degrees
    });
  }
  return newMeteors;
};

export const Meteors = ({ meteors }: { meteors: IMeteor[] }) => (
    <>
        {meteors.map((meteor) => (
            <div
                key={meteor.id}
                className="meteor animate-meteor"
                style={{
                    position: "absolute",
                    left: meteor.x + "%",
                    top: meteor.y + "%",
                    pointerEvents: "none",
                    width: meteor.size * 30 + "px",
                    height: meteor.size + "px",
                    transform: `rotate(${meteor.angle}deg)`,
                    animationDelay: `${meteor.delay}`,
                    animationDuration: `${meteor.animationDuration}s`,
                }}
            />
        ))}
    </>
);
