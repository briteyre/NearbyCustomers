import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CampService, CreateSpeakerRequest, Speaker } from '../../services/camp.service';
import { CreateSpeakerDialogComponent } from '../create-speaker-dialog/create-speaker-dialog.component';

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
    MatButtonModule,
    MatDialogModule,
    MatSnackBarModule,
  ],
  template: `
    <div class="speakers-container">
      <div class="speakers-header">
        <h1 class="page-title">
          <mat-icon>people</mat-icon>
          Speakers
        </h1>
        <div class="header-actions">
          <span class="speaker-count" *ngIf="!loading">{{ speakers.length }} speaker(s)</span>
          <button mat-raised-button color="primary" class="create-btn" (click)="openCreateSpeaker()">
            <mat-icon>person_add</mat-icon>
            Add Speaker
          </button>
        </div>
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
              <button mat-icon-button color="warn" (click)="onDeleteSpeaker(speaker)">
                <mat-icon>delete</mat-icon>
              </button>
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

    .header-actions {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .create-btn {
      font-size: 15px;
      padding: 8px 16px;
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

    @media (max-width: 768px) {
      .speakers-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .header-actions {
        width: 100%;
        justify-content: space-between;
      }
    }
  `]
})
export class SpeakersComponent implements OnInit {
  speakers: Speaker[] = [];
  loading = false;

  constructor(
    private campService: CampService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadSpeakers();
  }

  loadSpeakers(): void {
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

  openCreateSpeaker(): void {
    const dialogRef = this.dialog.open(CreateSpeakerDialogComponent, {
      width: '500px',
      disableClose: false,
    });

    dialogRef.afterClosed().subscribe((result: CreateSpeakerRequest | undefined) => {
      if (!result) {
        return;
      }

      this.campService.createSpeaker(result).subscribe({
        next: () => {
          this.snackBar.open('Speaker created successfully!', 'Close', {
            duration: 3000,
          });
          this.loadSpeakers();
        },
        error: () => {
          this.snackBar.open('Failed to create speaker. Please try again.', 'Close', {
            duration: 4000,
          });
        }
      });
    });
  }

  onDeleteSpeaker(speaker: Speaker): void {
    const ok = confirm(`Delete speaker ${speaker.firstName} ${speaker.lastName}?`);
    if (!ok) {
      return;
    }

    this.campService.deleteSpeaker(speaker.firstName, speaker.lastName).subscribe({
      next: (res) => {
        if (res?.success) {
          this.speakers = this.speakers.filter(s => s.speakerId !== speaker.speakerId);
          this.snackBar.open('Speaker deleted', undefined, { duration: 3000 });
        } else {
          this.snackBar.open('Failed to delete speaker', undefined, { duration: 3000 });
        }
      },
      error: () => this.snackBar.open('Failed to delete speaker', undefined, { duration: 3000 })
    });
  }
}
