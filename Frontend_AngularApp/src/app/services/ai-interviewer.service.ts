import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InterviewRequest } from '../components/models/interview-request.model';

export interface AnswerFeedback {
  overallScore: number;
  feedback: string;
  strengths: string[];
  improvements: string[];
}

export interface AnalyzeAnswerRequest {
  question: string;
  interviewType: string;
  userAnswer: string;
  questionNumber?: number;
  totalQuestions?: number;
}

export interface SummarizeInterviewRequest {   
  questions: string[];
  answers: string[];
  feedbacks: string[];
  role: string | null;
  interviewType: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class interviewerService {

  // Use a relative path so the Angular CLI proxy (`proxy.conf.json`) can forward `/api` requests
  private apiBase = '/api/AI';

  constructor(private http: HttpClient) {}

  // Response expected to contain `data` with an array of question strings
  getQuestions(prompt: InterviewRequest): Observable<{ success: boolean; message?: string; data: string[]; errors?: any[] }> {
    return this.http.post<{ success: boolean; message?: string; data: string[]; errors?: any[] }>(
      `${this.apiBase}/generate-interview-questions`,
      prompt
    );
  }

  analyzeAnswer(request: AnalyzeAnswerRequest): Observable<any> {
    return this.http.post(`${this.apiBase}/analyze-interview-answer`, request);
  }

  summarizeInterview(request: SummarizeInterviewRequest): Observable<any> {
    return this.http.post(`${this.apiBase}/summarize-interview`, request);
  }
}
