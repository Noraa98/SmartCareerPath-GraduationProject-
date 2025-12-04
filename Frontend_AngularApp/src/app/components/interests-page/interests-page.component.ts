import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { InputComponent } from '../ui/input/input.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { QuizService } from '../../services/quiz.service';
import { IconService } from '../../services/icon.service';

interface Interest {
  id: string;
  name: string;
  description: string;
  icon?: string; // Made icon optional
}

const interests: Interest[] = [
  { id: 'data-science', name: 'Data Science', description: 'AI, ML, Analytics' },
  { id: 'design', name: 'Design', description: 'UI/UX, Graphics' },
  { id: 'marketing', name: 'Marketing', description: 'Digital, Content' },
  { id: 'engineering', name: 'Engineering', description: 'Software, Hardware' },
  { id: 'hr', name: 'Human Resources', description: 'Talent, Culture' },
  { id: 'business', name: 'Business', description: 'Management, Strategy' },
  { id: 'healthcare', name: 'Healthcare', description: 'Medical, Wellness' },
  { id: 'education', name: 'Education', description: 'Teaching, Training' },
  { id: 'finance', name: 'Finance', description: 'Banking, Investment' },
  { id: 'media', name: 'Media', description: 'Journalism, PR' },
  { id: 'creative', name: 'Creative Arts', description: 'Photography, Film' },
  { id: 'security', name: 'Cybersecurity', description: 'InfoSec, Privacy' },
];

const additionalInterestsWithDetails: Interest[] = [
  { id: 'software-development', name: 'Software Development', description: 'Coding, Debugging, Building Software', icon: '💻' },
  { id: 'data-science', name: 'Data Science', description: 'AI, ML, Analytics', icon: '📊' },
  { id: 'artificial-intelligence', name: 'Artificial Intelligence', description: 'Machine Learning, Neural Networks', icon: '🤖' },
  { id: 'cloud-computing', name: 'Cloud Computing', description: 'AWS, Azure, GCP', icon: '☁️' },
  { id: 'cybersecurity', name: 'Cybersecurity', description: 'InfoSec, Privacy', icon: '🛡️' },
  { id: 'ui-ux-design', name: 'UI/UX Design', description: 'User Interfaces, User Experience', icon: '🎨' },
  { id: 'graphic-design', name: 'Graphic Design', description: 'Visual Content, Branding', icon: '🖌️' },
  { id: 'product-management', name: 'Product Management', description: 'Roadmaps, Features, Strategy', icon: '📋' },
  { id: 'business-analysis', name: 'Business Analysis', description: 'Requirements, Processes, Solutions', icon: '📈' },
  { id: 'marketing', name: 'Marketing', description: 'Digital, Content', icon: '📢' },
  { id: 'sales', name: 'Sales', description: 'Customer Acquisition, Revenue', icon: '💰' },
  { id: 'finance', name: 'Finance', description: 'Banking, Investment', icon: '💵' },
  { id: 'accounting', name: 'Accounting', description: 'Bookkeeping, Auditing', icon: '🧾' },
  { id: 'human-resources', name: 'Human Resources', description: 'Talent, Culture', icon: '👥' },
  { id: 'project-management', name: 'Project Management', description: 'Planning, Execution, Delivery', icon: '🗂️' },
  { id: 'networking', name: 'Networking', description: 'Infrastructure, Connectivity', icon: '🌐' },
  { id: 'devops', name: 'DevOps', description: 'CI/CD, Automation', icon: '🔧' },
  { id: 'mobile-development', name: 'Mobile Development', description: 'iOS, Android', icon: '📱' },
  { id: 'game-development', name: 'Game Development', description: 'Unity, Unreal Engine', icon: '🎮' },
  { id: 'robotics', name: 'Robotics', description: 'Automation, Hardware', icon: '🤖' },
  { id: 'healthcare-technology', name: 'Healthcare Technology', description: 'Medical Devices, Health IT', icon: '❤️' },
  { id: 'education-technology', name: 'Education Technology', description: 'E-Learning, Tools', icon: '🎓' },
  { id: 'environmental-science', name: 'Environmental Science', description: 'Sustainability, Ecology', icon: '🌱' },
  { id: 'mechanical-engineering', name: 'Mechanical Engineering', description: 'Machines, Design', icon: '⚙️' },
  { id: 'electrical-engineering', name: 'Electrical Engineering', description: 'Circuits, Power Systems', icon: '🔌' },
  { id: 'civil-engineering', name: 'Civil Engineering', description: 'Construction, Infrastructure', icon: '🏗️' },
  { id: 'content-writing', name: 'Content Writing', description: 'Blogs, Articles', icon: '✍️' },
  { id: 'social-media', name: 'Social Media', description: 'Platforms, Engagement', icon: '📱' },
  { id: 'law-legal-tech', name: 'Law/Legal Tech', description: 'Contracts, Compliance', icon: '⚖️' },
  { id: 'entrepreneurship', name: 'Entrepreneurship', description: 'Startups, Innovation', icon: '🚀' }
];


