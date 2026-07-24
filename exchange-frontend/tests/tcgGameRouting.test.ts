import { equal, fail } from 'node:assert';
import test from 'node:test';
import { gameLabel, getCardForGame, historySymbol, searchCardsForGame } from '../src/hooks/useTcgCards.ts';

test('One Piece hook routing uses the One Piece gateway and label', async () => {
  const originalFetch = globalThis.fetch;
  let requestedUrl = '';
  globalThis.fetch = (async (input: RequestInfo | URL) => {
    requestedUrl = String(input);
    return Response.json({ cards: [], page: 1, pageSize: 12, count: 0, totalCount: 0 });
  }) as typeof fetch;

  try {
    const result = await searchCardsForGame('one-piece', 'luffy');
    if (!result.ok) fail(result.message);
    equal(requestedUrl, '/api/tcg/one-piece/cards?query=luffy&pageSize=12');
    equal(gameLabel('one-piece'), 'One Piece');
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test('One Piece detail routing uses its provider id and card code for history', async () => {
  const originalFetch = globalThis.fetch;
  let requestedUrl = '';
  globalThis.fetch = (async (input: RequestInfo | URL) => {
    requestedUrl = String(input);
    return Response.json(onePieceDetail);
  }) as typeof fetch;

  try {
    const result = await getCardForGame('one-piece', '1024');
    if (!result.ok) fail(result.message);
    equal(requestedUrl, '/api/tcg/one-piece/cards/1024');
    equal(historySymbol('one-piece', result.data, '1024'), 'OP03-070');
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test('One Piece history falls back to the provider id for blank card codes', () => {
  const blankCodes = [null, '', '   '];
  for (const code of blankCodes) {
    equal(historySymbol('one-piece', { ...onePieceDetail, code }, '1024'), '1024');
  }
});

const onePieceDetail = {
  id: '1024', name: 'Monkey.D.Luffy', description: 'Leader card', setName: 'Pillars of Strength',
  code: 'OP03-070', cardNumber: '070', rarity: 'R', color: 'Purple', power: '7000',
  smallImageUrl: 'small.png', largeImageUrl: 'large.png', tcgPlayerUrl: null, source: 'APITCG',
  updatedAt: '2026-06-01T08:30:00.000Z', marketPrice: 0.31,
  prices: [{ market: 'tcgplayer', currency: 'USD', low: 0.15, mid: 0.35, high: 2.5, marketPrice: 0.31 }],
};
