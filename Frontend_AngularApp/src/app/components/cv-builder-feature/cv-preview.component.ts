import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { IconService } from '../../services/icon.service';

@Component({
  selector: 'app-cv-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cv-preview.component.html',
  styleUrls: ['./cv-preview.component.css']
})
export class CVPreviewComponent implements OnInit {
  cvData: any = null;
  currentDate: string = '';

  constructor(
    private router: Router,
    public iconService: IconService
  ) {
    this.currentDate = new Date().toLocaleDateString();
  }

  ngOnInit(): void {
    const raw = sessionStorage.getItem('cvPreviewData');
    if (raw) {
      try {
        this.cvData = JSON.parse(raw);
      } catch (e) {
        console.error('Failed to parse cvPreviewData from sessionStorage', e);
      }
    }
  }

  onBack(): void {
    this.router.navigate(['/cv-builder']);
  }

  onPrint(): void {
    window.print();
  }

  async downloadPdf(): Promise<void> {
    try {
      const jsPDF = (await import('jspdf')).jsPDF;
      const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });

      const pageWidth = doc.internal.pageSize.getWidth();
      const pageHeight = doc.internal.pageSize.getHeight();
      const margin = 12;
      const contentWidth = pageWidth - 2 * margin;
      let y = margin;

      // Define colors as tuples
      const primaryColor: [number, number, number] = [41, 128, 185]; // Professional blue
      const secondaryColor: [number, number, number] = [52, 73, 94]; // Dark slate
      const accentColor: [number, number, number] = [46, 204, 113]; // Green accent
      const textColor: [number, number, number] = [44, 62, 80]; // Dark text
      const lightText: [number, number, number] = [127, 140, 141]; // Light gray text

      // ===== PAGE 1: HEADER & SUMMARY =====
      
      // Header with background
      doc.setFillColor(primaryColor[0], primaryColor[1], primaryColor[2]);
      doc.rect(0, 0, pageWidth, 45, 'F');

      // Name
      doc.setTextColor(255, 255, 255);
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(28);
      doc.text((this.cvData?.fullName || 'Your Name').toUpperCase(), margin + 5, 18);

      // Desired Role
      doc.setFont('helvetica', '');
      doc.setFontSize(12);
      doc.text((this.cvData?.desiredJobRole || 'Professional'), margin + 5, 28);

      // Contact Info in header
      doc.setTextColor(220, 220, 220);
      doc.setFontSize(9);
      const contactInfo = `${this.cvData?.email || 'email@example.com'} • ${this.cvData?.phoneNumber || '+1 (000) 000-0000'}`;
      doc.text(contactInfo, margin + 5, 37);

      y = 52;

      // Professional Summary Section
      doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(11);
      doc.text('PROFESSIONAL SUMMARY', margin, y);

      // Divider line
      doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
      doc.setLineWidth(0.5);
      doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

      y += 6;
      doc.setTextColor(textColor[0], textColor[1], textColor[2]);
      doc.setFont('helvetica', '');
      doc.setFontSize(10);
      const summaryText = this.cvData?.professionalSummary || 'A technically focused, results-driven professional with proven expertise in relevant domains. Strong track record of delivering high-quality results through strategic planning, technical excellence, and effective collaboration.';
      const summaryLines = doc.splitTextToSize(summaryText, contentWidth);
      const summaryHeight = doc.getTextDimensions(summaryLines.join('\n')).h;
      doc.text(summaryLines, margin, y);
      
      // Move y position to after the summary text plus extra spacing
      y += summaryHeight + 28;

      // ===== WORK EXPERIENCE SECTION =====
      if (this.cvData?.workExperiences && this.cvData.workExperiences.length > 0) {
        doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('WORK EXPERIENCE', margin, y);

        doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
        doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

        y += 6;

        for (const exp of this.cvData.workExperiences) {
          if (!exp.role && !exp.companyName) continue;

          // Job Title & Company
          doc.setTextColor(secondaryColor[0], secondaryColor[1], secondaryColor[2]);
          doc.setFont('helvetica', 'bold');
          doc.setFontSize(10);
          const jobTitle = `${(exp.role || 'Position').toUpperCase()} | ${(exp.companyName || 'Company').toUpperCase()}`;
          doc.text(jobTitle, margin, y);

          // Dates
          doc.setTextColor(lightText[0], lightText[1], lightText[2]);
          doc.setFont('helvetica', 'italic');
          doc.setFontSize(9);
          const dateRange = `${exp.startDate || 'Start Date'} — ${exp.endDate || 'Present'}`;
          doc.text(dateRange, margin, y + 4);

          y += 8;

          // Description (bullet points)
          if (exp.description) {
            doc.setTextColor(textColor[0], textColor[1], textColor[2]);
            doc.setFont('helvetica', '');
            doc.setFontSize(9.5);
            const descLines = doc.splitTextToSize(`• ${exp.description}`, contentWidth - 4);
            doc.text(descLines, margin + 2, y);
            y += descLines.length * 3.5 + 2;
          }

          y += 2;

          // Check if we need a new page
          if (y > pageHeight - 25) {
            doc.addPage();
            y = margin;
          }
        }

        y += 2;
      }

