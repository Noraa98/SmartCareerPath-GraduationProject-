import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { cn } from '../../../utils';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {
  @Input() className: string = '';

  get cardClasses(): string {
return cn(
  'bg-card text-card-foreground rounded-xl border',
  this.className
);


  }
}

@Component({
  selector: 'app-card-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      [class]="cn('@container/card-header grid auto-rows-min grid-rows-[auto_auto] items-start gap-1.5 px-6 pt-6 has-data-[slot=card-action]:grid-cols-[1fr_auto] [.border-b]:pb-6', className)"
      data-slot="card-header">
      <ng-content></ng-content>
    </div>
  `,
  styles: []
})
export class CardHeaderComponent {
  @Input() className: string = '';
  cn = cn;
}

@Component({
  selector: 'app-card-title',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h4 [class]="cn('leading-none', className)" data-slot="card-title">
      <ng-content></ng-content>
    </h4>
  `,
  styles: []
})
export class CardTitleComponent {
  @Input() className: string = '';
  cn = cn;
}

@Component({
  selector: 'app-card-description',
  standalone: true,
  imports: [CommonModule],
  template: `
    <p [class]="cn('text-muted-foreground', className)" data-slot="card-description">
      <ng-content></ng-content>
    </p>
  `,
  styles: []
})
export class CardDescriptionComponent {
  @Input() className: string = '';
  cn = cn;
}

@Component({
  selector: 'app-card-content',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cn('px-6 [&:last-child]:pb-6', className)" data-slot="card-content">
      <ng-content></ng-content>
    </div>
  `,
  styles: []
})
export class CardContentComponent {
  @Input() className: string = '';
  cn = cn;
}

@Component({
  selector: 'app-card-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="cn('flex items-center px-6 pb-6 [.border-t]:pt-6', className)" data-slot="card-footer">
      <ng-content></ng-content>
    </div>
  `,
  styles: []
})
export class CardFooterComponent {
  @Input() className: string = '';
  cn = cn;
}