@Component({
  selector: 'app-interests-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    InputComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './interests-page.component.html',
  styleUrl: './interests-page.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('600ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class InterestsPageComponent {
  interests: Interest[];
  selectedInterests: string[] = [];
  searchQuery: string = '';
  loadingQuiz: boolean = false;
  errorMsg: string = '';

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    private quizService: QuizService,
    public iconService: IconService
  ) {
    const uniqueAdditionalInterests = additionalInterestsWithDetails.filter(
      additional => !interests.some(interest => interest.name === additional.name)
    );

    this.interests = [...interests, ...uniqueAdditionalInterests]; // Merge interests with descriptions and icons without duplicates.
  }

  get filteredInterests(): Interest[] {
    if (!this.searchQuery) return this.interests;
    const query = this.searchQuery.toLowerCase();
    return this.interests.filter(
      interest =>
        interest.name.toLowerCase().includes(query) ||
        interest.description.toLowerCase().includes(query)
    );
  }

  toggleInterest(id: string): void {
    if (this.selectedInterests.includes(id)) {
      this.selectedInterests = this.selectedInterests.filter(i => i !== id);
    } else {
      this.selectedInterests = [...this.selectedInterests, id];
    }
  }

  async handleContinue(): Promise<void> {
    console.log('handleContinue called', this.selectedInterests);
    if (this.selectedInterests.length > 0) {
      this.loadingQuiz = true;
      this.errorMsg = '';
      try {
        const questionCount = this.selectedInterests.length * 3;
        const questions = await this.quizService.generateQuizQuestions(this.selectedInterests, questionCount).toPromise();
        this.userDataService.setQuizQuestions(questions);
        this.userDataService.setInterests(this.selectedInterests);
        this.router.navigate(['/quiz']);
      } catch (err) {
        this.errorMsg = 'Failed to generate quiz questions. Please try again.';
        console.error('Quiz generation error:', err);
      } finally {
        this.loadingQuiz = false;
      }
    }
  }

  getIconName(id: string): string {
    const iconMap: Record<string, string> = {
      'data-science': 'chartBar',
      'design': 'paintBrush',
      'marketing': 'chartBar',
      'engineering': 'computerDesktop',
      'hr': 'users',
      'business': 'briefcase',
      'healthcare': 'heart',
      'education': 'academicCap',
      'finance': 'currencyDollar',
      'media': 'megaphone',
      'creative': 'camera',
      'security': 'shield',
      'software-development': 'computerDesktop',
      'artificial-intelligence': 'robot',
      'cloud-computing': 'cloud',
      'ui-ux-design': 'paintBrush',
      'graphic-design': 'paintBrush',
      'product-management': 'clipboard',
      'business-analysis': 'chartBar',
      'sales': 'currencyDollar',
      'accounting': 'clipboard',
      'project-management': 'clipboard',
      'networking': 'globe',
      'devops': 'wrench',
      'mobile-development': 'devicePhoneMobile',
      'game-development': 'gameController',
      'robotics': 'robot',
      'healthcare-technology': 'heart',
      'education-technology': 'academicCap',
      'environmental-science': 'leaf',
      'mechanical-engineering': 'cog',
      'electrical-engineering': 'plug',
      'civil-engineering': 'buildingOffice',
      'content-writing': 'pencil',
      'social-media': 'devicePhoneMobile',
      'law-legal-tech': 'scale',
      'entrepreneurship': 'rocket'
    };
    return iconMap[id] || 'star';
  }
}

