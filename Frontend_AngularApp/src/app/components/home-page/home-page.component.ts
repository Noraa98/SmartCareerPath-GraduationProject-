import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { trigger, state, style, transition, animate, query, stagger } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { FooterComponent } from '../footer/footer.component';
import { IconService } from '../../services/icon.service';
import { AuthService } from '../../services/auth.service';

interface Feature {
  id: string;
  title: string;
  description: string;
  icon: string;
  color: string;
  route: string;
}

interface CareerRecommendation {
  title: string;
  match: number;
  insight: string;
  icon: string;
}

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    ButtonComponent,
    CardComponent,
    CardContentComponent,
    FooterComponent
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(30px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideInLeft', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-30px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('slideInRight', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(30px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('staggerFadeIn', [
      transition(':enter', [
        query(':enter', [
          style({ opacity: 0, transform: 'translateY(20px)' }),
          stagger(100, [
            animate('500ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
          ])
        ], { optional: true })
      ])
    ]),
    trigger('scaleIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ]
})
export class HomePageComponent implements OnInit {
  features: Feature[] = [
    {
      id: 'cv-builder',
      title: 'CV Builder',
      description: 'Create a professional, ATS-friendly resume with AI-powered suggestions and templates.',
      icon: 'documentText',
      color: '',
      route: '/cv-builder'
    },
    {
      id: 'job-parser',
      title: 'Job Description Parser',
      description: 'Analyze job descriptions and extract key requirements, skills, and qualifications.',
      icon: 'magnifyingGlass',
      color: ' ',
      route: '/job-parser'
    },
    {
      id: 'ai-interviewer',
      title: 'AI Interviewer',
      description: 'Practice interviews with AI-powered questions tailored to your target role.',
      icon: 'chatBubbleEllipses',
      color: '',
      route: '/ai-interviewer'
    }
  ];

  recommendations: CareerRecommendation[] = [
    {
      title: 'Software Engineer',
      match: 92,
      insight: 'Strong match based on your technical skills and interests',
      icon: 'computerDesktop'
    },
    {
      title: 'Data Scientist',
      match: 87,
      insight: 'Excellent analytical skills alignment',
      icon: 'chartBar'
    },
    {
      title: 'Product Manager',
      match: 78,
      insight: 'Good leadership and communication match',
      icon: 'rocket'
    }
  ];

  currentRecommendationIndex: number = 0;
  isScrolled: boolean = false;
  searchQuery: string = '';

  ngOnInit(): void {
    // Auto-rotate recommendations
    setInterval(() => {
      this.currentRecommendationIndex = (this.currentRecommendationIndex + 1) % this.recommendations.length;
    }, 5000);
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(): void {
    this.isScrolled = window.scrollY > 50;
  }

  onFeatureClick(feature: Feature): void {
    this.router.navigate([feature.route]);
  }

  constructor(
    public router: Router,
    public iconService: IconService,
    private auth: AuthService
  ) {}

  onGetStarted(): void {
    if (this.auth.isLoggedIn()) {
      // If already logged in, just stay on /home (or re-navigate to it)
      this.router.navigate(['/home']);
    } else {
      // If not logged in, proceed to registration
      this.router.navigate(['/registration']);
    }
  }

  onExploreFeatures(): void {
    this.router.navigate(['/features']);
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      // Navigate to search results or recommendations
      this.router.navigate(['/recommendations'], { queryParams: { search: this.searchQuery } });
    }
  }

  onViewAllRecommendations(): void {
    this.router.navigate(['/recommendations']);
  }

  onTryCareerQuiz(): void {
    // Navigate to the career quiz entry (interests)
    this.router.navigate(['/interests']);
  }

  nextRecommendation(): void {
    this.currentRecommendationIndex = (this.currentRecommendationIndex + 1) % this.recommendations.length;
  }

  prevRecommendation(): void {
    this.currentRecommendationIndex = (this.currentRecommendationIndex - 1 + this.recommendations.length) % this.recommendations.length;
  }
}

