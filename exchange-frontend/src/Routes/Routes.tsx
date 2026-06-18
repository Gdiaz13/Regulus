
import HomePage from "../Pages/HomePage/HomePage";
import SearchPage from "../Pages/SearchPage/SearchPage";
import { createBrowserRouter, Navigate } from "react-router";
import App from "../App";
import CompanyPage from "../Pages/CompanyPage/CompanyPage";
import CompanyProfile from "../Components/CompanyProfile/CompanyProfile";
import IncomeStatement from "../Components/IncomeStatement/IncomeStatement";
import BalanceSheet from "../Components/BalanceSheet/BalanceSheet";
import CashFlowStatement from "../Components/CashFlowStatement/CashFlowStatement";
import PortfolioPage from "../Pages/PortfolioPage/PortfolioPage";
import NotFoundPage from "../Pages/NotFoundPage/NotFoundPage";

export const router = createBrowserRouter([
    {
        path: "/",
        element:<App />,
        children: [
            { path: "", element: <HomePage /> },
            { path: "search", element: <SearchPage /> },
            { path: "portfolio", element: <PortfolioPage /> },
            { path: "company/:ticker", 
                element: <CompanyPage />, 
                children: [
                    { index: true, element: <Navigate to="company-profile" replace /> },
                    { path: "company-profile", element: <CompanyProfile /> },
                    { path: "income-statement", element: <IncomeStatement /> },
                    { path: "balance-sheet", element: <BalanceSheet /> },
                    { path: "cashflow-statement", element: <CashFlowStatement /> }
                ]
            },
            { path: "*", element: <NotFoundPage /> },
        ]
    },
]);
