import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { cn } from '../../../utils';

@Component({
  selector: 'app-label',
  standalone: true,
  imports: [CommonModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => LabelComponent),
      multi: true
    }
  ],
  templateUrl: './label.component.html',
  styleUrl: './label.component.css'
})
export class LabelComponent implements ControlValueAccessor {
  @Input() className: string = '';
  @Input() htmlFor: string = '';

  value: any;
  onChange = () => {};
  onTouched = () => {};

  get labelClasses(): string {
    return cn(
      'flex items-center gap-2 text-sm leading-none font-medium select-none group-data-[disabled=true]:pointer-events-none group-data-[disabled=true]:opacity-50 peer-disabled:cursor-not-allowed peer-disabled:opacity-50',
      this.className
    );
  }

  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
}

