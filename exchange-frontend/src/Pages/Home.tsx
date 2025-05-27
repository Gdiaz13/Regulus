import { ThemeToggle } from "../Components/Theme/ThemeToggle";
import { StarBackground } from "../Components/Background/StarBackground";


export const Home = () => {
    return (
    <div className ="min-h-screen bg-background text-foreground overflow-x-hidden">
    <ThemeToggle />

    <StarBackground />
    </div>
    );
};