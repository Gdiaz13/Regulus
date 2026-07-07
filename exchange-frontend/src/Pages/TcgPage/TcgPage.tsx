import type { FormEvent } from 'react';
import ResourceStatus from '../../Components/AsyncResource/ResourceStatus';
import PriceChart from '../../Components/PriceChart/PriceChart';
import type { IPriceHistory, IPricePoint } from '../../Interfaces/APIResponses/IPriceHistory';
import type { IPokemonCardDetail, IPokemonCardPrice, IPokemonCardSummary } from '../../Interfaces/APIResponses/IPokemonCard';
import { usePokemonCards } from '../../hooks/usePokemonCards';
import styles from './TcgPage.module.css';

type Cards = ReturnType<typeof usePokemonCards>;

const TcgPage = () => {
  const cards = usePokemonCards();
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
      <p className={styles.eyebrow}>Pokemon TCG</p>
      <h1 className={styles.title}>Card detail and price context.</h1>
      <p className={styles.note}>Searches run through Regulas.Api, then stored card prices load from the same price-history foundation as stocks.</p>
    </header>
  );
}

function SearchPanel({ cards }: { cards: Cards }) {
  return (
    <form className={styles.searchPanel} onSubmit={(event) => submit(event, cards)}>
      <input className={styles.searchInput} value={cards.query} placeholder="Charizard, Pikachu, Moonbreon" onChange={(event) => cards.setQuery(event.target.value)} />
      <button type="submit" disabled={!canSearch(cards)}>Search cards</button>
    </form>
  );
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
    return <p className={styles.hint}>Enter a Pokemon card name to start.</p>;
  }
  if (cards.searchStatus !== 'success') {
    return <ResourceStatus status={cards.searchStatus} message={cards.searchMessage} />;
  }
  return <CardResults cards={cards.results} selectedId={cards.selected?.id} onSelect={cards.select} />;
}

function CardResults({ cards, selectedId, onSelect }: CardResultsProps) {
  return (
    <div className={styles.resultsPanel}>
      {cards.map((card) => <CardButton key={card.id} card={card} selected={card.id === selectedId} onSelect={onSelect} />)}
    </div>
  );
}

function CardButton({ card, selected, onSelect }: CardButtonProps) {
  return (
    <button className={cardClass(selected)} type="button" onClick={() => void onSelect(card.id)}>
      <CardThumb card={card} />
      <span className={styles.cardText}><strong>{card.name}</strong><span>{card.setName ?? 'Unknown set'} #{card.number ?? 'n/a'}</span></span>
      <span className={styles.price}>{money(card.marketPrice)}</span>
    </button>
  );
}

function CardThumb({ card }: { card: IPokemonCardSummary }) {
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
      <PriceTable prices={card.prices} />
      <StoredHistory history={history} status={historyStatus} message={historyMessage} />
    </article>
  );
}

function DetailHeader({ card }: { card: IPokemonCardDetail }) {
  return (
    <div className={styles.detailHeader}>
      {card.largeImageUrl ? <img src={card.largeImageUrl} alt={card.name} className={styles.cardImage} /> : null}
      <div><h2>{card.name}</h2><MetaList card={card} /><SourceLink card={card} /></div>
    </div>
  );
}

function MetaList({ card }: { card: IPokemonCardDetail }) {
  const values = [card.setName, card.rarity, card.types.join(', '), card.hp ? `${card.hp} HP` : null].filter(Boolean);
  return <p className={styles.meta}>{values.join(' | ')}</p>;
}

function SourceLink({ card }: { card: IPokemonCardDetail }) {
  if (!card.tcgPlayerUrl) {
    return <p className={styles.source}>Source {card.source}</p>;
  }
  return <a className={styles.sourceLink} href={card.tcgPlayerUrl} target="_blank" rel="noreferrer">Source {card.source}</a>;
}

function PriceTable({ prices }: { prices: IPokemonCardPrice[] }) {
  if (prices.length === 0) {
    return <p className={styles.hintCompact}>No provider prices returned for this card.</p>;
  }
  return <table className={styles.table}><tbody>{prices.map((price) => <PriceRow key={price.variant} price={price} />)}</tbody></table>;
}

function PriceRow({ price }: { price: IPokemonCardPrice }) {
  return (
    <tr>
      <th>{label(price.variant)}</th>
      <td>Market {money(price.market)}</td>
      <td>Low {money(price.low)}</td>
      <td>High {money(price.high)}</td>
    </tr>
  );
}

function StoredHistory({ history, status, message }: HistoryProps) {
  if (status !== 'success' || !history) {
    return <ResourceStatus status={status} message={message} />;
  }
  return <HistoryLoaded history={history} />;
}

function HistoryLoaded({ history }: { history: IPriceHistory }) {
  const latest = latestPoint(history);
  return (
    <section className={styles.historyPanel}>
      <HistorySummary history={history} latest={latest} />
      <PriceChart points={history.points} />
    </section>
  );
}

function HistorySummary({ history, latest }: { history: IPriceHistory; latest: IPricePoint }) {
  return (
    <div className={styles.historySummary}>
      <strong>Stored prices</strong>
      <span>{history.count} points | Latest {money(latest.close)} | Source {latest.source}</span>
      <span>{metadata(latest)}</span>
    </div>
  );
}

function latestPoint(history: IPriceHistory) {
  return history.points[history.points.length - 1];
}

function metadata(point: IPricePoint) {
  return [point.priceType, point.cardCondition, point.grade, point.currency].filter(Boolean).join(' | ') || 'No card-specific metadata';
}

function cardClass(selected: boolean) {
  return `${styles.cardButton} ${selected ? styles.cardButtonSelected : ''}`;
}

function label(value: string) {
  return value.replace(/([A-Z])/g, ' $1').replace(/^./, (text) => text.toUpperCase());
}

function money(value: number | null) {
  return value === null ? 'n/a' : `$${value.toFixed(2)}`;
}

type CardResultsProps = {
  cards: IPokemonCardSummary[];
  selectedId?: string;
  onSelect: (id: string) => void;
};

type CardButtonProps = {
  card: IPokemonCardSummary;
  selected: boolean;
  onSelect: (id: string) => void;
};

type DetailProps = {
  card: IPokemonCardDetail;
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
