import { Routes } from '@angular/router';
import { CalendarComponent } from './components/calendar/calendar.component';
import { SpeakersComponent } from './components/speakers/speakers.component';

export const routes: Routes = [
  { path: '', redirectTo: 'calendar', pathMatch: 'full' },
  { path: 'calendar', component: CalendarComponent },
  { path: 'speakers', component: SpeakersComponent },
];
