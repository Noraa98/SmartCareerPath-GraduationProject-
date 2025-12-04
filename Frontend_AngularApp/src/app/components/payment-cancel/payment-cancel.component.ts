import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ButtonComponent } from '../ui/button/button.component';
import { CardComponent, CardContentComponent } from '../ui/card/card.component';
import { IconService } from '../../services/icon.service';
import { PaymentService } from '../../services/payment.service';

@Component({
  selector: 'app-payment-cancel',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonComponent, CardComponent, CardContentComponent],
  templateUrl: './payment-cancel.component.html',
  styleUrl: './payment-cancel.component.css'
})
export class PaymentCancelComponent {
  constructor(private router: Router, public iconService: IconService, private payment: PaymentService) {}

  clearAndGoPlans(): void {
    // Clear all payment-related session data when user cancels
    try { sessionStorage.removeItem('payment_transaction'); } catch {}
    try { sessionStorage.removeItem('payment_transaction_raw'); } catch {}
    try { sessionStorage.removeItem('stripeSessionId'); } catch {}
    // Clear the tracked checkout session
    this.payment.clearTrackedSessionId();
    console.log('[PaymentCancel] Cleared payment session data; redirecting to plans');
    this.router.navigate(['/plans']);
  }

  goHome(): void {
    // Clear all payment-related session data when user goes home
    try { sessionStorage.removeItem('payment_transaction'); } catch {}
    try { sessionStorage.removeItem('payment_transaction_raw'); } catch {}
    try { sessionStorage.removeItem('stripeSessionId'); } catch {}
    // Clear the tracked checkout session
    this.payment.clearTrackedSessionId();
    console.log('[PaymentCancel] Cleared payment session data; redirecting to home');
    this.router.navigate(['/home']);
  }
}
