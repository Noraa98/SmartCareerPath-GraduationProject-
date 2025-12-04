import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class QuizService {
  constructor(private http: HttpClient) {}

  generateQuizQuestions(interests: string[], questionCount: number): Observable<any> {
    return this.http.post<any>('/api/AI/generate-quiz-questions', {
      interests,
      questionCount: questionCount.toString()
    });
  }
}
