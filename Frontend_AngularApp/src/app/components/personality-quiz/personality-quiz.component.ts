import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { ProgressComponent } from '../ui/progress/progress.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService, QuizQuestion } from '../../services/user-data.service';
import { IconService } from '../../services/icon.service';

interface Question {
  id: number;
  question: string;
  options: { id: string; label: string; emoji: string }[];
}

const questions: Question[] = [
  {
    id: 1,
    question: 'How do you prefer to work?',
    options: [
      { id: 'team', label: 'In a team', emoji: '👥' },
      { id: 'solo', label: 'Independently', emoji: '🧑‍💻' },
      { id: 'hybrid', label: 'Mix of both', emoji: '🤝' },
    ],
  },
  {
    id: 2,
    question: 'What energizes you the most?',
    options: [
      { id: 'creating', label: 'Creating new things', emoji: '✨' },
      { id: 'solving', label: 'Solving problems', emoji: '🧩' },
      { id: 'leading', label: 'Leading others', emoji: '🎯' },
      { id: 'helping', label: 'Helping people', emoji: '💝' },
    ],
  },
  {
    id: 3,
    question: 'How do you handle challenges?',
    options: [
      { id: 'analytical', label: 'Analyze data & facts', emoji: '📊' },
      { id: 'creative', label: 'Think creatively', emoji: '🎨' },
      { id: 'collaborative', label: 'Seek team input', emoji: '💬' },
      { id: 'decisive', label: 'Make quick decisions', emoji: '⚡' },
    ],
  },
  {
    id: 4,
    question: "What's your ideal work environment?",
    options: [
      { id: 'structured', label: 'Structured & organized', emoji: '📋' },
      { id: 'flexible', label: 'Flexible & dynamic', emoji: '🌊' },
      { id: 'innovative', label: 'Innovative & cutting-edge', emoji: '🚀' },
      { id: 'stable', label: 'Stable & predictable', emoji: '🏛️' },
    ],
  },
  {
    id: 5,
    question: 'What drives your career decisions?',
    options: [
      { id: 'impact', label: 'Making an impact', emoji: '🌍' },
      { id: 'growth', label: 'Personal growth', emoji: '📈' },
      { id: 'balance', label: 'Work-life balance', emoji: '⚖️' },
      { id: 'compensation', label: 'Compensation', emoji: '💰' },
    ],
  },
];

@Component({
  selector: 'app-personality-quiz',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    ProgressComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './personality-quiz.component.html',
  styleUrl: './personality-quiz.component.css',
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ opacity: 0, transform: 'translateX(-20px)' }))
      ])
    ])
  ]
})
export class PersonalityQuizComponent {
  questions: QuizQuestion[] = [];
  currentStep: number = 0;
  answers: { questionId: number; questionText: string; userAnswer: string }[] = [];
  selectedAnswer: string | null = null;
  isTransitioning: boolean = false;

  get currentQuestion(): QuizQuestion {
    return this.questions[this.currentStep];
  }

  get progress(): number {
    return ((this.currentStep + 1) / this.questions.length) * 100;
  }

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    public iconService: IconService
  ) {
    this.questions = this.userDataService.getQuizQuestions();
  }

  handleAnswer(choice: string): void {
    if (this.isTransitioning) return;
    this.selectedAnswer = choice;
    this.isTransitioning = true;
    setTimeout(() => {
      const question = this.currentQuestion;
      this.answers.push({
        questionId: this.currentStep,
        questionText: question.QuestionText,
        userAnswer: choice
      });
      this.selectedAnswer = null;
      if (this.currentStep < this.questions.length - 1) {
        this.currentStep++;
        this.isTransitioning = false;
      } else {
        this.userDataService.setQuizAnswers(this.answers);
        this.router.navigate(['/recommendations']);
      }
    }, 300);
  }

  handleBack(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
      this.answers.pop();
      this.selectedAnswer = null;
      this.isTransitioning = false;
    } else {
      this.router.navigate(['/interests']);
    }
  }
}

