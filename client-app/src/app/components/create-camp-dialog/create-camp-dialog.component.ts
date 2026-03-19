import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { CreateCampRequest } from '../../services/camp.service';

@Component({
  selector: 'app-create-camp-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
  ],
  template: `
    <h2 mat-dialog-title>Create New Camp</h2>
    <mat-dialog-content>
      <form [formGroup]="campForm" class="camp-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Camp Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter camp name" />
          <mat-error *ngIf="campForm.get('name')?.hasError('required')">Name is required</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>City</mat-label>
          <input matInput formControlName="city" placeholder="Enter city" />
          <mat-error *ngIf="campForm.get('city')?.hasError('required')">City is required</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Event Date</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="eventDate" placeholder="Select date" />
          <mat-datepicker-toggle matIconSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
          <mat-error *ngIf="campForm.get('eventDate')?.hasError('required')">Date is required</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="campForm.invalid">
        Create Camp
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .camp-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
      min-width: 340px;
      padding-top: 8px;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class CreateCampDialogComponent {
  campForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateCampDialogComponent>
  ) {
    this.campForm = this.fb.group({
      name: ['', Validators.required],
      city: ['', Validators.required],
      eventDate: [null, Validators.required],
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.campForm.valid) {
      const formValue = this.campForm.value;
      const request: CreateCampRequest = {
        name: formValue.name,
        city: formValue.city,
        eventDate: (formValue.eventDate as Date).toISOString(),
        length: 1,
      };
      this.dialogRef.close(request);
    }
  }
}
