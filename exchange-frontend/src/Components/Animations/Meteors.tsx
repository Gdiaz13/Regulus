import { type IMeteor } from "../../Interfaces/IMeteor";

export const Meteors = ({ meteors }: { meteors: IMeteor[] }) => (
    <>
        {meteors.map((meteor) => (
            <div
                key={meteor.id}
                className="meteor animate-meteor"
                style={{
                    width: meteor.size * 30 + "px",
                    height: meteor.size + "px",
                    left: meteor.x + "%",
                    top: meteor.y + "%",
                    animationDelay: `${meteor.delay}`,
                    animationDuration: `${meteor.animationDuration}s`,
                }}
            />
        ))}
    </>
);
