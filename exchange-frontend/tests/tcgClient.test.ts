import { equal, fail, match } from 'node:assert';
import test from 'node:test';
import { getOnePieceCard, searchOnePieceCards } from '../src/API/tcgClient.ts';

test('searchOnePieceCards uses the Regulas gateway without a provider key', async () => {
  const originalFetch = globalThis.fetch;
  let requestedUrl = '';
  let requestedHeaders = new Headers();
  globalThis.fetch = (async (input: RequestInfo | URL, init?: RequestInit) => {
    requestedUrl = String(input);
    requestedHeaders = new Headers(init?.headers);
    return Response.json({ cards: [], page: 1, pageSize: 12, count: 0, totalCount: 0 });
  }) as typeof fetch;

  try {
    const result = await searchOnePieceCards('Monkey D. Luffy', 12);
    if (!result.ok) fail(result.message);
    equal(result.data.count, 0);
    equal(requestedUrl, '/api/tcg/one-piece/cards?query=Monkey+D.+Luffy&pageSize=12');
    equal(requestedHeaders.has('x-api-key'), false);
    match(requestedUrl, /^\/api\/tcg\/one-piece\/cards/);
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test('getOnePieceCard uses the encoded Regulas detail route', async () => {
  const originalFetch = globalThis.fetch;
  let requestedUrl = '';
  globalThis.fetch = (async (input: RequestInfo | URL) => {
    requestedUrl = String(input);
    return Response.json(onePieceDetail);
  }) as typeof fetch;

  try {
    const result = await getOnePieceCard('1024');
    if (!result.ok) fail(result.message);
    equal(requestedUrl, '/api/tcg/one-piece/cards/1024');
    equal(result.data.code, 'OP03-070');
    equal(result.data.prices[0]?.market, 'tcgplayer');
  } finally {
    globalThis.fetch = originalFetch;
  }
});

const onePieceDetail = {
  id: '1024',
  name: 'Monkey.D.Luffy',
  description: 'Leader card',
  setName: 'Pillars of Strength',
  code: 'OP03-070',
  cardNumber: '070',
  rarity: 'R',
  color: 'Purple',
  power: '7000',
  smallImageUrl: 'small.png',
  largeImageUrl: 'large.png',
  tcgPlayerUrl: 'https://example.test/product/453022',
  source: 'APITCG',
  updatedAt: '2026-06-01T08:30:00.000Z',
  marketPrice: 0.31,
  prices: [{ market: 'tcgplayer', currency: 'USD', low: 0.15, mid: 0.35, high: 2.5, marketPrice: 0.31 }],
};
