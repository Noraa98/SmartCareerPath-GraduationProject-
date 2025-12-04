import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CareerRecommendationService {
  private apiBase = '/api/AI';

  constructor(private http: HttpClient) {}

  recommendCareerPath(body: any): Observable<any> {
    return this.http.post<any>(`${this.apiBase}/recommend-career-path-from-quiz`, body);
  }
}
