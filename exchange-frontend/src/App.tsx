import { Outlet } from "react-router-dom";
import "./App.css";
import { AuthProvider } from "./Auth/AuthProvider";
import Navbar from "./Components/Navbar/Navbar";

function App() {
  return (
    <AuthProvider>
      <Navbar />
      <Outlet />
    </AuthProvider>
  );
}

export default App;
