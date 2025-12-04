import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private isCollapsedSubject = new BehaviorSubject<boolean>(false);
  public isCollapsed$: Observable<boolean> = this.isCollapsedSubject.asObservable();

  private isVisibleSubject = new BehaviorSubject<boolean>(false);
  public isVisible$: Observable<boolean> = this.isVisibleSubject.asObservable();

  private readonly expandedWidth = '16rem';
  private readonly collapsedWidth = '4rem';

  toggle(): void {
    this.isCollapsedSubject.next(!this.isCollapsedSubject.value);
  }

  collapse(): void {
    this.isCollapsedSubject.next(true);
  }

  expand(): void {
    this.isCollapsedSubject.next(false);
  }

  show(): void {
    this.isVisibleSubject.next(true);
  }

  hide(): void {
    this.isVisibleSubject.next(false);
  }

  toggleVisibility(): void {
    this.isVisibleSubject.next(!this.isVisibleSubject.value);
  }

  getWidth(): string {
    return this.isCollapsedSubject.value ? this.collapsedWidth : this.expandedWidth;
  }
}

