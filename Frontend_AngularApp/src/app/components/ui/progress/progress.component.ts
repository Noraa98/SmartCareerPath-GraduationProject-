import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { cn } from '../../../utils';

@Component({
  selector: 'app-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './progress.component.html',
  styleUrl: './progress.component.css'
})
export class ProgressComponent {
  @Input() value: number = 0;
  @Input() className: string = '';

  get progressClasses(): string {
    return cn(
      'bg-primary/20 relative h-2 w-full overflow-hidden rounded-full',
      this.className
    );
  }

  get indicatorStyle(): { transform: string } {
    return {
      transform: `translateX(-${100 - (this.value || 0)}%)`
    };
  }
}

