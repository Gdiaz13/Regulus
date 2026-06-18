export type ConfigValue = string | number | null | undefined;

export interface DataConfig<T> {
  label: string;
  subTitle?: string;
  isCurrency?: boolean;
  render: (row: T) => ConfigValue;
}

export function formatConfigValue(value: ConfigValue, isCurrency?: boolean): string {
  if (value === null || value === undefined || value === "") {
    return "N/A";
  }

  if (isCurrency) {
    return typeof value === "number" ? `$${value.toLocaleString()}` : `$${value}`;
  }

  return typeof value === "number" ? value.toLocaleString() : value;
}
