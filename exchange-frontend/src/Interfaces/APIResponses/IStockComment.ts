export interface IStockComment {
  id: number;
  title: string;
  content: string;
  createdOn: string;
  stockId: number;
}

export interface CreateStockComment {
  title: string;
  content: string;
}
