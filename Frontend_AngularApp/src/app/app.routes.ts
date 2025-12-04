import { Routes } from '@angular/router';
import { RegistrationPageComponent } from './components/registration-page/registration-page.component';
import { InterestsPageComponent } from './components/interests-page/interests-page.component';
import { PersonalityQuizComponent } from './components/personality-quiz/personality-quiz.component';
import { RecommendationsPageComponent } from './components/recommendations-page/recommendations-page.component';
import { FeaturesHubComponent } from './components/features-hub/features-hub.component';
import { CVBuilderFeatureComponent } from './components/cv-builder-feature/cv-builder-feature.component';
import { CVPreviewComponent } from './components/cv-builder-feature/cv-preview.component';
import { JobParserFeatureComponent } from './components/job-parser-feature/job-parser-feature.component';
import { AIInterviewerFeatureComponent } from './components/ai-interviewer-feature/ai-interviewer-feature.component';
import { AIInterviewerReportComponent } from './components/ai-interviewer-feature/ai-interviewer-report.component';
import { LoginPageComponent } from './components/login-page/login-page.component';
import { AuthGuard } from './guards/auth.guard';
import { PremiumGuard } from './guards/premium.guard';
import { HomePageComponent } from './components/home-page/home-page.component';
import { PlansPageComponent } from './components/plans-page/plans-page.component';
import { PaymentSuccessComponent } from './components/payment-success/payment-success.component';
import { PaymentCancelComponent } from './components/payment-cancel/payment-cancel.component';
import { ChangePasswordComponent } from './components/change-password/change-password.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { VerifyEmailComponent } from './auth/verify-email/verify-email.component';




export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomePageComponent },
  { path: 'registration', component: RegistrationPageComponent },
  { path: 'login', component: LoginPageComponent },
  { path: 'verify-email', component: VerifyEmailComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'interests', component: InterestsPageComponent, canActivate: [AuthGuard] },
  { path: 'quiz', component: PersonalityQuizComponent, canActivate: [AuthGuard] },
  { path: 'features', component: FeaturesHubComponent, canActivate: [AuthGuard] },
  { path: 'cv-builder', component: CVBuilderFeatureComponent, canActivate: [AuthGuard] },
  { path: 'cv-builder/preview', component: CVPreviewComponent, canActivate: [AuthGuard] },
  { path: 'job-parser', component: JobParserFeatureComponent, canActivate: [PremiumGuard] },
  { path: 'ai-interviewer', component: AIInterviewerFeatureComponent, canActivate: [PremiumGuard] },
  { path: 'ai-interviewer/report', component: AIInterviewerReportComponent, canActivate: [PremiumGuard] },
  { path: 'recommendations', component: RecommendationsPageComponent, canActivate: [AuthGuard] },
  { path: 'plans', component: PlansPageComponent },
  { path: 'payment/success', component: PaymentSuccessComponent },
  { path: 'payment/cancel', component: PaymentCancelComponent },
  { path: 'paymob/response', component: PaymentSuccessComponent },
  { path: 'paymob/cancel', component: PaymentCancelComponent },
  { path: 'change-password', component: ChangePasswordComponent, canActivate: [AuthGuard] },
  { path: 'career-recommendation', component: InterestsPageComponent },
  { path: '**', redirectTo: '/home' }
];

