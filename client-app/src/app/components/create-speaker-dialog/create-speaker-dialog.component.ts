import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CreateSpeakerRequest } from '../../services/camp.service';

@Component({
  selector: 'app-create-speaker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Add Speaker</h2>
    <mat-dialog-content>
      <form [formGroup]="speakerForm" class="speaker-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>First Name</mat-label>
          <input matInput formControlName="firstName" />
          <mat-error *ngIf="speakerForm.get('firstName')?.hasError('required')">First name is required</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Last Name</mat-label>
          <input matInput formControlName="lastName" />
          <mat-error *ngIf="speakerForm.get('lastName')?.hasError('required')">Last name is required</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Middle Name</mat-label>
          <input matInput formControlName="middleName" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Company</mat-label>
          <input matInput formControlName="company" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Company URL</mat-label>
          <input matInput formControlName="companyUrl" placeholder="https://" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Blog URL</mat-label>
          <input matInput formControlName="blogUrl" placeholder="https://" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Twitter</mat-label>
          <input matInput formControlName="twitter" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>GitHub</mat-label>
          <input matInput formControlName="gitHub" />
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="speakerForm.invalid">
        Add Speaker
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .speaker-form {
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
export class CreateSpeakerDialogComponent {
  speakerForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateSpeakerDialogComponent>
  ) {
    this.speakerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      middleName: [''],
      company: [''],
      companyUrl: [''],
      blogUrl: [''],
      twitter: [''],
      gitHub: [''],
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.speakerForm.invalid) {
      return;
    }

    const value = this.speakerForm.value;
    const request: CreateSpeakerRequest = {
      firstName: value.firstName,
      lastName: value.lastName,
      middleName: value.middleName || undefined,
      company: value.company || undefined,
      companyUrl: value.companyUrl || undefined,
      blogUrl: value.blogUrl || undefined,
      twitter: value.twitter || undefined,
      gitHub: value.gitHub || undefined,
    };

    this.dialogRef.close(request);
  }
}
