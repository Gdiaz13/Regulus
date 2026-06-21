export type ConfigValue = string | number | null | undefined;

// Configs say how one data row should appear in a table or ratio list.
export interface DataConfig<T> {
  label: string;
  subTitle?: string;
  isCurrency?: boolean;
  render: (row: T) => ConfigValue;
}

// Pages own row identity because each API response has different stable fields.
export type RowKey<T> = (row: T) => string;

export function formatConfigValue(value: ConfigValue, isCurrency?: boolean): string {
  if (value === null || value === undefined || value === "") {
    return "N/A";
  }

  if (isCurrency) {
    return typeof value === "number" ? `$${value.toLocaleString()}` : `$${value}`;
  }

  return typeof value === "number" ? value.toLocaleString() : value;
}
