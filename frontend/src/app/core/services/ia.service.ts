import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class IaService {
  private readonly apiUrl = 'http://localhost:5002/api/ia';

  constructor(private http: HttpClient) {}

  obterSugestoes(): Observable<{ sugestao: string }> {
    return this.http.get<{ sugestao: string }>(`${this.apiUrl}/sugestoes`);
  }
}