import { deepStrictEqual, equal } from 'node:assert';
import test from 'node:test';
import { cardDescription, cardNumber, gameLabel, metaValues, placeholder, providerPriceRow, sourceUrl } from '../src/lib/tcgPresentation.ts';

test('One Piece presentation exposes provider metadata and prices', () => {
  equal(gameLabel('one-piece'), 'One Piece');
  equal(placeholder('one-piece'), 'Monkey.D.Luffy, Roronoa Zoro, Nami');
  equal(cardNumber(onePieceDetail), 'OP03-070');
  equal(cardDescription(onePieceDetail), 'Leader card');
  equal(sourceUrl(onePieceDetail), 'https://example.test/product/453022');
  deepStrictEqual(metaValues(onePieceDetail), ['Pillars of Strength', 'R', 'Purple', '7000 power']);
  deepStrictEqual(providerPriceRow(onePieceDetail.prices[0]), {
    key: 'tcgplayer-USD',
    label: 'Tcgplayer',
    values: ['USD', 'Market $0.31', 'Low $0.15', 'High $2.50'],
  });
});

const onePieceDetail = {
  id: '1024', name: 'Monkey.D.Luffy', description: 'Leader card', setName: 'Pillars of Strength',
  code: 'OP03-070', cardNumber: '070', rarity: 'R', color: 'Purple', power: '7000',
  smallImageUrl: 'small.png', largeImageUrl: 'large.png', tcgPlayerUrl: 'https://example.test/product/453022',
  source: 'APITCG', updatedAt: '2026-06-01T08:30:00.000Z', marketPrice: 0.31,
  prices: [{ market: 'tcgplayer', currency: 'USD', low: 0.15, mid: 0.35, high: 2.5, marketPrice: 0.31 }],
};
