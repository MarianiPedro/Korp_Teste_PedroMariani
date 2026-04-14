export interface ItemNota {
  id?: number;
  produtoId: number;
  produtoDescricao: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: number;
  numero: number;
  status: StatusNota;
  dataCriacao: string;
  itens: ItemNota[];
}

export enum StatusNota {
  Aberta = 1,
  Fechada = 2
}

export interface CriarNotaRequest {
  itens: ItemNota[];
}