import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { UserDataService } from '../../services/user-data.service';
import { HttpClientModule } from '@angular/common/http';
import { interviewerService, AnswerFeedback, AnalyzeAnswerRequest, SummarizeInterviewRequest } from '../../services/ai-interviewer.service';
import { InterviewRequest } from '../models/interview-request.model';
import { Observable, firstValueFrom } from 'rxjs';
import { IconService } from '../../services/icon.service';

interface QuestionFeedback {
  overallScore: number;
  feedback: string;
  strengths: string[];
  improvements: string[];
}

interface Question {
  id: number;
  question: string;
  category: string;
  answer: string;
  aiFeedback?: QuestionFeedback;
}

interface InterviewResult {
  question: Question;
  answer: string;
  feedback: QuestionFeedback;
}

interface APIResponse {
  success: boolean;
  message?: string;
  data: string[];
  errors?: any[];
}

type InterviewStyle = 'behavioral' | 'technical' | 'general';

@Component({
  selector: 'app-ai-interviewer-feature',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonComponent,
    CardComponent,
    CardContentComponent,
    HttpClientModule,
  ],
  providers: [interviewerService],
  templateUrl: './ai-interviewer-feature.component.html',
  styleUrl: './ai-interviewer-feature.component.css',
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
export class AIInterviewerFeatureComponent implements OnInit {
  userName: string = '';
  recommendedRole: string = 'DevOps Engineer';
  selectedStyle: string | null = null;
  interviewStarted: boolean = false;
  currentQuestionIndex: number = 0;
  isSubmitting: boolean = false;
  customRole: string = '';
  customQuestionCount: number = 5;
  newRequest: InterviewRequest = {
    role: 'DevOps Engineer',
    interviewType: '',
    questionCount: 5
  };

  interviewStyles: Array<{ id: string; name: string; icon: string; description: string }> = [
    { id: 'behavioral', name: 'Behavioral', icon: '🧠', description: 'Focus on past experiences and how you handled situations' },
    { id: 'technical', name: 'Technical', icon: '💻', description: 'Test your technical knowledge and problem-solving skills' },
    { id: 'general', name: 'General', icon: '💬', description: 'Mix of behavioral, technical, and general questions' }
  ];

  questions: Question[] = [];
  totalQuestions: number = 5;
  interviewResults: InterviewResult[] = [];
  interviewComplete: boolean = false;
  showReportView: boolean = false;
  reportNow: string = new Date().toLocaleString();
  interviewSummary: string = '';


  constructor(
    private router: Router,
    private interviewerService: interviewerService,
    private userDataService: UserDataService,
    public iconService: IconService
  ) {
    const userData = this.userDataService.getUserData();
    this.userName = userData.name || '';
    this.recommendedRole = 'DevOps Engineer';
    // Update newRequest with the correct role
    this.newRequest.role = this.recommendedRole;
  }

  ngOnInit(): void {
    // Initialize with Salesman role
    this.newRequest.role = 'DevOps Engineer';
  }

  selectStyle(style: string): void {
    this.newRequest.interviewType = style;
    this.selectedStyle = style as InterviewStyle;
    // Log the request being sent
    console.log('Selected style:', style);
    console.log('Request to be sent:', this.newRequest);
  }

  async loadQuestionsFromAPI(style: InterviewRequest): Promise<void> {
    try {
      console.log('Loading questions with request:', this.newRequest);
      console.log('Posting to:', `/api/AI/generate-interview-questions`);
      const response: APIResponse = await firstValueFrom(this.interviewerService.getQuestions(this.newRequest));
      
      console.log('API Response:', response);
      
      if (!response.data || response.data.length === 0) {
        throw new Error('No questions returned from API');
      }
      // Defensive parsing: backend may return raw question strings OR a JSON string
      // (for example a serialized AI provider response). Try to extract readable
      // question text(s) from each data element.
      const rawItems: string[] = response.data;
      const extracted: string[] = [];

      for (const item of rawItems) {
        let text = String(item || '').trim();

        // If the item looks like a JSON string, try to parse and extract content
        if (text.startsWith('{') || text.startsWith('[')) {
          try {
            const parsed = JSON.parse(text);
            // Common locations where the assistant content may be present
            text = parsed?.choices?.[0]?.message?.content || parsed?.message?.content || parsed?.content || JSON.stringify(parsed);
          } catch (e) {
            // leave text as-is if JSON.parse fails
            console.warn('Failed to parse API data element as JSON, using raw string');
          }
        }

        // Clean up AI model artifacts and tokens
        // Remove Mistral instruction markers: [/INST], [INST], etc.
        text = text.replace(/\[\/INST\]/g, '').replace(/\[INST\]/g, '').replace(/\[\/?\w+\]/g, '');
        // Remove leading/trailing brackets and content like [INST] ... [/INST]
        text = text.replace(/^\s*\[.*?\]\s*/, '').replace(/\s*\[.*?\]\s*$/, '');
        // Remove markdown code fence markers
        text = text.replace(/```[\w]*\n?/g, '');
        // Remove common AI filler
        text = text.replace(/^(here are|here's|here is|the following|below|answer:)/i, '').trim();

        // Split multi-line responses into candidate question lines
        const lines = text
          .split(/\r?\n/)
          .map(l => {
            // Remove numbering like "1.", "1)", "1 -", etc.
            l = l.replace(/^\s*\d+[\)\.\-\:]?\s*/, '');
            // Remove bullet points
            l = l.replace(/^\s*[-•*]\s*/, '');
            // Trim
            return l.trim();
          })
          .filter(l => l.length > 10); // filter out short fragments

        if (lines.length > 0) {
          extracted.push(...lines);
        } else if (text.length > 10) {
          extracted.push(text);
        }
      }

      // Capitalize the category from the selected style
      const categoryName = this.selectedStyle 
        ? this.selectedStyle.charAt(0).toUpperCase() + this.selectedStyle.slice(1)
        : 'General';

      // Respect requested question count when available
      const count = this.newRequest?.questionCount || extracted.length;
      const chosen = extracted.slice(0, count);

      // Transform into Question objects
      this.questions = chosen.map((questionText: string, index: number) => ({
        id: index + 1,
        question: questionText,
        category: categoryName,
        answer: ''
      }));

      this.totalQuestions = this.questions.length;
      console.log(`Loaded ${this.newRequest.interviewType} questions from API.`);
      console.log('Questions loaded:', this.questions);
    } catch (error: any) {
      console.error('Failed to load questions from API:', error);
      const errorMessage = error?.error?.message || error?.message || 'Failed to load questions. Please try again later.';
      console.error('Full error object:', error);
      console.error('Error status:', error?.status);
      console.error('Error statusText:', error?.statusText);
      alert(`Error: ${errorMessage}\n\nDEBUG: Make sure backend is running on http://localhost:5164 and dev server is running on http://localhost:4200`);
      this.interviewStarted = false;
    }
  }
  // initializeQuestions(style: InterviewStyle): void {
  //   const questionSets: Record<InterviewStyle, Question[]> = {
  //     behavioral: [
  //       {
  //         id: 1,
  //         question: 'Tell me about a time when you had to solve a complex problem. What approach did you take?',
  //         category: 'Behavioral',
  //         answer: ''
  //       },
  //       {
  //         id: 2,
  //         question: 'Describe a situation where you had to work under pressure. How did you handle it?',
  //         category: 'Behavioral',
  //         answer: ''
  //       },
  //       {
  //         id: 3,
  //         question: 'Give an example of when you had to collaborate with a difficult team member. What was the outcome?',
  //         category: 'Behavioral',
  //         answer: ''
  //       },
  //       {
  //         id: 4,
  //         question: 'Tell me about a time you failed at something. What did you learn from it?',
  //         category: 'Behavioral',
  //         answer: ''
  //       },
  //       {
  //         id: 5,
  //         question: 'Describe a project where you had to learn a new technology quickly. How did you approach it?',
  //         category: 'Behavioral',
  //         answer: ''
  //       }
  //     ],
  //     technical: [
  //       {
  //         id: 1,
  //         question: 'Explain the difference between let, const, and var in JavaScript. When would you use each?',
  //         category: 'Technical',
  //         answer: ''
  //       },
  //       {
  //         id: 2,
  //         question: 'How would you optimize a slow database query? Walk me through your thought process.',
  //         category: 'Technical',
  //         answer: ''
  //       },
  //       {
  //         id: 3,
  //         question: 'Describe how you would implement a caching strategy for a web application.',
  //         category: 'Technical',
  //         answer: ''
  //       },
  //       {
  //         id: 4,
  //         question: 'What is the difference between REST and GraphQL? When would you choose one over the other?',
  //         category: 'Technical',
  //         answer: ''
  //       },
  //       {
  //         id: 5,
  //         question: 'How do you handle errors in an asynchronous JavaScript function? Provide an example.',
  //         category: 'Technical',
  //         answer: ''
  //       }
  //     ],
  //     general: [
  //       {
  //         id: 1,
  //         question: 'Why are you interested in this position?',
  //         category: 'General',
  //         answer: ''
  //       },
  //       {
  //         id: 2,
  //         question: 'What are your greatest strengths and how do they apply to this role?',
  //         category: 'General',
  //         answer: ''
  //       },
  //       {
  //         id: 3,
  //         question: 'Where do you see yourself in 5 years?',
  //         category: 'General',
  //         answer: ''
  //       },
  //       {
  //         id: 4,
  //         question: 'How do you stay updated with the latest technologies in your field?',
  //         category: 'General',
  //         answer: ''
  //       },
  //       {
  //         id: 5,
  //         question: 'What questions do you have for us?',
  //         category: 'General',
  //         answer: ''
  //       }
  //     ]
  //   };

  //   this.questions = questionSets[style];
  //   this.totalQuestions = this.questions.length;
  // }

  async recieveQuestions(): Promise<void> {
    if (!this.selectedStyle) {
      alert('Please select an interview style first.');
      return;
    }
    
    // Update newRequest with custom role and question count if provided
    if (this.customRole.trim()) {
      this.newRequest.role = this.customRole.trim();
    }
    
    if (this.customQuestionCount > 0 && this.customQuestionCount <= 10) {
      this.newRequest.questionCount = this.customQuestionCount;
      this.totalQuestions = this.customQuestionCount;
    }
    
    // Reset interview state and clear previous questions
    this.questions = [];
    this.currentQuestionIndex = 0;
    
    await this.loadQuestionsFromAPI(this.newRequest);
    
    // Only start interview if questions were successfully loaded
    if (this.questions.length > 0) {
      this.startInterview();
    }
  }

  startInterview(): void {
    this.interviewStarted = true;
    this.currentQuestionIndex = 0;
  }

  nextQuestion(): void {
    if (this.currentQuestionIndex < this.questions.length - 1) {
      this.currentQuestionIndex++;
    } else if (this.currentQuestionIndex === this.questions.length - 1) {
      // Last question answered, show completion
      this.completeInterview();
    }
  }

  completeInterview(): void {
    // Build full interviewResults from all questions (include answers even if no AI feedback)
    const defaultFeedback: QuestionFeedback = {
      overallScore: 0,
      feedback: 'No AI feedback was provided for this answer.',
      strengths: [],
      improvements: []
    };

    this.interviewResults = this.questions.map(q => ({
      question: { ...q },
      answer: q.answer,
      feedback: q.aiFeedback ? q.aiFeedback : defaultFeedback
    }));

    this.interviewComplete = true;
    this.interviewStarted = false;

    // Call summarize interview API
    this.summarizeInterviewResults();
  }

  async summarizeInterviewResults(): Promise<void> {
    try {
      // Build the summarize request
      const summarizeRequest: SummarizeInterviewRequest = {
        questions: this.questions.map(q => q.question),
        answers: this.questions.map(q => q.answer),
        feedbacks: this.interviewResults.map(r => r.feedback.feedback),
        role: this.newRequest.role,
        interviewType: this.newRequest.interviewType
      };

      console.log('Calling summarize interview API with:', summarizeRequest);
      const response: any = await firstValueFrom(
        this.interviewerService.summarizeInterview(summarizeRequest)
      );

      console.log('Interview summary response:', response);
      // Handle both string and object responses
      if (typeof response.data === 'string') {
        this.interviewSummary = response.data;
      } else if (response.data && response.data.summary) {
        this.interviewSummary = response.data.summary;
      } else {
        this.interviewSummary = JSON.stringify(response.data) || '';
      }
      console.log('Interview summary stored:', this.interviewSummary);
    } catch (error: any) {
      console.error('Failed to generate interview summary:', error);
      this.interviewSummary = 'Summary could not be generated at this time.';
    }
  }

  viewReport(): void {
    console.log('viewReport() called');
    this.showReportView = true;
  }

  previousQuestion(): void {
    if (this.currentQuestionIndex > 0) {
      this.currentQuestionIndex--;
    }
  }

  async submitAnswer(): Promise<void> {
    const currentQuestion = this.questions[this.currentQuestionIndex];
    if (!currentQuestion.answer.trim()) {
      alert('Please provide an answer before submitting.');
      return;
    }

    this.isSubmitting = true;

    try {
      const analyzeRequest: AnalyzeAnswerRequest = {
        question: currentQuestion.question,
        interviewType: this.selectedStyle || 'general',
        userAnswer: currentQuestion.answer,
        questionNumber: this.currentQuestionIndex + 1,
        totalQuestions: this.totalQuestions
      };

      console.log('Analyzing answer with request:', analyzeRequest);
      const response: any = await firstValueFrom(
        this.interviewerService.analyzeAnswer(analyzeRequest)
      );

      console.log('AI Feedback Response:', response);
      console.log('Question #' + (this.currentQuestionIndex + 1) + ' Feedback:', response.data);

      // Extract feedback from response
      const feedback: QuestionFeedback = {
        overallScore: response.data.overallScore,
        feedback: response.data.feedback,
        strengths: response.data.strengths,
        improvements: response.data.improvements
      };
      
      console.log('Extracted feedback for Q' + (this.currentQuestionIndex + 1) + ':', feedback);

      // Store feedback in current question
      currentQuestion.aiFeedback = feedback;

      // Store or update result for final report
      const existingIndex = this.interviewResults.findIndex(r => r.question.id === currentQuestion.id);
      const resultEntry = {
        question: { ...currentQuestion },
        answer: currentQuestion.answer,
        feedback: feedback
      };
      if (existingIndex >= 0) {
        this.interviewResults[existingIndex] = resultEntry;
      } else {
        this.interviewResults.push(resultEntry);
      }

      console.log('Interview results updated:', this.interviewResults);
    } catch (error: any) {
      console.error('Failed to analyze answer:', error);
      alert('Failed to analyze your answer. Please try again.');
    } finally {
      this.isSubmitting = false;
    }
  }

  // Recording functionality removed

  get currentQuestion(): Question {
    if (this.questions.length === 0) {
      return { id: 0, question: '', category: '', answer: '' };
    }
    return this.questions[this.currentQuestionIndex];
  }

  get progress(): number {
    return ((this.currentQuestionIndex + 1) / this.totalQuestions) * 100;
  }

  getAverageScore(): number {
    const scored = this.interviewResults.filter(r => r.feedback && r.feedback.overallScore > 0);
    if (scored.length === 0) return 0;
    const total = scored.reduce((sum, result) => sum + result.feedback.overallScore, 0);
    return total / scored.length;
  }

  getHighestScore(): number {
    if (this.interviewResults.length === 0) return 0;
    return Math.max(...this.interviewResults.map(result => result.feedback.overallScore));
  }

  resetInterview(): void {
    this.questions = [];
    this.interviewResults = [];
    this.interviewComplete = false;
    this.interviewStarted = false;
    this.showReportView = false;
    this.currentQuestionIndex = 0;
    this.selectedStyle = null;
    this.interviewSummary = '';
    this.customRole = '';
    this.customQuestionCount = 5;
  }

  downloadPDF(): void {
    window.print();
  }

  async exportAsPdf(): Promise<void> {
    const html2canvas = (await import('html2canvas')).default;
    const jsPDF = (await import('jspdf')).jsPDF;
    
    try {
      const el = document.getElementById('ai-interview-report-root');
      if (!el) {
        alert('Report element not found');
        return;
      }

      // Capture the full scrollable content
      const canvas = await html2canvas(el, {
        scale: 1,
        useCORS: true,
        logging: false,
        allowTaint: true,
        backgroundColor: 'var(--background)'
      });

      const imgData = canvas.toDataURL('image/png');
      const canvasWidth = canvas.width;
      const canvasHeight = canvas.height;

      // Create PDF
      const pdf = new jsPDF({
        orientation: 'portrait',
        unit: 'pt',
        format: 'a4'
      });

      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const margin = 15;
      const contentWidth = pageWidth - 2 * margin;

      // Calculate image dimensions on PDF
      const scaleFactor = contentWidth / canvasWidth;
      const scaledImageHeight = canvasHeight * scaleFactor;
      const pageContentHeight = pageHeight - 2 * margin;

      // Calculate pages needed
      const numPages = Math.ceil(scaledImageHeight / pageContentHeight);

      // Load the image once
      const img = new Image();
      img.onload = () => {
        // Add pages with proper vertical slicing
        for (let pageIdx = 0; pageIdx < numPages; pageIdx++) {
          if (pageIdx > 0) {
            pdf.addPage();
          }

          // Calculate source image slice for this page
          const sourceYStart = (pageIdx * pageContentHeight) / scaleFactor;
          const sourceYEnd = Math.min(((pageIdx + 1) * pageContentHeight) / scaleFactor, canvasHeight);
          const sourceHeight = sourceYEnd - sourceYStart;

          // Create temporary canvas for this page slice
          const sliceCanvas = document.createElement('canvas');
          sliceCanvas.width = canvasWidth;
          sliceCanvas.height = sourceHeight;
          
          const ctx = sliceCanvas.getContext('2d');
          if (ctx) {
            ctx.drawImage(
              img,
              0, sourceYStart,
              canvasWidth, sourceHeight,
              0, 0,
              canvasWidth, sourceHeight
            );
          }

          const sliceImgData = sliceCanvas.toDataURL('image/png');
          const sliceHeight = (sourceHeight / canvasWidth) * contentWidth;

          pdf.addImage(sliceImgData, 'PNG', margin, margin, contentWidth, sliceHeight);
        }

        pdf.save(`ai-interview-report-${Date.now()}.pdf`);
      };
      img.onerror = () => {
        alert('Failed to load image for PDF export.');
      };
      img.src = imgData;
    } catch (e) {
      console.error('PDF export failed', e);
      console.error('Full error:', e);
      alert('PDF export failed. Try using the Print dialog instead (Ctrl+P or Cmd+P).');
    }
  }

  exportAsJson(): void {
    try {
      const data = {
        role: this.newRequest.role,
        interviewType: this.newRequest.interviewType,
        totalQuestions: this.totalQuestions,
        answeredCount: this.interviewResults.filter(r => r.answer && r.answer.trim().length > 0).length,
        averageScore: this.getAverageScore(),
        highestScore: this.getHighestScore(),
        interviewSummary: this.interviewSummary,
        results: this.interviewResults.map((r, i) => ({
          questionNumber: i + 1,
          question: r.question.question,
          category: r.question.category,
          userAnswer: r.answer,
          score: r.feedback?.overallScore || 0,
          feedback: r.feedback?.feedback || '',
          strengths: r.feedback?.strengths || [],
          improvements: r.feedback?.improvements || []
        })),
        exportedAt: new Date().toISOString()
      };
      const json = JSON.stringify(data, null, 2);
      const blob = new Blob([json], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `ai-interview-report-${Date.now()}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error('JSON export failed', e);
      alert('JSON export failed.');
    }
  }

  exportAsCsv(): void {
    try {
      let csv = 'Interview Summary\n';
      csv += this.interviewSummary + '\n\n';
      csv += 'Question #,Question,Category,User Answer,Score (0-10),Feedback,Strengths,Improvements\n';
      this.interviewResults.forEach((r, i) => {
        const question = this.escapeCsv(r.question.question);
        const category = r.question.category;
        const answer = this.escapeCsv(r.answer);
        const score = r.feedback?.overallScore || 0;
        const feedback = this.escapeCsv(r.feedback?.feedback || '');
        const strengths = this.escapeCsv((r.feedback?.strengths || []).join('; '));
        const improvements = this.escapeCsv((r.feedback?.improvements || []).join('; '));
        csv += `${i + 1},"${question}","${category}","${answer}",${score},"${feedback}","${strengths}","${improvements}"\n`;
      });
      const blob = new Blob([csv], { type: 'text/csv' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `ai-interview-report-${Date.now()}.csv`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error('CSV export failed', e);
      alert('CSV export failed.');
    }
  }

  private escapeCsv(str: string): string {
    return str.replace(/"/g, '""');
  }

  getAnsweredCount(): number {
    return this.interviewResults.filter(r => r.answer && r.answer.trim().length > 0).length;
  }

  getAnsweredPercentage(): number {
    if (this.totalQuestions === 0) return 0;
    return Math.round((this.getAnsweredCount() / this.totalQuestions) * 100);
  }

  Math = Math;

  onBack(): void {
    this.router.navigate(['/features']);
  }
}
