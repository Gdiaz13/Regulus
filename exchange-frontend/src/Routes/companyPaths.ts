const companyProfileTab = 'company-profile';

// Company links start on the profile tab; Routes also redirects bare tickers here.
export function companyProfilePath(symbol: string) {
  return `/company/${encodeTicker(symbol)}/${companyProfileTab}`;
}

function encodeTicker(symbol: string) {
  return encodeURIComponent(symbol.trim().toUpperCase());
}
