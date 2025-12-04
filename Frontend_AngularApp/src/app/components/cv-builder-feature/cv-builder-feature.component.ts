import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { InputComponent } from '../ui/input/input.component';
import { LabelComponent } from '../ui/label/label.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { IconService } from '../../services/icon.service';

interface WorkExperience {
  id: string;
  companyName: string;
  role: string;
  startDate: string;
  endDate: string;
  description: string;
}

@Component({
  selector: 'app-cv-builder-feature',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    InputComponent,
    LabelComponent,
    CardComponent,
    CardContentComponent
  ],
  templateUrl: './cv-builder-feature.component.html',
  styleUrl: './cv-builder-feature.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class CVBuilderFeatureComponent implements OnInit, AfterViewInit {
  // Personal Information
  fullName: string = '';
  email: string = '';
  phoneNumber: string = '';

  // Professional Summary
  professionalSummary: string = '';

  // Work Experience
  workExperiences: WorkExperience[] = [
    { id: '1', companyName: '', role: '', startDate: '', endDate: '', description: '' }
  ];

  // Education & Skills
  education: string = '';
  skills: string = '';
  certifications: string = '';
  languages: string = '';

  // Target Position
  desiredJobRole: string = '';

  // UI State
  currentSection: number = 0;
  isGenerating: boolean = false;
  generationProgress: number = 0;
  userName: string = '';

  sections = [
    { title: 'Personal Information', icon: 'user' },
    { title: 'Work Experience', icon: 'briefcase' },
    { title: 'Education & Skills', icon: 'academicCap' },
    { title: 'Target Position', icon: 'flag' }
  ];

  Math = Math;

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    public iconService: IconService
  ) {
    const userData = this.userDataService.getUserData();
    this.userName = userData.name || '';
  }

  ngOnInit(): void {
    // Pre-fill with user data if available
    if (this.userName) {
      this.fullName = this.userName;
    }
  }

  ngAfterViewInit(): void {
    const button = document.querySelector('app-button'); // Generalized selector
    if (button) {
      console.log('Generate My CV button is rendered on the page:', button);
    } else {
      console.error('Generate My CV button is not rendered in the DOM. Ensure the button exists in the template.');
    }
  }

  get progress(): number {
    return ((this.currentSection + 1) / this.sections.length) * 100;
  }

  addWorkExperience(): void {
    const newId = (this.workExperiences.length + 1).toString();
    this.workExperiences.push({
      id: newId,
      companyName: '',
      role: '',
      startDate: '',
      endDate: '',
      description: ''
    });
  }

  removeWorkExperience(index: number): void {
    if (this.workExperiences.length > 1) {
      this.workExperiences.splice(index, 1);
    }
  }

  nextSection(): void {
    if (this.currentSection < this.sections.length - 1) {
      this.currentSection++;
    }
  }

  prevSection(): void {
    if (this.currentSection > 0) {
      this.currentSection--;
    }
  }

  goToSection(index: number): void {
    this.currentSection = index;
  }

  generateCV(): void {
    // Validate required fields
    if (!this.fullName || !this.email || !this.phoneNumber) {
      alert('Please fill in all required personal information fields.');
      this.currentSection = 0;
      return;
    }
    // Prepare CV payload and navigate to preview page
    const cvPayload = {
      fullName: this.fullName,
      email: this.email,
      phoneNumber: this.phoneNumber,
      professionalSummary: this.professionalSummary,
      workExperiences: this.workExperiences,
      education: this.education,
      skills: this.skills,
      certifications: this.certifications,
      languages: this.languages,
      desiredJobRole: this.desiredJobRole
    };

    try {
      sessionStorage.setItem('cvPreviewData', JSON.stringify(cvPayload));
      console.log('CV data stored in sessionStorage:', cvPayload); // Debugging log
      this.router.navigate(['/cv-builder/preview']).then(success => {
        if (success) {
          console.log('Navigation to /cv-builder/preview successful');
        } else {
          console.error('Navigation to /cv-builder/preview failed');
        }
      });
    } catch (e) {
      console.error('Failed to store CV payload in sessionStorage', e);
      alert('Failed to generate CV preview. Please try again.');
    }
  }

  onBack(): void {
    this.router.navigate(['/features']);
  }
}
