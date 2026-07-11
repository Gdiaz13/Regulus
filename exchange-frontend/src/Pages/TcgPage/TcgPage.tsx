import type { FormEvent } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PriceChart from '../../Components/PriceChart/PriceChart';
import type { IPriceHistory, IPricePoint } from '../../Interfaces/APIResponses/IPriceHistory';
import type { IMagicCardDetail, IMagicCardPrice } from '../../Interfaces/APIResponses/IMagicCard';
import type { IPokemonCardPrice } from '../../Interfaces/APIResponses/IPokemonCard';
import { useTcgCards } from '../../hooks/useTcgCards';
import type { TcgCardDetail, TcgCardSummary, TcgGame } from '../../hooks/useTcgCards';
import styles from './TcgPage.module.css';

type Cards = ReturnType<typeof useTcgCards>;
type CardPrice = IPokemonCardPrice | IMagicCardPrice;

const TcgPage = () => {
  const cards = useTcgCards();
  return (
    <main className={styles.page}>
      <Header />
      <SearchPanel cards={cards} />
      <section className={styles.layout}>
        <ResultsPanel cards={cards} />
        <DetailPanel cards={cards} />
      </section>
    </main>
  );
};

function Header() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>TCG markets</p>
      <h1 className={styles.title}>Card detail and price context.</h1>
      <p className={styles.note}>Search Pokemon and Magic cards through Regulas.Api, then read stored prices from the same history foundation.</p>
    </header>
  );
}

function SearchPanel({ cards }: { cards: Cards }) {
  return (
    <form className={styles.searchPanel} onSubmit={(event) => submit(event, cards)}>
      <GameTabs game={cards.game} onSelect={cards.setGame} />
      <input className={styles.searchInput} value={cards.query} placeholder={placeholder(cards.game)} onChange={(event) => cards.setQuery(event.target.value)} />
      <button type="submit" disabled={!canSearch(cards)}>Search cards</button>
    </form>
  );
}

function GameTabs({ game, onSelect }: { game: TcgGame; onSelect: (game: TcgGame) => void }) {
  return <div className={styles.gameTabs}>{games.map((item) => <GameButton key={item} value={item} game={game} onSelect={onSelect} />)}</div>;
}

function GameButton({ value, game, onSelect }: { value: TcgGame; game: TcgGame; onSelect: (game: TcgGame) => void }) {
  return <button className={gameClass(value === game)} type="button" onClick={() => onSelect(value)}>{gameLabel(value)}</button>;
}

function submit(event: FormEvent<HTMLFormElement>, cards: Cards) {
  event.preventDefault();
  void cards.search();
}

function canSearch(cards: Cards) {
  return cards.query.trim().length > 0 && cards.searchStatus !== 'loading';
}

function ResultsPanel({ cards }: { cards: Cards }) {
  if (cards.searchStatus === 'idle') {
    return <p className={styles.hint}>Enter a {gameLabel(cards.game)} card name to start.</p>;
  }
  if (cards.searchStatus !== 'success') {
    return <ResourceStatus status={cards.searchStatus} message={cards.searchMessage} />;
  }
  return <CardResults cards={cards.results} selectedId={cards.selected?.id} onSelect={cards.select} />;
}

function CardResults({ cards, selectedId, onSelect }: CardResultsProps) {
  return <div className={styles.resultsPanel}>{cards.map((card) => <CardButton key={card.id} card={card} selected={card.id === selectedId} onSelect={onSelect} />)}</div>;
}

function CardButton({ card, selected, onSelect }: CardButtonProps) {
  return (
    <button className={cardClass(selected)} type="button" onClick={() => void onSelect(card.id)}>
      <CardThumb card={card} />
      <span className={styles.cardText}><strong>{card.name}</strong><span>{card.setName ?? 'Unknown set'} #{cardNumber(card)}</span></span>
      <span className={styles.price}>{summaryPrice(card)}</span>
    </button>
  );
}

function CardThumb({ card }: { card: TcgCardSummary }) {
  return card.smallImageUrl ? <img src={card.smallImageUrl} alt="" className={styles.thumb} /> : <span className={styles.thumbFallback} />;
}

function DetailPanel({ cards }: { cards: Cards }) {
  if (cards.detailStatus === 'idle') {
    return <p className={styles.hint}>Search results will open a card detail view here.</p>;
  }
  if (cards.detailStatus !== 'success' || !cards.selected) {
    return <ResourceStatus status={cards.detailStatus} message={cards.detailMessage} />;
  }
  return <CardDetail card={cards.selected} history={cards.history} historyStatus={cards.historyStatus} historyMessage={cards.historyMessage} />;
}

function CardDetail({ card, history, historyStatus, historyMessage }: DetailProps) {
  return (
    <article className={styles.detailPanel}>
      <DetailHeader card={card} />
      <OracleText card={card} />
      <PriceTable prices={card.prices} />
      <StoredHistory history={history} status={historyStatus} message={historyMessage} />
    </article>
  );
}

function DetailHeader({ card }: { card: TcgCardDetail }) {
  return <div className={styles.detailHeader}>{card.largeImageUrl ? <img src={card.largeImageUrl} alt={card.name} className={styles.cardImage} /> : null}<div><h2>{card.name}</h2><MetaList card={card} /><SourceLink card={card} /></div></div>;
}

function MetaList({ card }: { card: TcgCardDetail }) {
  return <p className={styles.meta}>{metaValues(card).join(' | ')}</p>;
}

