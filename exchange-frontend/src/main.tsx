import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
// import { searchCompanies } from './API/Api.tsx'

// // searchCompanies("AAPL").then(result => {
// //   console.log(result);
// // });

createRoot(document.getElementById('root') as HTMLElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
