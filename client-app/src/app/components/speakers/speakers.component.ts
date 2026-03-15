import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CampService, Speaker } from '../../services/camp.service';

@Component({
  selector: 'app-speakers',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  template: `
    <div class="speakers-container">
      <div class="speakers-header">
        <h1 class="page-title">
          <mat-icon>people</mat-icon>
          Speakers
        </h1>
        <span class="speaker-count" *ngIf="!loading">{{ speakers.length }} speaker(s)</span>
      </div>

      <div *ngIf="loading" class="loading-spinner">
        <mat-spinner diameter="50"></mat-spinner>
      </div>

      <div *ngIf="!loading && speakers.length === 0" class="no-speakers">
        <mat-icon class="no-icon">person_off</mat-icon>
        <p>No speakers found.</p>
      </div>

      <div *ngIf="!loading && speakers.length > 0" class="speakers-grid">
        <mat-card *ngFor="let speaker of speakers" class="speaker-card">
          <mat-card-content>
            <div class="speaker-avatar">
              <mat-icon class="avatar-icon">account_circle</mat-icon>
            </div>
            <div class="speaker-name">
              {{ speaker.firstName }}
              <span *ngIf="speaker.middleName"> {{ speaker.middleName }}</span>
              {{ speaker.lastName }}
            </div>
            <div class="speaker-company" *ngIf="speaker.company">
              <mat-icon class="company-icon">business</mat-icon>
              {{ speaker.company }}
            </div>
            <div class="speaker-links">
              <a *ngIf="speaker.twitter" [href]="'https://twitter.com/' + speaker.twitter"
                 target="_blank" mat-icon-button [matTooltip]="'Twitter: @' + speaker.twitter">
                <mat-icon>alternate_email</mat-icon>
              </a>
              <a *ngIf="speaker.gitHub" [href]="'https://github.com/' + speaker.gitHub"
                 target="_blank" mat-icon-button [matTooltip]="'GitHub: ' + speaker.gitHub">
                <mat-icon>code</mat-icon>
              </a>
              <a *ngIf="speaker.blogUrl" [href]="speaker.blogUrl"
                 target="_blank" mat-icon-button matTooltip="Blog">
                <mat-icon>article</mat-icon>
              </a>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .speakers-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .speakers-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .page-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      font-size: 28px;
      font-weight: 600;
      color: #1565c0;
    }

    .speaker-count {
      background: #e3f2fd;
      color: #1565c0;
      padding: 4px 12px;
      border-radius: 16px;
      font-size: 14px;
      font-weight: 500;
    }

    .loading-spinner {
      display: flex;
      justify-content: center;
      padding: 48px;
    }

    .no-speakers {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 60px 20px;
      color: #78909c;
    }

    .no-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
    }

    .speakers-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 20px;
    }

    .speaker-card {
      border-radius: 12px !important;
      transition: transform 0.2s, box-shadow 0.2s;
    }

    .speaker-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(21, 101, 192, 0.18) !important;
    }

    .speaker-avatar {
      display: flex;
      justify-content: center;
      margin-bottom: 12px;
    }

    .avatar-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      color: #90caf9;
    }

    .speaker-name {
      font-size: 18px;
      font-weight: 600;
      color: #0d47a1;
      text-align: center;
      margin-bottom: 6px;
    }

    .speaker-company {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      font-size: 13px;
      color: #546e7a;
      margin-bottom: 12px;
    }

    .company-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .speaker-links {
      display: flex;
      justify-content: center;
      gap: 4px;
    }

    .speaker-links a {
      color: #1976d2;
    }
  `]
})
export class SpeakersComponent implements OnInit {
  speakers: Speaker[] = [];
  loading = false;

  constructor(private campService: CampService) {}

  ngOnInit(): void {
    this.loading = true;
    this.campService.getSpeakers().subscribe({
      next: (speakers) => {
        this.speakers = speakers;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
