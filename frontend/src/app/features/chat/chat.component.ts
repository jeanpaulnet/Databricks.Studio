import { Component, inject, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { ChatService } from '../../core/services/chat.service';
import { ChatMessage } from '../../shared/models/models';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatCardModule, MatInputModule, MatButtonModule,
    MatIconModule, MatProgressSpinnerModule, MatChipsModule
  ],
  template: `
    <div class="chat-container">
      <mat-card class="chat-card">
        <mat-card-header>
          <mat-card-title>Analytics Assistant</mat-card-title>
          <mat-card-subtitle>Ask questions about your analytics data</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <div class="messages" #messagesContainer>
            @if (messages().length === 0) {
              <div class="empty-state">
                <mat-icon>smart_toy</mat-icon>
                <p>Ask me anything about your analytics data.</p>
                <div class="quick-actions">
                  @for (q of quickQuestions; track q) {
                    <button mat-stroked-button (click)="sendQuick(q)">{{ q }}</button>
                  }
                </div>
              </div>
            }
            @for (msg of messages(); track $index) {
              <div class="message" [class.user]="msg.role === 'user'" [class.assistant]="msg.role === 'assistant'">
                <div class="bubble">{{ msg.content }}</div>
              </div>
            }
            @if (loading()) {
              <div class="message assistant">
                <div class="bubble typing">
                  <mat-spinner diameter="16" />
                  <span>Thinking...</span>
                </div>
              </div>
            }
          </div>
        </mat-card-content>

        <mat-card-actions class="input-area">
          <mat-form-field appearance="outline" class="input-field">
            <input matInput
              [(ngModel)]="input"
              placeholder="Ask about analytics count, total value, status breakdown..."
              (keydown.enter)="send()"
              [disabled]="loading()" />
          </mat-form-field>
          <button mat-fab color="primary" (click)="send()" [disabled]="loading() || !input.trim()">
            <mat-icon>send</mat-icon>
          </button>
        </mat-card-actions>
      </mat-card>
    </div>
  `,
  styles: [`
    .chat-container { max-width: 800px; margin: 0 auto; height: calc(100vh - 120px); display: flex; flex-direction: column; }
    .chat-card { display: flex; flex-direction: column; height: 100%; }
    mat-card-content { flex: 1; overflow: hidden; padding: 0 16px; }
    .messages { height: 100%; overflow-y: auto; display: flex; flex-direction: column; gap: 12px; padding: 16px 0; }
    .message { display: flex; }
    .message.user { justify-content: flex-end; }
    .message.assistant { justify-content: flex-start; }
    .bubble { max-width: 75%; padding: 10px 14px; border-radius: 18px; line-height: 1.5; white-space: pre-wrap; word-break: break-word; }
    .message.user .bubble { background: #1976d2; color: white; border-bottom-right-radius: 4px; }
    .message.assistant .bubble { background: #f5f5f5; color: #212121; border-bottom-left-radius: 4px; }
    .bubble.typing { display: flex; align-items: center; gap: 8px; color: #757575; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; color: #9e9e9e; gap: 16px; }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; }
    .quick-actions { display: flex; flex-wrap: wrap; gap: 8px; justify-content: center; }
    .quick-actions button { font-size: 12px; }
    .input-area { display: flex; align-items: center; gap: 8px; padding: 8px 16px 16px; }
    .input-field { flex: 1; }
  `]
})
export class ChatComponent implements AfterViewChecked {
  @ViewChild('messagesContainer') private messagesEl!: ElementRef;

  private readonly chatService = inject(ChatService);

  messages = signal<ChatMessage[]>([]);
  loading = signal(false);
  input = '';

  readonly quickQuestions = [
    'How many analytics are there?',
    'How many published?',
    'What is the Total Value of all published analytics?'
  ];

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  sendQuick(q: string) {
    this.input = q;
    this.send();
  }

  send() {
    const text = this.input.trim();
    if (!text || this.loading()) return;

    const history = [...this.messages()];
    this.messages.update(m => [...m, { role: 'user', content: text }]);
    this.input = '';
    this.loading.set(true);

    this.chatService.send({ message: text, history }).subscribe({
      next: (res) => {
        this.messages.update(m => [...m, { role: 'assistant', content: res.reply }]);
        this.loading.set(false);
      },
      error: () => {
        this.messages.update(m => [...m, { role: 'assistant', content: 'Sorry, I encountered an error. Please try again.' }]);
        this.loading.set(false);
      }
    });
  }

  private scrollToBottom() {
    try {
      const el = this.messagesEl?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    } catch {}
  }
}
