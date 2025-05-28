import { ThemeToggle } from "../Components/Theme/ThemeToggle";
import { StarBackground } from "../Components/Backgrounds/StarBackground";


export const Home = () => {
    return (
    <div className ="min-h-screen bg-background text-foreground overflow-x-hidden">
    <ThemeToggle />

    <StarBackground />
    </div>
    );
};