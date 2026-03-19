import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { CampService, Camp } from '../../services/camp.service';
import { CreateCampDialogComponent } from '../create-camp-dialog/create-camp-dialog.component';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
  ],
  template: `
    <div class="calendar-container">
      <div class="calendar-header">
        <h1 class="page-title">
          <mat-icon>event</mat-icon>
          Camp Calendar
        </h1>
        <button mat-raised-button color="primary" class="create-btn" (click)="openCreateCamp()">
          <mat-icon>add</mat-icon>
          Create Camp
        </button>
      </div>

      <div class="date-picker-section">
        <mat-card class="date-card">
          <mat-card-content>
            <div class="date-picker-wrapper">
              <mat-calendar
                [(selected)]="selectedDate"
                (selectedChange)="onDateSelected($event)">
              </mat-calendar>
            </div>
          </mat-card-content>
        </mat-card>

        <div class="camps-section">
          <mat-card class="camps-card">
            <mat-card-header>
              <mat-card-title>
                Camps on {{ selectedDate | date:'MMMM d, yyyy' }}
              </mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div *ngIf="loading" class="loading-spinner">
                <mat-spinner diameter="40"></mat-spinner>
              </div>

              <div *ngIf="!loading && filteredCamps.length === 0" class="no-camps">
                <mat-icon class="no-camps-icon">event_busy</mat-icon>
                <p>No camps scheduled for this date.</p>
              </div>

              <div *ngIf="!loading && filteredCamps.length > 0" class="camps-list">
                <mat-card *ngFor="let camp of filteredCamps" class="camp-item">
                  <mat-card-content>
                    <div class="camp-info">
                      <div class="camp-name">{{ camp.name }}</div>
                      <div class="camp-details">
                        <mat-icon class="detail-icon">location_city</mat-icon>
                        <span>{{ camp.city }}</span>
                        <mat-icon class="detail-icon">schedule</mat-icon>
                        <span>{{ camp.length }} day(s)</span>
                      </div>
                    </div>
                  </mat-card-content>
                </mat-card>
              </div>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .calendar-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .calendar-header {
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

    .create-btn {
      font-size: 15px;
      padding: 8px 20px;
    }

    .date-picker-section {
      display: flex;
      gap: 24px;
      align-items: flex-start;
    }

    .date-card {
      flex-shrink: 0;
      border-radius: 12px;
    }

    .camps-section {
      flex: 1;
    }

    .camps-card {
      border-radius: 12px;
    }

    .loading-spinner {
      display: flex;
      justify-content: center;
      padding: 32px;
    }

    .no-camps {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px 20px;
      color: #78909c;
    }

    .no-camps-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 12px;
    }

    .camps-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding-top: 8px;
    }

    .camp-item {
      background: #e3f2fd;
      border-left: 4px solid #1976d2;
      border-radius: 8px !important;
    }

    .camp-name {
      font-size: 18px;
      font-weight: 600;
      color: #0d47a1;
      margin-bottom: 8px;
    }

    .camp-details {
      display: flex;
      align-items: center;
      gap: 6px;
      color: #546e7a;
      font-size: 14px;
    }

    .detail-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    @media (max-width: 768px) {
      .date-picker-section {
        flex-direction: column;
      }
      .date-card {
        width: 100%;
      }
    }
  `]
})
export class CalendarComponent implements OnInit {
  selectedDate: Date = new Date();
  allCamps: Camp[] = [];
  filteredCamps: Camp[] = [];
  loading = false;

  constructor(
    private campService: CampService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCamps();
  }

  loadCamps(): void {
    this.loading = true;
    this.campService.getCamps().subscribe({
      next: (camps) => {
        this.allCamps = camps;
        this.filterCampsByDate();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onDateSelected(date: Date | null): void {
    if (date) {
      this.selectedDate = date;
      this.filterCampsByDate();
    }
  }

  filterCampsByDate(): void {
    if (!this.selectedDate) return;
    const selected = this.selectedDate;
    this.filteredCamps = this.allCamps.filter(camp => {
      const campDate = new Date(camp.eventDate);
      return campDate.getFullYear() === selected.getFullYear() &&
             campDate.getMonth() === selected.getMonth() &&
             campDate.getDate() === selected.getDate();
    });
  }

  openCreateCamp(): void {
    const dialogRef = this.dialog.open(CreateCampDialogComponent, {
      width: '440px',
      disableClose: false,
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.campService.createCamp(result).subscribe({
          next: () => {
            this.snackBar.open('Camp created successfully!', 'Close', {
              duration: 3000,
              panelClass: ['success-snack']
            });
            this.loadCamps();
          },
          error: () => {
            this.snackBar.open('Failed to create camp. Please try again.', 'Close', {
              duration: 4000,
              panelClass: ['error-snack']
            });
          }
        });
      }
    });
  }
}
