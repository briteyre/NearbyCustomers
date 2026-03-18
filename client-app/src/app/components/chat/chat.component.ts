import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LlmService } from '../../services/llm.service';

@Component({
  selector: 'app-chat',
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
            <mat-icon>chat</mat-icon>
            Assistant Chat
          </mat-card-title>
        </mat-card-header>

        <mat-card-content>
          <div class="messages-list">
            <div *ngFor="let m of messages" class="message-row" [class.user]="m.sender === 'user'" [class.assistant]="m.sender === 'assistant'">
              <div class="message-bubble">
                <div class="message-header">
                  <mat-icon class="message-icon">{{ m.sender === 'user' ? 'person' : 'smart_toy' }}</mat-icon>
                  <span class="message-sender">{{ m.sender === 'user' ? 'You' : 'Assistant' }}</span>
                </div>
                <div class="message-text">{{ m.text }}</div>
              </div>
            </div>
          </div>
        </mat-card-content>

        <mat-card-actions class="chat-actions">
          <mat-form-field class="chat-input" appearance="outline">
            <input matInput placeholder="Ask a question..." [(ngModel)]="input" (keyup.enter)="sendMessage()" />
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="sendMessage()" [disabled]="loading || !input.trim()">
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
    .messages-list { max-height: 420px; overflow: auto; display: flex; flex-direction: column; gap: 10px; padding: 6px 2px; }
    .message-row { display: flex; }
    .message-row.user { justify-content: flex-end; }
    .message-row.assistant { justify-content: flex-start; }
    .message-bubble { max-width: 85%; border-radius: 12px; padding: 10px 12px; border: 1px solid #dbe5f1; background: #f8fbff; }
    .message-row.user .message-bubble { background: #e8f1ff; border-color: #c4dafd; }
    .message-row.assistant .message-bubble { background: #f5f7fa; border-color: #d8dee8; }
    .message-header { display: flex; align-items: center; gap: 6px; margin-bottom: 4px; }
    .message-icon { width: 18px; height: 18px; font-size: 18px; color: #1565c0; }
    .message-sender { font-weight: 600; font-size: 12px; color: #1565c0; }
    .message-text { white-space: pre-wrap; overflow-wrap: anywhere; line-height: 1.4; }
    .chat-actions { display: flex; gap: 12px; align-items: center; padding: 16px; }
    .chat-input { flex: 1; }
  `]
})
export class ChatComponent {
  input = '';
  messages: { sender: 'user' | 'assistant'; text: string }[] = [];
  loading = false;

  constructor(private llm: LlmService) {}

  sendMessage(): void {
    const text = this.input?.trim();
    if (!text) return;
    this.messages.push({ sender: 'user', text });
    this.input = '';
    this.loading = true;

    this.llm.chat(text).subscribe({
      next: (res) => {
        console.log('LLM raw response:', res);
        const answer = this.extractAnswer(res);
        this.messages.push({ sender: 'assistant', text: answer });
        this.loading = false;
      },
      error: (err) => {
        console.error('LLM request error:', err);
        this.messages.push({ sender: 'assistant', text: 'Error: failed to get response from server.' });
        this.loading = false;
      }
    });
  }

  // Try several common response shapes and fall back to a sensible message.
  private extractAnswer(res: any): string {
    if (!res) return 'No response';
    if (typeof res === 'string') {
      // Handle APIs that return a JSON string payload.
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
    if (res.body && typeof res.body.reply === 'string') return res.body.reply;
    if (res.body && typeof res.body.answer === 'string') return res.body.answer;
    if (typeof res.data === 'string') return res.data;
    if (res.data && typeof res.data.reply === 'string') return res.data.reply;
    if (res.data && typeof res.data.answer === 'string') return res.data.answer;
    // OpenAI-style responses
    if (res.choices && Array.isArray(res.choices) && res.choices.length) {
      const c = res.choices[0];
      if (typeof c.text === 'string') return c.text;
      if (c.message && typeof c.message.content === 'string') return c.message.content;
    }
    // Other nested outputs
    if (res.output && Array.isArray(res.output) && res.output.length) {
      return res.output.map((o: any) => o.text || o.content || JSON.stringify(o)).join('\n');
    }
    return 'No response';
  }
}
