import { lazy, Suspense } from "react";
import type { ReactNode } from "react";
import { createBrowserRouter, Navigate } from "react-router-dom";
import App from "../App";
import ResourceStatus from "../Components/AsyncResource/ResourceStatus";

const HomePage = lazy(() => import("../Pages/HomePage/HomePage"));
const SearchPage = lazy(() => import("../Pages/SearchPage/SearchPage"));
const PortfolioPage = lazy(() => import("../Pages/PortfolioPage/PortfolioPage"));
const PredictionsPage = lazy(() => import("../Pages/PredictionsPage/PredictionsPage"));
const PriceHistoryPage = lazy(() => import("../Pages/PriceHistoryPage/PriceHistoryPage"));
const CompanyPage = lazy(() => import("../Pages/CompanyPage/CompanyPage"));
const CompanyProfile = lazy(() => import("../Components/CompanyProfile/CompanyProfile"));
const IncomeStatement = lazy(() => import("../Components/IncomeStatement/IncomeStatement"));
const BalanceSheet = lazy(() => import("../Components/BalanceSheet/BalanceSheet"));
const CashFlowStatement = lazy(() => import("../Components/CashFlowStatement/CashFlowStatement"));
const NotFoundPage = lazy(() => import("../Pages/NotFoundPage/NotFoundPage"));
const routeFallback = <ResourceStatus status="loading" message={null} />;

// Route components load on demand so the first bundle stays small.
export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { path: "", element: page(<HomePage />) },
      { path: "search", element: page(<SearchPage />) },
      { path: "portfolio", element: page(<PortfolioPage />) },
      { path: "predictions", element: page(<PredictionsPage />) },
      { path: "price-history", element: page(<PriceHistoryPage />) },
      {
        path: "company/:ticker",
        element: page(<CompanyPage />),
        children: [
          { index: true, element: <Navigate to="company-profile" replace /> },
          { path: "company-profile", element: page(<CompanyProfile />) },
          { path: "income-statement", element: page(<IncomeStatement />) },
          { path: "balance-sheet", element: page(<BalanceSheet />) },
          { path: "cashflow-statement", element: page(<CashFlowStatement />) },
        ],
      },
      { path: "*", element: page(<NotFoundPage />) },
    ],
  },
]);

function page(element: ReactNode) {
  return (
    <Suspense fallback={routeFallback}>
      {element}
    </Suspense>
  );
}
