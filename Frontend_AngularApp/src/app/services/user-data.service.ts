import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export type Step = 'registration' | 'home' | 'interests' | 'quiz' | 'recommendations' | 'features' | 'cv-builder' | 'job-parser' | 'ai-interviewer';

export interface QuizAnswer {
  questionId: number;
  questionText: string;
  userAnswer: string;
}

export interface UserData {
  name: string;
  interests: string[];
  quizAnswers: QuizAnswer[];
  recommendedRole?: string;
}

export interface QuizQuestion {
  QuestionText: string;
  Choices: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserDataService {
  private currentStepSubject = new BehaviorSubject<Step>('registration');
  public currentStep$: Observable<Step> = this.currentStepSubject.asObservable();

  private userDataSubject = new BehaviorSubject<UserData>({
    name: '',
    interests: [],
    quizAnswers: [],
  });
  public userData$: Observable<UserData> = this.userDataSubject.asObservable();

  private quizQuestions: QuizQuestion[] = [];

  getCurrentStep(): Step {
    return this.currentStepSubject.value;
  }

  setCurrentStep(step: Step): void {
    this.currentStepSubject.next(step);
  }

  getUserData(): UserData {
    return this.userDataSubject.value;
  }

  updateUserData(updates: Partial<UserData>): void {
    const currentData = this.userDataSubject.value;
    this.userDataSubject.next({ ...currentData, ...updates });
  }

  setName(name: string): void {
    this.updateUserData({ name });
  }

  setInterests(interests: string[]): void {
    this.updateUserData({ interests });
  }

  setQuizAnswers(answers: QuizAnswer[]): void {
    this.updateUserData({ quizAnswers: answers });
  }

  setRecommendedRole(role: string): void {
    this.updateUserData({ recommendedRole: role });
  }

  resetQuizAnswers(): void {
    this.updateUserData({ quizAnswers: [] });
  }

  setQuizQuestions(questions: QuizQuestion[]): void {
    this.quizQuestions = questions;
  }

  getQuizQuestions(): QuizQuestion[] {
    return this.quizQuestions;
  }
}

