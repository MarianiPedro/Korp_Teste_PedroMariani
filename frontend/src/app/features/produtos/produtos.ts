import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ProdutoService } from '../../core/services/produto.service';
import { Produto } from '../../shared/models/produto.model';

@Component({
  selector: 'app-produtos',
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
    MatDialogModule
  ],
  templateUrl: './produtos.html',
  styleUrl: './produtos.scss'
})
export class ProdutosComponent implements OnInit, OnDestroy {
  produtos: Produto[] = [];
  displayedColumns = ['codigo', 'descricao', 'saldo', 'acoes'];
  form: FormGroup;
  editandoId: number | null = null;
  carregando = false;
  salvando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private produtoService: ProdutoService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      codigo: ['', [Validators.required, Validators.maxLength(50)]],
      descricao: ['', [Validators.required, Validators.maxLength(200)]],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.carregarProdutos();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarProdutos(): void {
    this.carregando = true;
    this.produtoService.listar()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (produtos) => {
          this.produtos = produtos;
          this.carregando = false;
        },
        error: () => {
          this.snackBar.open('Erro ao carregar produtos.', 'Fechar', { duration: 3000 });
          this.carregando = false;
        }
      });
  }

  salvar(): void {
    if (this.form.invalid) return;
    this.salvando = true;

    const request = this.form.value;
    const operacao = this.editandoId
      ? this.produtoService.atualizar(this.editandoId, request)
      : this.produtoService.criar(request);

    operacao.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.snackBar.open(
          this.editandoId ? 'Produto atualizado!' : 'Produto criado!',
          'Fechar',
          { duration: 3000 }
        );
        this.resetForm();
        this.carregarProdutos();
        this.salvando = false;
      },
      error: () => {
        this.snackBar.open('Erro ao salvar produto.', 'Fechar', { duration: 3000 });
        this.salvando = false;
      }
    });
  }

  editar(produto: Produto): void {
    this.editandoId = produto.id;
    this.form.patchValue({
      codigo: produto.codigo,
      descricao: produto.descricao,
      saldo: produto.saldo
    });
  }

  deletar(id: number): void {
    if (!confirm('Deseja excluir este produto?')) return;
    this.produtoService.deletar(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.snackBar.open('Produto excluído!', 'Fechar', { duration: 3000 });
          this.carregarProdutos();
        },
        error: () => {
          this.snackBar.open('Erro ao excluir produto.', 'Fechar', { duration: 3000 })
        }
      });
  }

  resetForm(): void {
    this.form.reset({ saldo: 0 });
    this.editandoId = null;
  }
}