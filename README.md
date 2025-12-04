#  Smart Career Path Recommender

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-18-DD0031?style=flat-square&logo=angular)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.3-3178C6?style=flat-square&logo=typescript)](https://www.typescriptlang.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

> **Full-stack AI-powered career guidance platform** built with .NET 9 and Angular 18, featuring intelligent career recommendations, CV generation, interview simulation, and job parsing capabilities.

---

##  Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Installation](#installation)
- [API Documentation](#api-documentation)
- [Screenshots](#screenshots)
- [Developer](#developer)

---

##  Overview

Smart Career Path Recommender is a comprehensive career development platform that integrates AI to provide personalized career guidance, professional CV generation, interview simulations, and job description parsing. Built from the ground up using clean architecture principles.

### **Core Capabilities**
-  **AI-powered career recommendations** based on user quiz responses
-  **Automated CV generation** with professional formatting
-  **Text-based interview simulator** with AI evaluation
-  **Job description parsing** and requirement extraction
-  **Secure authentication** with JWT
-  **Multi-provider payment integration** (PayPal, Paymob)

---

##  Key Features

###  **Career Path Recommendation Engine**
- **Intelligent Quiz System**: Multi-dimensional assessment covering technical and soft skills
- **AI-Powered Analysis**: Integration with AI models to analyze quiz responses and generate career recommendations
- **Confidence Scoring**: Probability scores for each recommended career path
- **Learning Recommendations**: Suggested courses and resources aligned with career goals

###  **AI CV Builder**
- **AI Content Generation**: Leverages AI models to create professional summaries and experience descriptions
- **Form-based Input**: User-friendly forms for entering personal info, education, experience, projects, and skills
- **PDF Export**: Professional PDF generation with clean formatting
- **Single Template**: Clean, professional format supporting both Arabic and English
- **Real-time Preview**: See changes as you build your CV

###  **AI Interview Simulator**
- **Multi-format Interviews**: Technical, HR, and Behavioral question types
- **Chat-based Interface**: Text-only conversation format
- **AI Evaluation**: Automated scoring and feedback on answers
- **Performance Reports**: Detailed analysis of interview performance
- **Question Bank**: Curated interview questions with AI-generated variations

###  **Job Description Parser**
- **Text Extraction**: Parse job descriptions to extract key information
- **Skills Recognition**: Identify required skills and qualifications
- **Structured Output**: Organized display of role title, responsibilities, and requirements
- **Manual Analysis**: User-friendly interface for reviewing parsed data

###  **Security & Authentication**
- **JWT Authentication**: Token-based authentication system
- **Password Security**: Secure password hashing with bcrypt
- **Email Verification**: Account activation via email confirmation
- **Password Reset**: Secure token-based password reset flow
- **Role-based Access**: User and Premium account levels

###  **Payment Integration**
- **Multi-provider Support**: PayPal and Paymob integration
- **Strategy Pattern**: Flexible payment provider architecture
- **Webhook Handling**: Secure payment verification
- **Subscription Plans**: Monthly and yearly billing options
- **Transaction History**: Complete payment tracking
- **Refund Management**: Manual refund processing workflow

---

##  Technology Stack

### **Backend (.NET 9)**

#### **Core Frameworks**
- **ASP.NET Core 9.0**: Web API framework
- **Entity Framework Core 9.0**: ORM for database operations
- **AutoMapper 13.0**: Object-to-object mapping
- **FluentValidation 11.9**: Request validation

#### **Authentication & Security**
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT authentication
- **BCrypt.Net-Next**: Password hashing

#### **Database**
- **Microsoft SQL Server**: Primary database
- **SQL Server Management Studio**: Database management

#### **AI Integration**
- **Pre-trained AI Models**: Integration with existing AI APIs for text generation and analysis
- **RESTful API Clients**: HTTP clients for AI service communication

#### **Payment**
- **PayPal SDK**: PayPal payment processing
- **Paymob Integration**: Egyptian market payment gateway

#### **Testing**
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library

#### **Utilities**
- **Serilog**: Structured logging
- **Swashbuckle (Swagger)**: API documentation

### **Frontend (Angular 18)**

#### **Core Framework**
- **Angular 18**: SPA framework
- **TypeScript 5.3**: Type-safe development
- **RxJS 7.8**: Reactive programming

#### **UI Components**
- **Angular Material 17**: Material Design components
- **Bootstrap 5**: Responsive grid and utilities
- **Tailwind CSS 3.4**: Utility-first styling

#### **Form Handling**
- **Angular Reactive Forms**: Form management
- **Custom Validators**: Business logic validation

#### **Payment UI**
- **PayPal JavaScript SDK**: PayPal buttons and checkout

#### **Utilities**
- **HttpClient**: API communication
- **ngx-toastr**: Toast notifications
- **RxJS Operators**: State management

### **Development Tools**

- **Visual Studio 2022**: Backend IDE
- **Visual Studio Code**: Frontend IDE
- **Postman**: API testing
- **Git & GitHub**: Version control
- **SQL Server Management Studio**: Database tools

---

##  Architecture

### **Clean Architecture (Onion Architecture)**

```
┌─────────────────────────────────────────────────────────┐
│              Presentation Layer (APIs)                   │
│         Controllers, DTOs, Request/Response             │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Application Layer                           │
│       Services, Business Logic, Validation              │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                Domain Layer                              │
│        Entities, Enums, Value Objects                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│           Infrastructure Layer                           │
│    Data Access, EF Core, External Services              │
└─────────────────────────────────────────────────────────┘
```

### **Design Patterns**
- **Repository Pattern**: Data access abstraction
- **Unit of Work Pattern**: Transaction management
- **Strategy Pattern**: Payment provider switching
- **Factory Pattern**: Object creation
- **Result Pattern**: Consistent error handling
- **Dependency Injection**: Loose coupling

### **Project Structure**

```
SmartCareer.sln
├── SmartCareer.APIs                    # REST API
│   ├── Controllers/
│   ├── Middleware/
│   └── Program.cs
│
├── SmartCareer.Application             # Business Logic
│   ├── Services/
│   ├── Validators/
│   └── Mapping/
│
├── SmartCareer.Application.Abstraction # Contracts
│   ├── DTOs/
│   ├── Services/
│   └── Common/
│
├── SmartCareer.Domain                  # Domain Models
│   ├── Entities/
│   ├── Enums/
│   └── Contracts/
│
└── SmartCareer.Infrastructure          # Data & External
    └── Persistence/
        ├── Data/
        ├── Repositories/
        └── Configurations/
```

---

##  AI Integration

### **AI Model Integration**
- Integration with pre-trained AI models via API calls
- Prompt engineering for career recommendations
- Text generation for CV content enhancement
- Interview answer evaluation and scoring
- Job description parsing and extraction

### **Implementation Approach**
```csharp
// Example: Career recommendation using AI
public async Task<RecommendationDto> GenerateRecommendation(QuizResultDto quiz)
{
    var prompt = BuildCareerPrompt(quiz);
    var aiResponse = await _aiClient.GenerateCompletion(prompt);
    return ParseRecommendation(aiResponse);
}
```

---

##  Installation

### **Prerequisites**
- .NET 9 SDK
- Node.js 20+ & npm
- SQL Server 2019+
- AI API access credentials
- Payment provider accounts

### **Backend Setup**

```bash
# Clone repository
git clone https://github.com/noura-ahmed/smart-career-recommender.git
cd smart-career-recommender/backend

# Restore packages
dotnet restore

# Update connection string in appsettings.json
# Apply migrations
dotnet ef database update --project SmartCareer.Infrastructure.Persistence --startup-project SmartCareer.APIs

# Run application
dotnet run --project SmartCareer.APIs
```

### **Frontend Setup**

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Update API URL in environment.ts
# Run development server
ng serve

# Build for production
ng build --configuration production
```

### **Configuration**

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SmartCareer;Trusted_Connection=True;"
  },
  "JWT": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "SmartCareer",
    "Audience": "SmartCareerUsers",
    "ExpiryMinutes": 60
  },
  "AI": {
    "ApiUrl": "your-ai-api-url",
    "ApiKey": "your-api-key"
  },
  "PayPal": {
    "ClientId": "your-paypal-client-id",
    "ClientSecret": "your-paypal-secret"
  },
  "Paymob": {
    "ApiKey": "your-paymob-key",
    "IntegrationId": "your-integration-id"
  }
}
```

---
## Frontend (Angular 18)

The frontend of **Smart Career Path Recommender** is built entirely with **Angular 18** and provides a modern, responsive, and user-friendly interface for all platform features.

### **Key Features**

* **Dashboard**: Overview of user activities, recommendations, and CV status
* **Career Quiz Interface**: Interactive quiz with real-time AI feedback
* **CV Builder**: Form-based input with real-time preview and PDF export
* **Interview Simulator**: Chat-based interface for AI-driven interview practice
* **Job Parser**: Upload or paste job descriptions to extract structured information
* **User Profile & Settings**: Manage account, subscription, and personal data
* **Responsive UI**: Optimized for desktop, tablet, and mobile devices
* **Notifications**: Toast alerts for actions and errors using ngx-toastr
* **Payment UI**: Integrated PayPal and Paymob checkout buttons

### **Technology & Libraries**

* **Angular 18**: Single-page application framework
* **TypeScript 5.3**: Type-safe development
* **RxJS 7.8**: Reactive programming for state management
* **Angular Material 17**: Material Design components
* **Bootstrap 5 & Tailwind CSS 3.4**: Responsive and utility-first styling
* **Reactive Forms**: Custom form validation for complex user inputs
* **HttpClient**: API communication with backend
* **ngx-toastr**: Toast notifications for user actions

### **Running the Frontend**

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Update API base URL in src/environments/environment.ts
# Run development server
ng serve

# Build for production
ng build --configuration production
```

### **Folder Structure**

```
frontend/
├── src/
│   ├── app/
│   │   ├── components/      # Reusable UI components
│   │   ├── pages/           # Feature pages (Dashboard, CV Builder, Interview, etc.)
│   │   ├── services/        # API services
│   │   ├── guards/          # Route guards
│   │   └── app.module.ts
│   ├── assets/              # Images, icons, styles
│   └── environments/        # Environment variables
```

This setup ensures a fully **responsive and modular frontend**, easily maintainable and extensible for future features such as audio interviews or mobile app integration.


##  API Documentation

### **Swagger Documentation**
Access interactive API docs at: `https://localhost:7001/swagger`

### **Key Endpoints**

#### **Authentication**
```http
POST /api/auth/register       # User registration
POST /api/auth/login          # User login
POST /api/auth/refresh-token  # Token refresh
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

#### **Career Recommendation**
```http
GET  /api/quiz/questions      # Get quiz questions
POST /api/quiz/submit         # Submit quiz answers
GET  /api/recommendation/{id} # Get recommendation
```

#### **CV Builder**
```http
POST /api/cv/generate         # Generate CV
GET  /api/cv/download/{id}    # Download PDF
GET  /api/cv/user/{userId}    # Get user CVs
```

#### **Interview**
```http
POST /api/interview/start     # Start interview session
POST /api/interview/answer    # Submit answer
POST /api/interview/finish    # Complete interview
GET  /api/interview/report/{id}
```

#### **Job Parser**
```http
POST /api/job/parse           # Parse job description
GET  /api/job/{id}            # Get parsed job
```

#### **Payment**
```http
GET  /api/payment/pricing     # Get pricing plans
POST /api/payment/create-session
POST /api/payment/verify
GET  /api/payment/history/{userId}
```

---

##  Screenshots

### Dashboard
![Dashboard](https://via.placeholder.com/800x450?text=User+Dashboard)

### Career Recommendation
![Recommendation](https://via.placeholder.com/800x450?text=Career+Recommendation+Results)

### CV Builder
![CV Builder](https://via.placeholder.com/800x450?text=CV+Builder+Interface)

### Interview Simulator
![Interview](https://via.placeholder.com/800x450?text=Interview+Simulator)

---

## 🚀 Features in Production

### **Implemented**
✅ User authentication and authorization  
✅ Career path quiz with AI analysis  
✅ CV generation with PDF export  
✅ Text-based interview simulator  
✅ Job description parsing  
✅ Payment integration (PayPal, Paymob)  
✅ User profile management  
✅ Premium account system  
✅ Responsive UI design  

### **Future Enhancements**
- Audio interview support
- Advanced job matching algorithm
- Skills gap detailed analysis
- Multiple CV templates
- Admin analytics dashboard
- Discount code system
- Mobile application

---

##  Developer

**Noura Ahmed**
- Full-Stack Software Engineer
- Email: noura.ahmed7258@gmail.com
- LinkedIn: [linkedin.com/in/noura-ahmed-36779b304](https://www.linkedin.com/in/noura-ahmed-36779b304)
- GitHub: [github.com/noura-ahmed](https://github.com)

---

##  Acknowledgments

- Pre-trained AI models for natural language processing
- PayPal and Paymob for payment infrastructure
- Microsoft for .NET and development tools
- Angular team for the frontend framework

---

##  License

This project is licensed under the MIT License.

---

<div align="center">

**⭐ If you find this project useful, please consider giving it a star! ⭐**

Made with ❤️ by Noura Ahmed

</div>
