import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LlmService } from '../../services/llm.service';

@Component({
  selector: 'app-sk-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="chat-container">
      <mat-card class="chat-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>auto_awesome</mat-icon>
            SK Assistant Chat
          </mat-card-title>
          <mat-card-subtitle>Powered by Semantic Kernel</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="messages-list">
            <div *ngFor="let m of messages" class="message-row" [class.user]="m.sender === 'user'" [class.assistant]="m.sender === 'assistant'">
              <div class="message-bubble">
                <div class="message-header">
                  <mat-icon class="message-icon">{{ m.sender === 'user' ? 'person' : 'smart_toy' }}</mat-icon>
                  <span class="message-sender">{{ m.sender === 'user' ? 'You' : 'SK Assistant' }}</span>
                </div>
                <div class="message-text">{{ m.text }}</div>
              </div>
            </div>
          </div>
        </mat-card-content>

        <mat-card-actions class="chat-actions">
          <mat-form-field class="chat-input" appearance="outline">
            <input matInput placeholder="Ask with Semantic Kernel..." [(ngModel)]="input" (keyup.enter)="sendMessage()" />
          </mat-form-field>
          <button mat-raised-button color="accent" (click)="sendMessage()" [disabled]="loading || !input.trim()">
            <mat-icon *ngIf="!loading">send</mat-icon>
            <mat-spinner *ngIf="loading" diameter="18"></mat-spinner>
            Send
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .chat-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    .chat-card { border-radius: 12px; }
    mat-card-subtitle { color: #7986cb; font-size: 12px; margin-top: 4px; }
    .messages-list { max-height: 420px; overflow: auto; display: flex; flex-direction: column; gap: 10px; padding: 6px 2px; }
    .message-row { display: flex; }
    .message-row.user { justify-content: flex-end; }
    .message-row.assistant { justify-content: flex-start; }
    .message-bubble { max-width: 85%; border-radius: 12px; padding: 10px 12px; border: 1px solid #e1bee7; background: #f3e5f5; }
    .message-row.user .message-bubble { background: #e8eaf6; border-color: #c5cae9; }
    .message-row.assistant .message-bubble { background: #f3e5f5; border-color: #e1bee7; }
    .message-header { display: flex; align-items: center; gap: 6px; margin-bottom: 4px; }
    .message-icon { width: 18px; height: 18px; font-size: 18px; color: #7b1fa2; }
    .message-sender { font-weight: 600; font-size: 12px; color: #7b1fa2; }
    .message-text { white-space: pre-wrap; overflow-wrap: anywhere; line-height: 1.4; }
    .chat-actions { display: flex; gap: 12px; align-items: center; padding: 16px; }
    .chat-input { flex: 1; }
  `]
})
export class SkChatComponent implements OnInit {
  input = '';
  messages: { sender: 'user' | 'assistant'; text: string }[] = [];
  loading = false;

  constructor(private llm: LlmService) {}

  ngOnInit(): void {
  }

  sendMessage(): void {
    const text = this.input?.trim();
    if (!text) return;
    this.messages.push({ sender: 'user', text });
    this.input = '';
    this.loading = true;

    this.llm.skChat(text).subscribe({
      next: (res) => {
        console.log('SK LLM raw response:', res);
        const answer = this.extractAnswer(res);
        this.messages.push({ sender: 'assistant', text: answer });
        this.loading = false;
      },
      error: (err) => {
        console.error('SK LLM request error:', err);
        this.messages.push({ sender: 'assistant', text: 'Error: failed to get response from Semantic Kernel.' });
        this.loading = false;
      }
    });
  }

  // Try several common response shapes and fall back to a sensible message.
  private extractAnswer(res: any): string {
    if (!res) return 'No response';
    if (typeof res === 'string') {
      try {
        const parsed = JSON.parse(res);
        if (typeof parsed.reply === 'string') return parsed.reply;
        if (typeof parsed.answer === 'string') return parsed.answer;
      } catch {
        return res;
      }
      return res;
    }
    if (typeof res.reply === 'string') return res.reply;
    if (typeof res.answer === 'string') return res.answer;
    if (typeof res.text === 'string') return res.text;
    return JSON.stringify(res);
  }
}
