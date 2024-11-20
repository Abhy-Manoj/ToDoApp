# TodoApp

## Overview
**TodoApp** is a web-based application to manage projects and their associated todos. It provides features to:
- Create, update, and delete projects.
- Manage todos within a project, including adding, editing, and toggling their status.
- Export project summaries to GitHub as secret gists in Markdown format.
- Authenticate users with a secure login and registration system.

---

## Features
1. **Authentication**: User signup, login, and logout functionality with session management.
2. **Project Management**: Create, view, edit, and delete projects.
3. **Todo Management**: Manage todos with status toggling between `Pending` and `Completed`.
4. **Export to Gist**: Export project summaries to GitHub as secret gists and save locally as Markdown files.
5. **Responsive UI**: Built with Bootstrap for an intuitive and responsive user experience.

---

## Prerequisites
- **.NET 6 SDK** or higher
- **SQL Server** with SQL Server Management Studio (SSMS)
- **GitHub Account** (to generate a Personal Access Token for Gist API)
- **Git** (optional for version control)

---

## Setup Instructions

### 1. Clone the Repository
Clone this repository to your local machine:

git clone https://github.com/your-username/TodoApp.git
Navigate to the project folder:

cd TodoApp

### 2. Configure the Database
Open SQL Server Management Studio (SSMS).
Execute the SQL scripts provided in the DatabaseSetup.sql file to:
Create the database (My_DB).
Create tables (Users, Projects, Todos).
Define all stored procedures for CRUD operations.

### 3. Update the Configuration
Open appsettings.json.

Update the ConnectionStrings section with your SQL Server details:

json
Copy code
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=My_DB;Integrated Security=True;TrustServerCertificate=True;"
}
Add your GitHub Personal Access Token for the Gist API:

Generate a token in your GitHub account with the gist permission.
Replace the placeholder in TodoController.cs:

client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_PERSONAL_ACCESS_TOKEN");

### 4. Build and Run the Application
Build the application:
dotnet build
Run the application:
dotnet run

Open your browser and navigate to the URL provided (e.g., http://localhost:5000).

## Test Instructions

Testing
Authentication:

Register a new user and verify that login works.
Test invalid credentials for error handling.
Project Management:

Add, edit, and delete projects. Verify changes in the database.
Try adding projects with invalid or missing titles to test validations.
Todo Management:

Add, edit, and delete todos for a project.
Toggle the status between Pending and Completed.
Export to Gist:

Export project summaries and confirm the Gist is created on GitHub.
Verify that the Markdown file is saved locally.


Folder Structure

TodoApp/
├── Controllers/
│   ├── AuthController.cs
│   ├── ProjectController.cs
│   ├── TodoController.cs
├── Models/
│   ├── Project.cs
│   ├── Todo.cs
│   ├── User.cs
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   ├── Signup.cshtml
│   │   ├── LandingPage.cshtml
│   ├── Project/
│   │   ├── Index.cshtml
│   ├── Todo/
│   │   ├── Details.cshtml
│   ├── Shared/
│       ├── _Layout.cshtml
│       ├── _ValidationScriptsPartial.cshtml
├── appsettings.json
├── Program.cs
├── README.md