function OracleText({ card }: { card: TcgCardDetail }) {
  return isMagicDetail(card) && card.oracleText ? <p className={styles.oracleText}>{card.oracleText}</p> : null;
}

function SourceLink({ card }: { card: TcgCardDetail }) {
  const url = sourceUrl(card);
  return url ? <a className={styles.sourceLink} href={url} target="_blank" rel="noreferrer">Source {card.source}</a> : <p className={styles.source}>Source {card.source}</p>;
}

function PriceTable({ prices }: { prices: CardPrice[] }) {
  if (prices.length === 0) {
    return <p className={styles.hintCompact}>No provider prices returned for this card.</p>;
  }
  return <table className={styles.table}><tbody>{prices.map((price) => <PriceRow key={priceKey(price)} price={price} />)}</tbody></table>;
}

function PriceRow({ price }: { price: CardPrice }) {
  return isMagicPrice(price) ? <MagicPriceRow price={price} /> : <PokemonPriceRow price={price} />;
}

function PokemonPriceRow({ price }: { price: IPokemonCardPrice }) {
  return <tr><th>{label(price.variant)}</th><td>Market {money(price.market)}</td><td>Low {money(price.low)}</td><td>High {money(price.high)}</td></tr>;
}

function MagicPriceRow({ price }: { price: IMagicCardPrice }) {
  return <tr><th>{label(price.finish)}</th><td>{price.currency.toUpperCase()}</td><td>Market {providerMoney(price.marketPrice, price.currency)}</td><td /></tr>;
}

function StoredHistory({ history, status, message }: HistoryProps) {
  if (status !== 'success' || !history) {
    return <ResourceStatus status={status} message={message} />;
  }
  return <HistoryLoaded history={history} />;
}

function HistoryLoaded({ history }: { history: IPriceHistory }) {
  const latest = latestPoint(history);
  return <section className={styles.historyPanel}><HistorySummary history={history} latest={latest} /><PriceChart points={history.points} /></section>;
}

function HistorySummary({ history, latest }: { history: IPriceHistory; latest: IPricePoint }) {
  return <div className={styles.historySummary}><strong>Stored prices</strong><span>{history.count} points | Latest {providerMoney(latest.close, latest.currency)} | Source {latest.source}</span><span>{metadata(latest)}</span></div>;
}

function metaValues(card: TcgCardDetail) {
  return isMagicDetail(card) ? [card.setName, card.rarity, card.typeLine, card.manaCost, card.colors.join(', ')] : [card.setName, card.rarity, card.types.join(', '), card.hp ? `${card.hp} HP` : null].filter(Boolean);
}

function sourceUrl(card: TcgCardDetail) {
  return isMagicDetail(card) ? card.scryfallUrl : card.tcgPlayerUrl;
}

function cardNumber(card: TcgCardSummary) {
  return 'collectorNumber' in card ? card.collectorNumber ?? 'n/a' : card.number ?? 'n/a';
}

function summaryPrice(card: TcgCardSummary) {
  return 'marketCurrency' in card ? providerMoney(card.marketPrice, card.marketCurrency) : money(card.marketPrice);
}

function priceKey(price: CardPrice) {
  return isMagicPrice(price) ? `${price.currency}-${price.finish}` : price.variant;
}

function latestPoint(history: IPriceHistory) {
  return history.points[history.points.length - 1];
}

function metadata(point: IPricePoint) {
  return [point.priceType, point.cardCondition, point.grade, point.currency].filter(Boolean).join(' | ') || 'No card-specific metadata';
}

function isMagicDetail(card: TcgCardDetail): card is IMagicCardDetail {
  return 'typeLine' in card;
}

function isMagicPrice(price: CardPrice): price is IMagicCardPrice {
  return 'finish' in price;
}

function gameClass(selected: boolean) {
  return `${styles.gameTab} ${selected ? styles.gameTabActive : ''}`;
}

function cardClass(selected: boolean) {
  return `${styles.cardButton} ${selected ? styles.cardButtonSelected : ''}`;
}

function label(value: string) {
  return value.replace(/([A-Z])/g, ' $1').replace(/^./, (text) => text.toUpperCase());
}

function money(value: number | null) {
  return providerMoney(value, 'USD');
}

function providerMoney(value: number | null, currency: string | null) {
  if (value === null) {
    return 'n/a';
  }
  return currencyAmount(value, currency);
}

function currencyAmount(value: number, currency: string | null) {
  const code = (currency ?? 'USD').toUpperCase();
  if (code === 'USD') {
    return `$${value.toFixed(2)}`;
  }
  return code === 'EUR' ? `€${value.toFixed(2)}` : `${value.toFixed(2)} ${code}`;
}

function gameLabel(game: TcgGame) {
  return game === 'pokemon' ? 'Pokemon' : 'Magic';
}

function placeholder(game: TcgGame) {
  return game === 'pokemon' ? 'Charizard, Pikachu, Moonbreon' : 'Lightning Bolt, Black Lotus, Ragavan';
}

const games: TcgGame[] = ['pokemon', 'magic'];

type CardResultsProps = {
  cards: TcgCardSummary[];
  selectedId?: string;
  onSelect: (id: string) => void;
};

type CardButtonProps = {
  card: TcgCardSummary;
  selected: boolean;
  onSelect: (id: string) => void;
};

type DetailProps = {
  card: TcgCardDetail;
  history: IPriceHistory | null;
  historyStatus: Cards['historyStatus'];
  historyMessage: string | null;
};

type HistoryProps = {
  history: IPriceHistory | null;
  status: Cards['historyStatus'];
  message: string | null;
};

export default TcgPage;
