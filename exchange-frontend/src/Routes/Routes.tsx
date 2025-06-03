
import HomePage from "../Pages/HomePage/HomePage";
import SearchPage from "../Pages/SearchPage/SearchPage";
import { createBrowserRouter } from "react-router";
import App from "../App";
import CompanyPage from "../Pages/CompanyPage/CompanyPage";

export const router = createBrowserRouter([
    {
        path: "/",
        element:<App />,
        children: [
            { path: "", element: <HomePage /> },
            { path: "search", element: <SearchPage /> },
            { path: "company/:ticker", element: <CompanyPage /> },

        ]
    },
]);