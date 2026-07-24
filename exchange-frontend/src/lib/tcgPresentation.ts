import type { IMagicCardDetail, IMagicCardPrice } from '../Interfaces/APIResponses/IMagicCard';
import type { IOnePieceCardDetail, IOnePieceCardPrice } from '../Interfaces/APIResponses/IOnePieceCard';
import type { IPokemonCardPrice } from '../Interfaces/APIResponses/IPokemonCard';
import type { TcgCardDetail, TcgCardSummary, TcgGame } from '../hooks/useTcgCards';

export type TcgProviderPrice = IPokemonCardPrice | IMagicCardPrice | IOnePieceCardPrice;
export type ProviderPriceRow = { key: string; label: string; values: string[] };

export function gameLabel(game: TcgGame) {
  if (game === 'pokemon') return 'Pokemon';
  return game === 'magic' ? 'Magic' : 'One Piece';
}

export function placeholder(game: TcgGame) {
  if (game === 'pokemon') return 'Charizard, Pikachu, Moonbreon';
  return game === 'magic' ? 'Lightning Bolt, Black Lotus, Ragavan' : 'Monkey.D.Luffy, Roronoa Zoro, Nami';
}

export function cardNumber(card: TcgCardSummary) {
  if ('collectorNumber' in card) return card.collectorNumber ?? 'n/a';
  if ('code' in card) return card.code ?? 'n/a';
  return card.number ?? 'n/a';
}

export function cardDescription(card: TcgCardDetail) {
  if (isMagicDetail(card)) return card.oracleText;
  return isOnePieceDetail(card) ? card.description : null;
}

export function metaValues(card: TcgCardDetail) {
  if (isMagicDetail(card)) return compact([card.setName, card.rarity, card.typeLine, card.manaCost, card.colors.join(', ')]);
  if (isOnePieceDetail(card)) return compact([card.setName, card.rarity, card.color, card.power ? `${card.power} power` : null]);
  return compact([card.setName, card.rarity, card.types.join(', '), card.hp ? `${card.hp} HP` : null]);
}

export function sourceUrl(card: TcgCardDetail) {
  if (isMagicDetail(card)) return card.scryfallUrl;
  return card.tcgPlayerUrl;
}

export function providerPriceRow(price: TcgProviderPrice): ProviderPriceRow {
  if (isMagicPrice(price)) return magicPriceRow(price);
  return isOnePiecePrice(price) ? onePiecePriceRow(price) : pokemonPriceRow(price);
}

function magicPriceRow(price: IMagicCardPrice): ProviderPriceRow {
  return { key: `${price.currency}-${price.finish}`, label: label(price.finish), values: [price.currency.toUpperCase(), `Market ${providerMoney(price.marketPrice, price.currency)}`] };
}

function onePiecePriceRow(price: IOnePieceCardPrice): ProviderPriceRow {
  return { key: `${price.market}-${price.currency}`, label: label(price.market), values: [price.currency.toUpperCase(), `Market ${providerMoney(price.marketPrice, price.currency)}`, `Low ${providerMoney(price.low, price.currency)}`, `High ${providerMoney(price.high, price.currency)}`] };
}

function pokemonPriceRow(price: IPokemonCardPrice): ProviderPriceRow {
  return { key: price.variant, label: label(price.variant), values: [`Market ${money(price.market)}`, `Low ${money(price.low)}`, `High ${money(price.high)}`] };
}

export function providerMoney(value: number | null, currency: string | null) {
  if (value === null) return 'n/a';
  const code = (currency ?? 'USD').toUpperCase();
  if (code === 'USD') return `$${value.toFixed(2)}`;
  return code === 'EUR' ? `€${value.toFixed(2)}` : `${value.toFixed(2)} ${code}`;
}

function money(value: number | null) {
  return providerMoney(value, 'USD');
}

function label(value: string) {
  return value.replace(/([A-Z])/g, ' $1').replace(/^./, (text) => text.toUpperCase());
}

function compact(values: Array<string | null | undefined>) {
  return values.filter((value): value is string => Boolean(value));
}

function isMagicDetail(card: TcgCardDetail): card is IMagicCardDetail {
  return 'typeLine' in card;
}

function isOnePieceDetail(card: TcgCardDetail): card is IOnePieceCardDetail {
  return 'power' in card;
}

function isMagicPrice(price: TcgProviderPrice): price is IMagicCardPrice {
  return 'finish' in price;
}

function isOnePiecePrice(price: TcgProviderPrice): price is IOnePieceCardPrice {
  return 'market' in price && typeof price.market === 'string';
}
