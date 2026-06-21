import { useEffect, useState } from 'react';
import type { ChangeEvent, Dispatch, FormEvent, SetStateAction } from 'react';
import type { CreatePortfolioStock, IPortfolioStock } from '../../../Interfaces/APIResponses/IPortfolioStock';
import { portfolioSymbolMaxLength } from '../portfolioRules';
import styles from './StockDetails.module.css';

type StockFieldName = keyof StockFieldState;

const stockFields = [
  { name: 'symbol', label: 'Ticker', maxLength: portfolioSymbolMaxLength },
  { name: 'companyName', label: 'Company name' },
  { name: 'purchasePrice', label: 'Purchase price', type: 'number' },
  { name: 'lastDividend', label: 'Last dividend', type: 'number' },
  { name: 'industry', label: 'Industry' },
  { name: 'marketCap', label: 'Market cap', type: 'number' },
] satisfies StockFieldConfig[];

export default function StockDetails({ stock, onUpdate }: Props) {
  const form = useStockDetailsForm(stock, onUpdate);
  return <StockDetailsForm {...form} />;
}

function useStockDetailsForm(stock: IPortfolioStock, onUpdate: UpdateStock) {
  const [fields, setFields] = useState(() => stockFieldState(stock));
  useEffect(() => setFields(stockFieldState(stock)), [stock]);
  return {
    fields,
    setField: fieldSetter(setFields),
    submit: submitStockDetails(stock.id, fields, onUpdate),
  };
}

function StockDetailsForm(props: StockDetailsFormProps) {
  return (
    <form className={styles.form} onSubmit={props.submit}>
      <div className={styles.grid}>{stockFields.map(renderStockField(props))}</div>
      <button type="submit" disabled={!props.fields.symbol.trim()}>Update details</button>
    </form>
  );
}

function renderStockField(props: StockDetailsFormProps) {
  return (field: StockFieldConfig) => <StockField key={field.name} field={field} form={props} />;
}

function StockField({ field, form }: StockFieldProps) {
  return (
    <label>
      <span>{field.label}</span>
      <input {...stockInputProps(field, form)} />
    </label>
  );
}

function stockInputProps(field: StockFieldConfig, form: StockDetailsFormProps) {
  return {
    maxLength: field.maxLength,
    min: numberMin(field),
    onChange: fieldChange(field, form),
    step: numberStep(field),
    type: field.type ?? 'text',
    value: form.fields[field.name],
  };
}

function numberStep(field: StockFieldConfig) {
  return field.type === 'number' ? 'any' : undefined;
}

function numberMin(field: StockFieldConfig) {
  return field.type === 'number' ? 0 : undefined;
}

function fieldChange(field: StockFieldConfig, form: StockDetailsFormProps) {
  return (event: ChangeEvent<HTMLInputElement>) => form.setField(field.name, event.target.value);
}

function fieldSetter(setFields: Dispatch<SetStateAction<StockFieldState>>) {
  return (name: StockFieldName, value: string) => setFields((fields) => ({ ...fields, [name]: value }));
}

function submitStockDetails(id: number, fields: StockFieldState, onUpdate: UpdateStock) {
  return (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void onUpdate(id, stockRequest(fields));
  };
}

function stockFieldState(stock: IPortfolioStock): StockFieldState {
  return {
    symbol: stock.symbol,
    companyName: stock.companyName,
    purchasePrice: String(stock.purchasePrice),
    lastDividend: String(stock.lastDividend),
    industry: stock.industry,
    marketCap: String(stock.marketCap),
  };
}

function stockRequest(fields: StockFieldState): CreatePortfolioStock {
  return {
    symbol: fields.symbol,
    companyName: fields.companyName,
    purchasePrice: numberField(fields.purchasePrice),
    lastDividend: numberField(fields.lastDividend),
    industry: fields.industry,
    marketCap: numberField(fields.marketCap),
  };
}

function numberField(value: string) {
  const parsed = Number(value);
  return validNumber(value, parsed) ? parsed : undefined;
}

function validNumber(value: string, parsed: number) {
  return value.trim().length > 0 && Number.isFinite(parsed);
}

type Props = {
  stock: IPortfolioStock;
  onUpdate: UpdateStock;
};

type UpdateStock = (id: number, stock: CreatePortfolioStock) => Promise<boolean>;

type StockFieldState = {
  symbol: string;
  companyName: string;
  purchasePrice: string;
  lastDividend: string;
  industry: string;
  marketCap: string;
};

type StockFieldConfig = {
  name: StockFieldName;
  label: string;
  maxLength?: number;
  type?: 'number';
};

type StockDetailsFormProps = {
  fields: StockFieldState;
  setField: (name: StockFieldName, value: string) => void;
  submit: (event: FormEvent<HTMLFormElement>) => void;
};

type StockFieldProps = {
  field: StockFieldConfig;
  form: StockDetailsFormProps;
};
