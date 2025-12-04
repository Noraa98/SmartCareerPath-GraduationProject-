import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ParseJobDescriptionService {
  private apiBase = '/api/AI';

  constructor(private http: HttpClient) {}

  parseJobDescription(jobDescription: string): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/parse-job-description`, { jobDescription });
  }
}
