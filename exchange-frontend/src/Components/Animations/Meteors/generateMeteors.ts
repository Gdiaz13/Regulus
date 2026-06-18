import { type IMeteor } from "../../../Interfaces/Animations/IMeteor";

export const generateMeteors = (): IMeteor[] => {
  const numberOfMeteors = 5;
  const newMeteors: IMeteor[] = [];

  for (let i = 0; i < numberOfMeteors; i++) {
    newMeteors.push(createMeteor(i));
  }

  return newMeteors;
};

function createMeteor(id: number): IMeteor {
  return {
    id,
    size: Math.random() * 2 + 1,
    x: Math.random() * 50,
    y: Math.random() * 50,
    delay: Math.random() * 15,
    animationDuration: Math.random() * 3 + 3,
  };
}
