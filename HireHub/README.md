# HireHub 🚀

A complete, modern 3-role Job Marketplace platform built with **ASP.NET Core MVC 6**. 

HireHub connects Job Seekers with Recruiters and includes a powerful ATS Resume Checker. It uses a Freemium SaaS model with daily limits, subscriptions, and admin revenue tracking.

## ✨ Key Features

*   **Job Seekers:** Create a profile, upload a resume, apply to jobs (10/day on Free plan), track application statuses, and check resume ATS scores.
*   **Recruiters:** Company profiles, post jobs (2/day on Free plan), manage applicants with a Kanban board, schedule interviews, and export applicant CSVs.
*   **Admin Panel:** Manage users, verify recruiters, moderate jobs, approve manual payments, and view revenue charts (Chart.js).
*   **ATS Resume Checker:** Free tool to check if a resume passes Applicant Tracking Systems.
*   **Automation:** Hangfire background jobs for subscription expiry, daily limit resets, and email notifications.

## 🛠️ Tech Stack

*   **Backend:** C#, ASP.NET Core 6 MVC, Entity Framework Core
*   **Database:** SQL Server
*   **Frontend:** HTML5, CSS3, Bootstrap 5, Vanilla JavaScript, Chart.js, Quill.js (Rich Text)
*   **Tools & Libraries:** Hangfire (Background Jobs), MailKit (Email SMTP), QuestPDF, PdfPig (ATS text extraction)

## 🚀 How to Run Locally

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/YOUR-USERNAME/HireHub.git