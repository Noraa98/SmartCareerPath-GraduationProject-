import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { ParseJobDescriptionService } from '../../services/parse-job-description.service';
import { HttpClientModule } from '@angular/common/http';
import { IconService } from '../../services/icon.service';

interface AnalysisResult {
  keySkills: string[];
  requirements: string[];
  keywords: string[];
  suggestedImprovements: string[];
}

@Component({
  selector: 'app-job-parser-feature',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    CardComponent,
    CardContentComponent,
    HttpClientModule
  ],
  templateUrl: './job-parser-feature.component.html',
  styleUrl: './job-parser-feature.component.css',
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('500ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('scaleIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ]
})
export class JobParserFeatureComponent {
  jobDescription: string = '';
  isAnalyzing: boolean = false;
  analysisComplete: boolean = false;
  analysisResult: any = null;
  userName: string = '';

  constructor(
    private router: Router,
    private userDataService: UserDataService,
    private parseJobDescriptionService: ParseJobDescriptionService,
    public iconService: IconService
  ) {
    const userData = this.userDataService.getUserData();
    this.userName = userData.name || '';
  }

  analyzeJobPosting(): void {
    if (!this.jobDescription.trim()) {
      alert('Please paste a job description to analyze.');
      return;
    }

    this.isAnalyzing = true;
    this.analysisComplete = false;
    this.analysisResult = null;

    this.parseJobDescriptionService.parseJobDescription(this.jobDescription).subscribe({
      next: (response) => {
        try {
          if (response.success && response.data) {
            let data = response.data;
            
            // If data is a string (raw JSON response), parse it
            if (typeof data === 'string') {
              // Strip markdown code fence markers if present
              let cleanData = data.trim();
              cleanData = cleanData.replace(/^```json\n?/, '').replace(/^```\n?/, '');
              cleanData = cleanData.replace(/\n?```$/, '');
              
              try {
                data = JSON.parse(cleanData);
              } catch (e) {
                console.warn('Failed to parse response data as JSON, using raw string');
              }
            }
            
            this.analysisResult = data;
            this.analysisComplete = true;
          } else {
            alert(response.message || 'Failed to parse job description.');
            this.analysisComplete = false;
          }
        } catch (err) {
          console.error('Error processing job description response:', err);
          alert('Error processing job description.');
          this.analysisComplete = false;
        } finally {
          this.isAnalyzing = false;
        }
      },
      error: (err) => {
        console.error('Error parsing job description:', err);
        alert('Error parsing job description. Please check the backend logs.');
        this.isAnalyzing = false;
        this.analysisComplete = false;
      }
    });
  }

  clearAnalysis(): void {
    this.jobDescription = '';
    this.analysisComplete = false;
    this.analysisResult = null;
  }

  onBack(): void {
    this.router.navigate(['/features']);
  }
}
