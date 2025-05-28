import { useEffect } from "react";

export function onWindowResize(callback: () => void) {
  useEffect(() => {
    callback(); // Run once on mount
    const handleResize = () => callback();
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, [callback]);
}