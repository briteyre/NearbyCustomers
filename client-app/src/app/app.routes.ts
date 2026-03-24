import { Routes } from '@angular/router';
import { CalendarComponent } from './components/calendar/calendar.component';
import { SpeakersComponent } from './components/speakers/speakers.component';
import { ChatComponent } from './components/chat/chat.component';
import { ChatPageComponent } from './pages/chat-page/chat-page.component';

export const routes: Routes = [
  { path: '', redirectTo: 'calendar', pathMatch: 'full' },
  { path: 'calendar', component: CalendarComponent },
  { path: 'speakers', component: SpeakersComponent },
  { path: 'chat', component: ChatPageComponent },
];
