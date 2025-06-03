import { StarBackground } from "../../Components/Backgrounds/StarBackground";
import Hero from "../../Components/Hero/Hero";


const HomePage = () => {
    return (
    <div className ="min-h-screen bg-background text-foreground overflow-x-hidden">

    <StarBackground />
    <Hero />
    </div>
    );
};

export default HomePage