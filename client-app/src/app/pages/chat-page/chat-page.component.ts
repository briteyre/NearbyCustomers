import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatComponent } from '../../components/chat/chat.component';
import { SkChatComponent } from '../../components/sk-chat/sk-chat.component';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [CommonModule, ChatComponent, SkChatComponent],
  template: `
    <div class="chats-container">
      <div class="chat-column">
        <app-chat></app-chat>
      </div>
      <div class="chat-column">
        <app-sk-chat></app-sk-chat>
      </div>
    </div>
  `,
  styles: [`
    .chats-container {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 24px;
      padding: 24px;
      min-height: calc(100vh - 64px);
      background: linear-gradient(135deg, #f5f9ff 0%, #f0f4ff 100%);
    }

    .chat-column {
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
    }

    @media (max-width: 1200px) {
      .chats-container {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ChatPageComponent {}
