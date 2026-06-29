import { useState } from 'react';
import type { ChangeEvent, FormEvent, ReactNode } from 'react';
import type { IPredictAsset } from '../../../Interfaces/APIResponses/IPrediction';
import { assetTypes } from '../../../lib/assetTypes';
import styles from './PredictionForm.module.css';

type FormState = ReturnType<typeof useFormState>;
type SetText = (value: string) => void;

// Collects one asset and hands it to the page to stage for a prediction run.
export default function PredictionForm({ onAdd }: { onAdd: (asset: IPredictAsset) => void }) {
  const form = useFormState(onAdd);
  return (
    <form className={styles.form} onSubmit={form.submit}>
      <FieldRow form={form} />
      <button type="submit" disabled={!canAdd(form.symbol, form.price)}>Add asset</button>
    </form>
  );
}

function useFormState(onAdd: (asset: IPredictAsset) => void) {
  const [symbol, setSymbol] = useState('');
  const [assetType, setAssetType] = useState('Stock');
  const [category, setCategory] = useState('');
  const [price, setPrice] = useState('');
  const submit = makeSubmit({ symbol, assetType, category, price }, onAdd, () => resetFields(setSymbol, setCategory, setPrice));
  return { symbol, setSymbol, assetType, setAssetType, category, setCategory, price, setPrice, submit };
}

function makeSubmit(values: RawValues, onAdd: (asset: IPredictAsset) => void, reset: () => void) {
  return (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!canAdd(values.symbol, values.price)) {
      return;
    }
    onAdd(buildAsset(values));
    reset();
  };
}

function resetFields(setSymbol: SetText, setCategory: SetText, setPrice: SetText) {
  setSymbol('');
  setCategory('');
  setPrice('');
}

function FieldRow({ form }: { form: FormState }) {
  return (
    <div className={styles.fields}>
      <Labeled label="Symbol"><input value={form.symbol} maxLength={32} onChange={onText(form.setSymbol)} /></Labeled>
      <Labeled label="Type"><TypeSelect value={form.assetType} onChange={form.setAssetType} /></Labeled>
      <Labeled label="Category"><input value={form.category} placeholder="Technology" onChange={onText(form.setCategory)} /></Labeled>
      <Labeled label="Current price"><input type="number" min="0" step="0.01" value={form.price} onChange={onText(form.setPrice)} /></Labeled>
    </div>
  );
}

function Labeled({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className={styles.field}>
      <span>{label}</span>
      {children}
    </label>
  );
}

function TypeSelect({ value, onChange }: { value: string; onChange: SetText }) {
  return (
    <select value={value} onChange={onText(onChange)}>
      {assetTypes.map((type) => <option key={type} value={type}>{type}</option>)}
    </select>
  );
}

function onText(setter: SetText) {
  return (event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => setter(event.target.value);
}

function canAdd(symbol: string, price: string) {
  return symbol.trim().length > 0 && Number(price) > 0;
}

function buildAsset(values: RawValues): IPredictAsset {
  return {
    symbol: values.symbol.trim().toUpperCase(),
    assetType: values.assetType,
    category: values.category.trim() || undefined,
    currentPrice: Number(values.price),
  };
}

type RawValues = {
  symbol: string;
  assetType: string;
  category: string;
  price: string;
};
