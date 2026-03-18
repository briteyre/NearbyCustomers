import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatRequest {
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class LlmService {
  private apiBase = '/api';

  constructor(private http: HttpClient) {}

  // Use a permissive `any` response type because backend shapes may vary.
  chat(message: string): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/llm/chat`, { message });
  }
}
