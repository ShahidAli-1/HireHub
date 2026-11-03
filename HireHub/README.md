# 🚀 HireHub - Modern Job Marketplace & ATS Checker

HireHub is a complete, full-stack 3-role Job Marketplace built with **ASP.NET Core MVC 6**. It connects Job Seekers with Recruiters through a Freemium SaaS model, featuring daily usage limits, manual payment verification, automated background jobs, and an AI-inspired ATS Resume Checker.

## ✨ Key Features

*   **For Job Seekers:** Create detailed profiles, upload resumes, apply to jobs (10/day on Free plan), track application statuses, and scan resumes with the ATS Checker.
*   **For Recruiters:** Manage company profiles, post jobs (2/day on Free plan), review applicants via a drag-and-drop Kanban Board, schedule interviews, and export applicant lists to CSV.
*   **For Admins:** Complete control panel to manage users, verify recruiters, moderate jobs, approve manual payments, and view real-time revenue analytics.
*   **ATS Resume Checker:** A public tool that analyzes PDF/DOCX files and scores them based on ATS best practices (contact info, length, action verbs, and essential sections).
*   **Automation & Security:** Hangfire background jobs for auto-expiring subscriptions/jobs, email notifications via MailKit, and BCrypt password hashing.

## 🛠️ Tech Stack

*   **Backend:** C#, ASP.NET Core 6 MVC, Entity Framework Core
*   **Database:** SQL Server
*   **Frontend:** HTML5, CSS3, Bootstrap 5, Vanilla JavaScript, Chart.js (Analytics), Quill.js (Rich Text), SortableJS (Kanban)
*   **Libraries:** Hangfire (Background Jobs), MailKit (Emails), PdfPig (PDF Parsing), BCrypt.Net (Security), Toastify (Notifications)

## 📸 Screenshots

### 1. Modern Landing Page
*Featuring a dynamic mesh background, glassmorphism search, and a 3D floating job card.*

<img width="1920" height="4468" alt="Home page of HireHub" src="https://github.com/user-attachments/assets/f0448517-ff06-4b6e-b6eb-bb6b7ee78f7c" />
### 2. Job Seeker Dashboard
*Tracking applications, interviews, and profile completeness.*

<img width="1920" height="1102" alt="Job Seeker Dashboard" src="https://github.com/user-attachments/assets/c7da7006-0439-44cd-8afc-16c4a0dc2432" />
### 3. Job Seeker Applications
*Track Applications.*

<img width="1920" height="1068" alt="My Application" src="https://github.com/user-attachments/assets/af7dc893-93c5-446c-9367-17a2021466b0" />
### 4. ATS Resume Checker
*Instant scoring with actionable feedback for job seekers.*

<img width="1920" height="1591" alt="Ats resume score" src="https://github.com/user-attachments/assets/2da42a06-c542-4a49-925c-f4e796dc61c0" />
### 4. Subscription & Upgrade Plan
*Freemium SaaS model with side-by-side plan comparison and manual payment submission.*

<img width="1920" height="1068" alt="Upgrade Plan" src="https://github.com/user-attachments/assets/81ddb78b-0615-4655-bb84-f874d78e77c1" />

### 5. Recruiter Dashboard
*Overview of job performance, recent applicants, and quick actions.*

<img width="1920" height="1092" alt="RecruiterDashboard" src="https://github.com/user-attachments/assets/a530a105-8e99-4679-b493-ef7b9a502a15" />
### 6. Company Profile (Recruiter)
*Managing company details, uploading logos, and setting industry information.*

<img width="1920" height="1026" alt="Company Profile" src="https://github.com/user-attachments/assets/9571c15e-e030-4159-84ee-7b6a647fddbb" />
### 7. Post a Job (Recruiter)
*Creating detailed job listings with a rich text editor (Quill.js).*

<img width="1920" height="1854" alt="Post Job" src="https://github.com/user-attachments/assets/f73aac60-a8be-4a39-acfb-08318a586ba8" />
### 8. Applicants List (Recruiter)
*Filtering candidates by status (New, Shortlisted, Interview, Hired, Rejected).*

<img width="1920" height="997" alt="Applicants" src="https://github.com/user-attachments/assets/3b061a22-01d7-49f2-b980-fa73dcb73dcd" />
### 9. Admin Revenue Reports
*Real-time charts (Chart.js) for MRR and active subscriptions.*

<img width="1920" height="1966" alt="Revenue Report" src="https://github.com/user-attachments/assets/ab147dcf-5768-4da5-9145-34d056d8134a" />
## 🚀 How to Run Locally

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/ShahidAli-1/HireHub.git
    
2.  **Setup the Database & Secrets:**
Open appsettings.example.json and rename it to appsettings.json.

Update the DefaultConnection string with your SQL Server details.

Update the Email section with your Gmail address and App Password.
    
3.  **Apply Migrations:**
Open the Package Manager Console in Visual Studio and run:
Update-Database

4.  **Run the App:**
Press F5 in Visual Studio. The database will automatically seed with an Admin account:

Admin Email: admin@hirehub.pk

Admin Password: Admin@123
