import { type IStar } from "../../../Interfaces/Animations/IStar";

export const generateStars = (): IStar[] => {
  const numberOfStars = starCount();
  const newStars: IStar[] = [];

  for (let i = 0; i < numberOfStars; i++) {
    newStars.push(createStar(i));
  }

  return newStars;
};

function starCount() {
  return Math.floor((window.innerWidth * window.innerHeight) / 10000);
}

function createStar(id: number): IStar {
  return {
    id,
    size: Math.random() * 3 + 1,
    x: Math.random() * 100,
    y: Math.random() * 100,
    opacity: Math.random() * 0.5 + 0.5,
    animationDuration: Math.random() * 4 + 2,
  };
}
