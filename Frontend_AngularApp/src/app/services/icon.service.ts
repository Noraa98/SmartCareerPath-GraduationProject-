import { Injectable } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { icons } from '../utils/icons';

@Injectable({
  providedIn: 'root'
})
export class IconService {
  constructor(private sanitizer: DomSanitizer) {}

  getIcon(iconName: string): SafeHtml {
    const iconKey = iconName as keyof typeof icons;
    const iconSvg = icons[iconKey] || '';
    return this.sanitizer.bypassSecurityTrustHtml(iconSvg);
  }

  getIconHtml(iconName: string): string {
    const iconKey = iconName as keyof typeof icons;
    return icons[iconKey] || '';
  }
}

