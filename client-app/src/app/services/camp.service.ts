import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Camp {
  campId: number;
  name: string;
  city: string;
  eventDate: string;
  length: number;
}

export interface CreateCampRequest {
  name: string;
  city: string;
  eventDate: string;
  length: number;
}

export interface Speaker {
  speakerId: number;
  firstName: string;
  lastName: string;
  middleName?: string;
  company?: string;
  companyUrl?: string;
  blogUrl?: string;
  twitter?: string;
  gitHub?: string;
}

export interface CreateSpeakerRequest {
  firstName: string;
  lastName: string;
  middleName?: string;
  company?: string;
  companyUrl?: string;
  blogUrl?: string;
  twitter?: string;
  gitHub?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CampService {
  private apiBase = '/api';

  constructor(private http: HttpClient) {}

  getCamps(): Observable<Camp[]> {
    return this.http.get<Camp[]>(`${this.apiBase}/camps`);
  }

  createCamp(request: CreateCampRequest): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(`${this.apiBase}/camps`, request);
  }

  getSpeakers(): Observable<Speaker[]> {
    return this.http.get<Speaker[]>(`${this.apiBase}/speakers`);
  }

  createSpeaker(request: CreateSpeakerRequest): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(`${this.apiBase}/speakers`, request);
  }

  deleteSpeaker(firstName: string, lastName: string): Observable<{ success: boolean }> {
    const url = `${this.apiBase}/speakers/${encodeURIComponent(firstName)}/${encodeURIComponent(lastName)}`;
    return this.http.delete<{ success: boolean }>(url);
  }
}
