# Contract Monthly Claim System (CMCS)

A web-based application developed as part of PROG6212 to streamline the submission, verification, approval, and reporting of monthly claims for Independent Contractor (IC) lecturers.

This system follows a structured workflow:
**Lecturer > Programme Coordinator > Academic Manager > HR**  
ensuring accurate processing, transparency, and administrative efficiency.

---

## Important Note for Marking

This GitHub repository contains **two branches**:  
- 'main'  
- 'master'  

- **All project work, commits, updates, and final POE submission are located in the 'master' branch.**  
Please switch to the **master branch** when reviewing the code.

Thank you.

---

## System Overview

The CMCS system allows Independent Contractor Lecturers to submit monthly claims, and ensures these claims move through the correct approval workflow until final reporting is completed by HR.

The system supports:
- Automated calculations  
- Role-based dashboards  
- Secure document uploads  
- Approval workflow tracking  
- HR reporting and invoice generation  

---

## User Roles & Their Permissions

### **1. Lecturer**
- Login using provided credentials  
- Submit monthly claims  
- Auto-calculation of total amount (Hours X Rate)  
- Upload supporting documents  
- View claim history and statuses

### **2. Programme Coordinator (PC)**
- View claims with status **"Pending Verification"**  
- Approve or reject claims  

### **3. Academic Manager (AM)**
- View claims verified by PC  
- Approve or reject claims  
- Moves claim to HR after approval

### **4. HR (Super User)**
- Add/edit/delete **all users** (including IC lecturers)  
- Assign departments & automatically apply hourly rates  
- View all approved claims for reporting purposes  
- Generate clean, formatted **PDF reports and invoices**  
- Cannot approve/reject claims - view-only for reporting  

---

## Login Credentials

### **Lecturer Login**
- Username: 'lect1'
- Password: '1234'

### **Programme Coordinator Login**
- Username: 'coord1'
- Password: 'coordinator1234'

### **Academic Manager Login**
- Username: 'manager1'
- Password: 'manager1234'

### **HR Login (Super User)**
- Username: 'hr1'
- Password: 'hr1234'

Credentials are also displayed on the **Login Page UI**, as recommended by the lecturer.

---

## Technology Stack

### **Frontend**
- Razor Pages (ASP.NET Core MVC)
- HTML, CSS, Bootstrap 
- JavaScript & jQuery (for auto-calculation)

### **Backend**
- ASP.NET Core MVC
- C#  
- Entity Framework Core  
- iTextSharp (PDF generation)

### **Database**
- SQL Server (SSMS)
- DB Script included in **/Documentation/DatabaseScript.sql**

### **Version Control**
- Git & GitHub  
- 10+ descriptive commits  

---

## Database Overview

Tables used in the system:

- 'Users'
- 'Lecturers'
- 'Departments'
- 'Claims'
- 'Modules'
- 'ModuleAssignments'

Relationships include:
- One-to-many between **Departments > Lecturers**
- One-to-many between **Lecturers > Claims**
- Many-to-many between **Lecturers > Modules**

---

## How to Run the Application (Step-by-Step)

Follow these steps:

### **1. Clone the repository**

git clone https://github.com/Makhubela-Lefa/CMCSApplication.git

### **2. Switch to the master branch**

git checkout master

### **3. Open the project**
- Open **Visual Studio 2022**
- Select **Open a project or solution**
- Choose the folder containing 'CMCSApplication.sln'

### **4. Restore dependencies**
Visual Studio will automatically restore NuGet packages.  
If not, go to:  
'Tools > NuGet Package Manager > Restore'

### **5. Database Setup**
1. Open SQL Server Management Studio (SSMS)
2. Create a new database:
   
   CREATE DATABASE CMCSDatabase;

3. Run the script located here:  

   /Documentation/CMCSDatabaseQuery2.sql

4. Ensure your connection string in 'appsettings.json' is correct.

### **6. Run the Application**
- Set the project as **StartUp Project**
- Press **F5** or click **Start**

### **7. Login Using the Provided Credentials**
Use the login details listed earlier in this README.

---

## Features Implemented

### **Lecturer Automation**
- Auto-calculation of total amount  
- Hourly rate pulled automatically  
- Read-only fields for rate & identity  
- File upload validation (.pdf, .docx, .xlsx)  
- 220-hour max validation  

### **Coordinator & Manager Workflow**
- Role-based access  
- Approve/reject functionality  
- Claims only visible at the correct stage  

### **HR Features**
- Full user management  
- Dynamic hourly rate inheritance from department  
- Edit lecturer data without breaking workflow  
- PDF invoice & claim reporting  
- View-only access to approved claims  

---

## PDF Generation

The system generates:
- **Full Claims Report (HR)**  
- **Lecturer Invoice PDF**  

Built using **iTextSharp** with a green corporate theme to match app branding.

---

## YouTube Demonstration Video
  
Video Link: ''

The video is **10-15 minutes**, demonstrating:
- Role logins  
- Full claim lifecycle  
- PDF generation  
- HR user & department management  
- Error handling  
- Validation  

---

## Project Structure

```
CMCSApplication/
- Controllers/
- Models/
- Views/
- wwwroot/
- Documentation/
    - CMCSDatabaseQuery2.sql
    - CMCS_Part3_Presentation.pptx
- README.md
- CMCSApplication.sln

---

## Version Control Summary

- More than **10 commits**  
- Each commit includes a **clear message**  
- Includes feature additions, bug fixes, UI improvements, and documentation updates

---

## Notes for Examiner Convenience

- Login credentials shown in UI  
- Branch to mark: **MASTER**  
- Major features demonstrated in video  
- README includes full running instructions  
- Database script included  
- No registration - HR creates all accounts  

---