      // ===== EDUCATION SECTION =====
      if (this.cvData?.education) {
        doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('EDUCATION', margin, y);

        doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
        doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

        y += 6;
        doc.setTextColor(textColor[0], textColor[1], textColor[2]);
        doc.setFont('helvetica', '');
        doc.setFontSize(10);
        const eduLines = doc.splitTextToSize(this.cvData.education, contentWidth);
        doc.text(eduLines, margin, y);
        y += eduLines.length * 4 + 3;

        y += 10;

        if (y > pageHeight - 25) {
          doc.addPage();
          y = margin;
        }
      }

      // ===== CERTIFICATIONS SECTION =====
      if (this.cvData?.certifications) {
        doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('CERTIFICATIONS', margin, y);

        doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
        doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

        y += 6;
        doc.setTextColor(textColor[0], textColor[1], textColor[2]);
        doc.setFont('helvetica', '');
        doc.setFontSize(10);
        const certLines = doc.splitTextToSize(this.cvData.certifications, contentWidth);
        doc.text(certLines, margin, y);
        y += certLines.length * 4 + 3;

        y += 10;

        if (y > pageHeight - 25) {
          doc.addPage();
          y = margin;
        }
      }

      // ===== SKILLS SECTION =====
      if (this.cvData?.skills) {
        doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('SKILLS & EXPERTISE', margin, y);

        doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
        doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

        y += 6;
        doc.setTextColor(textColor[0], textColor[1], textColor[2]);
        doc.setFont('helvetica', '');
        doc.setFontSize(10);
        
        // Format skills as bullets or comma-separated
        const skillsList = this.cvData.skills
          .split(/[,\n]/)
          .map((s: string) => s.trim())
          .filter((s: string) => s.length > 0);
        
        if (skillsList.length > 0) {
          let skillsText = skillsList.map((skill: string) => `• ${skill}`).join('\n');
          const skillsLines = doc.splitTextToSize(skillsText, contentWidth - 2);
          doc.text(skillsLines, margin + 2, y);
          y += skillsLines.length * 3.5 + 2;
        }

        y += 10;

        if (y > pageHeight - 25) {
          doc.addPage();
          y = margin;
        }
      }

      // ===== LANGUAGES SECTION =====
      if (this.cvData?.languages) {
        doc.setTextColor(primaryColor[0], primaryColor[1], primaryColor[2]);
        doc.setFont('helvetica', 'bold');
        doc.setFontSize(11);
        doc.text('LANGUAGES', margin, y);

        doc.setDrawColor(accentColor[0], accentColor[1], accentColor[2]);
        doc.line(margin, y + 1.5, margin + contentWidth, y + 1.5);

        y += 6;
        doc.setTextColor(textColor[0], textColor[1], textColor[2]);
        doc.setFont('helvetica', '');
        doc.setFontSize(10);
        
        const langsList = this.cvData.languages
          .split(/[,\n]/)
          .map((l: string) => l.trim())
          .filter((l: string) => l.length > 0);
        
        if (langsList.length > 0) {
          let langsText = langsList.map((lang: string) => `• ${lang}`).join('\n');
          const langsLines = doc.splitTextToSize(langsText, contentWidth - 2);
          doc.text(langsLines, margin + 2, y);
          y += langsLines.length * 3.5 + 2;
        }

        y += 8;
      }

      // ===== FOOTER =====
      doc.setTextColor(lightText[0], lightText[1], lightText[2]);
      doc.setFont('helvetica', 'italic');
      doc.setFontSize(8);
      const pageCount = doc.internal.pages.length - 1; // Exclude the initial page
      for (let i = 1; i <= pageCount; i++) {
        doc.setPage(i);
        doc.text(
          `Page ${i} of ${pageCount}`,
          pageWidth / 2,
          pageHeight - 8,
          { align: 'center' }
        );
      }

      doc.save(`CV-${(this.cvData?.fullName || 'resume').replace(/\s+/g, '_')}-${Date.now()}.pdf`);
    } catch (e) {
      console.error('PDF export failed', e);
      alert('PDF export failed. You can use the Print dialog (Ctrl+P or Cmd+P) as a fallback.');
    }
  }

  // Helper for multiline text in jsPDF
  addMultilineText(doc: any, text: string, x: number, y: number, maxWidth: number, lineHeight: number): number {
    if (!text) return y;
    const lines = doc.splitTextToSize(text, maxWidth);
    for (const line of lines) {
      doc.text(line, x, y);
      y += lineHeight;
    }
    return y;
  }
}
