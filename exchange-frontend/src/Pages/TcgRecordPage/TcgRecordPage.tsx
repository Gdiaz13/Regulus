import { useState } from 'react';
import { Link } from 'react-router-dom';
import { getPriceHistory, recordManualPrice } from '../../API/priceHistoryClient';
import type { ICaptureResult, IManualPriceRequest, IPriceHistory, IPricePoint } from '../../Interfaces/APIResponses/IPriceHistory';
import styles from './TcgRecordPage.module.css';

const priceTypes = ['Sold', 'Listed', 'Market'];
const tcgGames = ['Pokemon', 'Magic', 'One Piece'];

type TcgForm = { symbol: string; name: string; category: string; date: string; price: string; priceType: string; condition: string; grade: string; currency: string };
type SetText = (value: string) => void;
type EntryState = { message: string; history: IPriceHistory | null; busy: boolean };
type ApplyEntry = (state: EntryState) => void;

// Record TCG card prices by hand and review what is stored. Complements the
// /tcg browse page: the gateway keeps the source metadata so sold/graded
// prices never mix silently with raw listings.
const TcgRecordPage = () => {
  const { form, update } = useTcgForm();
  const [entry, setEntry] = useState<EntryState>({ message: '', history: null, busy: false });
  return (
    <main className={styles.page}>
      <Header />
      <EntryForm form={form} update={update} busy={entry.busy} onSubmit={() => submitEntry(form, setEntry)} onLoad={() => loadHistory(form, setEntry)} />
      {entry.message && <p className={styles.message}>{entry.message}</p>}
      {entry.history && <HistoryTable history={entry.history} />}
    </main>
  );
};

function useTcgForm() {
  const [form, setForm] = useState<TcgForm>(emptyForm());
  const update = (key: keyof TcgForm): SetText => (value) => setForm((current) => ({ ...current, [key]: value }));
  return { form, update };
}

function emptyForm(): TcgForm {
  return { symbol: '', name: '', category: 'Pokemon', date: today(), price: '', priceType: 'Sold', condition: '', grade: '', currency: 'USD' };
}

function today() {
  return new Date().toISOString().slice(0, 10);
}

async function submitEntry(form: TcgForm, apply: ApplyEntry) {
  const price = Number(form.price);
  if (!form.symbol.trim() || !(price > 0)) {
    apply({ message: 'Enter a card code and a positive price.', history: null, busy: false });
    return;
  }
  apply({ message: '', history: null, busy: true });
  const result = await recordManualPrice(form.symbol.trim(), toRequest(form, price));
  await applyEntryResult(form, result.ok ? entryMessage(result.data) : result.message, apply);
}

async function loadHistory(form: TcgForm, apply: ApplyEntry) {
  if (!form.symbol.trim()) {
    apply({ message: 'Enter a card code to load its stored prices.', history: null, busy: false });
    return;
  }
  apply({ message: '', history: null, busy: true });
  await applyEntryResult(form, '', apply);
}

// Both paths end by re-reading storage, so the table always shows saved truth.
async function applyEntryResult(form: TcgForm, message: string, apply: ApplyEntry) {
  const history = await getPriceHistory(form.symbol.trim(), 'TcgCard', 365);
  apply({ message: history.ok ? message : history.message, history: history.ok ? history.data : null, busy: false });
}

function toRequest(form: TcgForm, price: number): IManualPriceRequest {
  return {
    date: form.date,
    price,
    priceType: form.priceType,
    cardCondition: form.condition.trim() || null,
    grade: form.grade.trim() || null,
    currency: form.currency.trim() || null,
    name: form.name.trim() || null,
    category: form.category.trim() || null,
  };
}

function entryMessage(result: ICaptureResult) {
  return result.captured > 0
    ? `${result.symbol} price saved (source ${result.source}).`
    : `${result.symbol} already has a price for that date; nothing was added.`;
}

function Header() {
  return (
    <header className={styles.header}>
      <p className={styles.eyebrow}>TCG cards</p>
      <h1 className={styles.title}>Record card prices by hand.</h1>
      <p className={styles.note}>Record Pokemon, Magic, and One Piece prices by hand. Type, condition, grade, currency, and game are stored so listings stay comparable. <Link to="/tcg">Browse Pokemon cards</Link> for card details and stored charts.</p>
    </header>
  );
}

function EntryForm({ form, update, busy, onSubmit, onLoad }: EntryFormProps) {
  return (
    <div className={styles.form}>
      <CardInputs form={form} update={update} />
      <MetadataInputs form={form} update={update} />
      <div className={styles.actions}>
        <button onClick={onSubmit} disabled={busy}>Save price</button>
        <button onClick={onLoad} disabled={busy}>Load stored</button>
      </div>
    </div>
  );
}

function CardInputs({ form, update }: FieldProps) {
  return (
    <div className={styles.row}>
      <input value={form.symbol} maxLength={32} placeholder="Card code e.g. SV3-125" onChange={(event) => update('symbol')(event.target.value)} />
      <input value={form.name} placeholder="Card name (optional)" onChange={(event) => update('name')(event.target.value)} />
      <select value={form.category} onChange={(event) => update('category')(event.target.value)} aria-label="Card game">
        {tcgGames.map((game) => <option key={game} value={game}>{game}</option>)}
      </select>
      <input type="date" value={form.date} onChange={(event) => update('date')(event.target.value)} />
      <input value={form.price} inputMode="decimal" placeholder="Price e.g. 120.50" onChange={(event) => update('price')(event.target.value)} />
    </div>
  );
}

function MetadataInputs({ form, update }: FieldProps) {
  return (
    <div className={styles.row}>
      <select value={form.priceType} onChange={(event) => update('priceType')(event.target.value)} aria-label="Price type">
        {priceTypes.map((type) => <option key={type} value={type}>{type}</option>)}
      </select>
      <input value={form.condition} placeholder="Condition e.g. Near Mint" onChange={(event) => update('condition')(event.target.value)} />
      <input value={form.grade} placeholder="Grade e.g. PSA 9" onChange={(event) => update('grade')(event.target.value)} />
      <input value={form.currency} maxLength={8} placeholder="Currency" onChange={(event) => update('currency')(event.target.value)} />
    </div>
  );
}

function HistoryTable({ history }: { history: IPriceHistory }) {
  const newestFirst = [...history.points].reverse();
  return (
    <section className={styles.results}>
      <p className={styles.summary}>{history.symbol} · {history.count} stored point(s)</p>
      <table className={styles.table}>
        <thead>
          <tr><th>Date</th><th>Price</th><th>Type</th><th>Condition</th><th>Grade</th><th>Currency</th><th>Source</th></tr>
        </thead>
        <tbody>{newestFirst.map((point) => <HistoryRow key={point.date} point={point} />)}</tbody>
      </table>
    </section>
  );
}

function HistoryRow({ point }: { point: IPricePoint }) {
  return (
    <tr>
      <td>{point.date}</td>
      <td>{point.close.toFixed(2)}</td>
      <td>{point.priceType ?? '-'}</td>
      <td>{point.cardCondition ?? '-'}</td>
      <td>{point.grade ?? '-'}</td>
      <td>{point.currency ?? '-'}</td>
      <td>{point.source}</td>
    </tr>
  );
}

type EntryFormProps = FieldProps & { busy: boolean; onSubmit: () => void; onLoad: () => void };
type FieldProps = { form: TcgForm; update: (key: keyof TcgForm) => SetText };

export default TcgRecordPage;
