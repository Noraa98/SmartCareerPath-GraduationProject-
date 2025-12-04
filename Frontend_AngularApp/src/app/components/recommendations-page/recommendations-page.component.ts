import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { CareerRecommendationService } from '../../services/career-recommendation.service';
import { IconService } from '../../services/icon.service';

interface CareerRecommendation {
  id: number;
  title: string;
  description: string;
  matchPercentage: number;
  salaryRange: string;
  demand: string;
  skills: string[];
  icon: string;
}

const careerRecommendations: CareerRecommendation[] = [
  {
    id: 1,
    title: 'AI/ML Engineer',
    description: 'Design and develop artificial intelligence and machine learning systems',
    matchPercentage: 94,
    salaryRange: '$120K - $180K',
    demand: 'Very High',
    skills: ['Python', 'TensorFlow', 'Deep Learning', 'Data Analysis'],
    icon: 'robot',
  },
  {
    id: 2,
    title: 'Data Scientist',
    description: 'Extract insights from complex data using statistical analysis and ML',
    matchPercentage: 89,
    salaryRange: '$110K - $160K',
    demand: 'High',
    skills: ['Python', 'R', 'SQL', 'Statistics'],
    icon: 'chartBar',
  },
  {
    id: 3,
    title: 'Product Manager',
    description: 'Lead product strategy and development from conception to launch',
    matchPercentage: 85,
    salaryRange: '$130K - $190K',
    demand: 'High',
    skills: ['Strategy', 'Agile', 'Analytics', 'Communication'],
    icon: 'flag',
  },
];

@Component({
  selector: 'app-recommendations-page',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './recommendations-page.component.html',
  styleUrl: './recommendations-page.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class RecommendationsPageComponent implements OnInit {
  isLoading: boolean = true;
  expandedCard: number | null = null;
  recommendation: any = null;
  userName: string = '';

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    private careerRecommendationService: CareerRecommendationService,
    public iconService: IconService
  ) {}

  ngOnInit(): void {
    const userData = this.userDataService.getUserData();
    this.userName = userData.name;
    const interests = userData.interests;
    const answers = userData.quizAnswers;
    this.careerRecommendationService.recommendCareerPath({
      interests,
      answers
    }).subscribe({
      next: (result) => {
        this.recommendation = result;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  toggleCard(id: number): void {
    this.expandedCard = this.expandedCard === id ? null : id;
  }

  handleRetakeQuiz(): void {
    this.userDataService.resetQuizAnswers();
    this.router.navigate(['/interests']);
  }

  handleContinue(): void {
    this.router.navigate(['/features']);
  }
}

