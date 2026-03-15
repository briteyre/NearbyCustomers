import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar class="app-toolbar" color="primary">
      <mat-icon class="app-logo">emoji_events</mat-icon>
      <span class="app-name">NearbyCustomers</span>
      <span class="spacer"></span>
      <a mat-button routerLink="/calendar" routerLinkActive="active-link">
        <mat-icon>calendar_today</mat-icon>
        Calendar
      </a>
      <a mat-button routerLink="/speakers" routerLinkActive="active-link">
        <mat-icon>people</mat-icon>
        Speakers
      </a>
    </mat-toolbar>
    <main class="app-content">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .app-toolbar {
      background: linear-gradient(135deg, #1565c0 0%, #1976d2 50%, #42a5f5 100%) !important;
      color: white;
      box-shadow: 0 2px 8px rgba(21, 101, 192, 0.4);
    }

    .app-logo {
      margin-right: 8px;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .app-name {
      font-size: 20px;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .spacer {
      flex: 1;
    }

    .app-content {
      min-height: calc(100vh - 64px);
      background: #f5f9ff;
    }

    a[mat-button] {
      color: rgba(255, 255, 255, 0.85);
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 4px;
    }

    a[mat-button]:hover, .active-link {
      color: white !important;
      background: rgba(255, 255, 255, 0.15) !important;
      border-radius: 4px;
    }
  `]
})
export class AppComponent {
  title = 'NearbyCustomers';
}
