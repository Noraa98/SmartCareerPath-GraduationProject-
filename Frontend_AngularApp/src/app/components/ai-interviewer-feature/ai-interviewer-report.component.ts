import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { ButtonComponent } from '../ui/button/button.component';
import { IconService } from '../../services/icon.service';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';

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

@Component({
  selector: 'app-ai-interviewer-report',
  standalone: true,
  imports: [CommonModule, CardComponent, CardContentComponent, ButtonComponent],
  templateUrl: './ai-interviewer-report.component.html',
  styleUrls: ['./ai-interviewer-report.component.css']
})
export class AIInterviewerReportComponent implements OnInit {
  results: InterviewResult[] = [];
  role: string = '';
  interviewType: string = '';
  totalQuestions: number = 0;
  Math = Math;
  now: string = new Date().toLocaleString();

  constructor(
    private router: Router,
    public iconService: IconService
  ) {}

  ngOnInit(): void {
    console.log('AIInterviewerReportComponent initialized');
    try {
      const raw = sessionStorage.getItem('aiInterviewReport');
      console.log('sessionStorage data:', raw);
      if (!raw) {
        console.warn('No report data found in sessionStorage');
        return;
      }
      const parsed = JSON.parse(raw);
      this.results = parsed.results || [];
      this.role = parsed.role || '';
      this.interviewType = parsed.interviewType || '';
      this.totalQuestions = parsed.totalQuestions || this.results.length;
      console.log('Report loaded with', this.results.length, 'results');
    } catch (e) {
      console.error('Failed to load report:', e);
    }
  }

  getAverageScore(): number {
    const scored = this.results.filter(r => r.feedback?.overallScore > 0);
    if (scored.length === 0) return 0;
    const total = scored.reduce((sum, r) => sum + (r.feedback?.overallScore || 0), 0);
    return total / scored.length;
  }

  getHighestScore(): number {
    if (this.results.length === 0) return 0;
    return Math.max(...this.results.map(r => r.feedback?.overallScore || 0));
  }

  downloadPDF(): void {
    // Fallback: open print dialog
    window.print();
  }

  async exportAsPdf(): Promise<void> {
    try {
      const el = document.getElementById('ai-interview-report-root');
      if (!el) {
        alert('Report element not found');
        return;
      }

      // Render to canvas
      const canvas = await html2canvas(el, { scale: 2 });
      const imgData = canvas.toDataURL('image/png');
      const pdf = new jsPDF({ orientation: 'portrait', unit: 'pt', format: 'a4' });

      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();

      const imgProps = (pdf as any).getImageProperties(imgData);
      const imgWidth = pageWidth - 40; // margins
      const imgHeight = (imgProps.height * imgWidth) / imgProps.width;

      let position = 20;
      pdf.addImage(imgData, 'PNG', 20, position, imgWidth, imgHeight);
      // If content overflows one page, add pages (basic handling)
      if (imgHeight > pageHeight - 40) {
        // split by page height
        // For large content, just scale down to fit single page
      }

      pdf.save(`ai-interview-report-${Date.now()}.pdf`);
    } catch (e) {
      console.error('PDF export failed', e);
      alert('PDF export failed. You can use the Print dialog as a fallback.');
    }
  }

  exportAsJson(): void {
    try {
      const data = {
        role: this.role,
        interviewType: this.interviewType,
        totalQuestions: this.totalQuestions,
        answeredCount: this.results.filter(r => r.answer && r.answer.trim().length > 0).length,
        averageScore: this.results.filter(r => r.feedback?.overallScore > 0).length > 0 
          ? this.results.filter(r => r.feedback?.overallScore > 0).reduce((sum, r) => sum + (r.feedback?.overallScore || 0), 0) / this.results.filter(r => r.feedback?.overallScore > 0).length
          : 0,
        highestScore: this.results.length > 0 ? Math.max(...this.results.map(r => r.feedback?.overallScore || 0)) : 0,
        results: this.results.map((r, i) => ({
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
      let csv = 'Question #,Question,Category,User Answer,Score (0-10),Feedback,Strengths,Improvements\n';
      this.results.forEach((r, i) => {
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
    // Remove quotes and escape properly for CSV
    return str.replace(/"/g, '""');
  }

  onBack(): void {
    this.router.navigate(['/ai-interviewer']);
  }
}
