import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { NotaFiscalService } from '../../core/services/notaFiscal.service';
import { ProdutoService } from '../../core/services/produto.service';
import { NotaFiscal, StatusNota } from '../../shared/models/notaFiscal.model';
import { Produto } from '../../shared/models/produto.model';
import { IaService } from '../../core/services/ia.service';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-notas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatSelectModule,
    MatChipsModule,
    MatExpansionModule,
    MatProgressBarModule
  ],
  templateUrl: './notas.html',
  styleUrl: './notas.scss'
})
export class NotasComponent implements OnInit, OnDestroy {
  notas: NotaFiscal[] = [];
  produtos: Produto[] = [];
  displayedColumns = ['numero', 'status', 'dataCriacao', 'itens', 'acoes'];
  form: FormGroup;
  carregando = false;
  salvando = false;
  imprimindo: number | null = null;
  StatusNota = StatusNota;
  sugestaoIa: string = '';
  carregandoIa = false;
  private destroy$ = new Subject<void>();

  constructor(
    private notaService: NotaFiscalService,
    private produtoService: ProdutoService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private iaService: IaService
  ) {
    this.form = this.fb.group({
      itens: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.carregarNotas();
    this.carregarProdutos();
    this.carregarSugestaoIa();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get itens(): FormArray {
    return this.form.get('itens') as FormArray;
  }

  novoItem(): FormGroup {
    return this.fb.group({
      produtoId: [null, Validators.required],
      produtoDescricao: [''],
      quantidade: [1, [Validators.required, Validators.min(1)]]
    });
  }

  adicionarItem(): void {
    this.itens.push(this.novoItem());
  }

  removerItem(index: number): void {
    this.itens.removeAt(index);
  }

  onProdutoSelecionado(index: number): void {
    const item = this.itens.at(index);
    const produtoId = item.get('produtoId')?.value;
    const produto = this.produtos.find(p => p.id === produtoId);
    if (produto) {
      item.get('produtoDescricao')?.setValue(produto.descricao);
    }
  }

  carregarNotas(): void {
    this.carregando = true;
    this.notaService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notas) => {
          this.notas = notas;
          this.carregando = false;
        },
        error: () => {
          this.snackBar.open('Erro ao carregar notas fiscais.', 'Fechar', { duration: 3000 });
          this.carregando = false;
        }
      });
  }

  carregarProdutos(): void {
    this.produtoService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (produtos) => this.produtos = produtos,
        error: () => this.snackBar.open('Erro ao carregar produtos.', 'Fechar', { duration: 3000 })
      });
  }

  salvar(): void {
    if (this.form.invalid || this.itens.length === 0) {
      this.snackBar.open('Adicione ao menos um item à nota.', 'Fechar', { duration: 3000 });
      return;
    }
    this.salvando = true;

    this.notaService.criar(this.form.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Nota fiscal criada!', 'Fechar', { duration: 3000 });
          this.itens.clear();
          this.carregarNotas();
          this.salvando = false;
        },
        error: () => {
          this.snackBar.open('Erro ao criar nota fiscal.', 'Fechar', { duration: 3000 });
          this.salvando = false;
        }
      });
  }

  imprimir(nota: NotaFiscal): void {
    if (nota.status !== StatusNota.Aberta) {
      this.snackBar.open('Apenas notas abertas podem ser impressas.', 'Fechar', { duration: 3000 });
      return;
    }
    this.imprimindo = nota.id;

    this.notaService.imprimir(nota.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.snackBar.open(res.mensagem, 'Fechar', { duration: 3000 });
          this.carregarNotas();
          this.imprimindo = null;
        },
        error: (err) => {
          const mensagem = err.error?.mensagem ?? 'Erro ao imprimir nota.';
          this.snackBar.open(mensagem, 'Fechar', { duration: 4000 });
          this.imprimindo = null;
        }
      });
  }

  statusLabel(status: StatusNota): string {
    return status === StatusNota.Aberta ? 'Aberta' : 'Fechada';
  }

  carregarSugestaoIa(): void {
  this.carregandoIa = true;
  this.iaService.obterSugestoes()
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (res) => {
        this.sugestaoIa = res.sugestao;
        this.carregandoIa = false;
      },
      error: () => {
        this.sugestaoIa = 'Sugestão de IA indisponível no momento.';
        this.carregandoIa = false;
      }
    });
  }
}