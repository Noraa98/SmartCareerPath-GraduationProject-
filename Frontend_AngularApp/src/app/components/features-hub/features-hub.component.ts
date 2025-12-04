import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { AuthService } from '../../services/auth.service';
import { IconService } from '../../services/icon.service';

interface Feature {
  id: string;
  title: string;
  description: string;
  icon: string;
  features: string[];
  color: string;
}

const features: Feature[] = [
  {
    id: 'cv-builder',
    title: 'CV Builder Agent',
    description: 'Create professional, ATS-optimized CVs tailored to your target roles. Our AI analyzes job requirements and highlights your skills.',
    icon: 'documentText',
    features: ['ATS-optimized templates', 'Industry-specific customization', 'Real-time AI suggestions', 'Export to PDF/Word'],
    color: '',
  },
  {
    id: 'job-parser',
    title: 'Job Description Parser',
    description: 'Instantly analyze job postings to extract key requirements, skills, and qualifications. Get a match score and actionable insights.',
    icon: 'magnifyingGlass',
    features: ['Extract key requirements', 'Skill match analysis', 'Salary range insights', 'Personalized recommendations'],
    color: '',
  },
  {
    id: 'ai-interviewer',
    title: 'AI Interviewer',
    description: 'Practice with our AI interviewer that simulates real interview scenarios. Get instant feedback on your answers and improve your performance.',
    icon: 'chatBubbleEllipses',
    features: ['Realistic interview simulation', 'Instant feedback & scoring', 'Multiple interview styles', 'Performance analytics'],
    color: '',
  },
  {
    id: 'career-recommendation',
    title: 'Career Path Recommendation',
    description: 'Discover your ideal career path based on your interests and quiz answers. Get personalized recommendations and insights powered by AI.',
    icon: 'compass',
    features: [
      'Personalized career path',
      'AI-driven recommendations',
      'Reasoning and insights',
      'Seamless integration with your profile'
    ],
    color: '',
  },
];

@Component({
  selector: 'app-features-hub',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './features-hub.component.html',
  styleUrl: './features-hub.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class FeaturesHubComponent {
  features = features;
  userName: string = '';
  userRole: string | null = '';
  premiumFeatures = ['job-parser', 'ai-interviewer']; // IDs of premium-only features

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    private auth: AuthService,
    public iconService: IconService
  ) {
    const userData = this.userDataService.getUserData();
    this.userName = userData.name;
    this.userRole = this.auth.getUserRole();
  }

  isPremium(): boolean {
    return this.userRole === 'Premium';
  }

  isPremiumFeature(featureId: string): boolean {
    return this.premiumFeatures.includes(featureId);
  }

  isFeatureDisabled(featureId: string): boolean {
    return this.isPremiumFeature(featureId) && !this.isPremium();
  }

  onFeatureSelect(featureId: string): void {
    if (!this.isFeatureDisabled(featureId)) {
      this.router.navigate([`/${featureId}`]);
    }
  }
}

