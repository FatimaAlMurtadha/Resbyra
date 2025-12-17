----------------------------- Resbyra ----------------------------------

### Project Overview

Resbyra is a group-based school project developed as part of an agile software development assignment (MAI25, Malmö). The goal of the project is to plan, develop, and deliver a REST API as an agile team, following SCRUM principles.

The application represents a travel agency booking system with moderate complexity. 

This project combines the ideas of:

## Idea 1 – Package Constructor: Travelers are be able to create their own package tours that can include activities, hotels, and transportation between different destinations. The packages created are also reusable.

## Idea 2 – The Food Lover’s Odyssey: This idea focused on culinary experiences. Travelers can book packages that take them on a food journey through different cities or countries, with each destination selected for its local gastronomy.



---

### Project Purpose

The purpose of this project is to:

Practice agile teamwork and SCRUM roles.

Design and implement a RESTful API using Minimal API.

Work with databases, queries, and structured data models.

Apply requirement engineering techniques such as User Stories and BDD.

Collaborate and deliver code using Git and GitHub.



---

### Application Concept

Resbyra is a travel booking platform where travelers can:

Create custom travel packages.

Combine destinations, amenities, activities, and food-related experiences.

Reuse previously created travel packages "ready-made packages by admin".

The system also supports administrative functionality for managing the travel agency’s offerings.


---

### Functional Scope

## Traveler Features:

User authentication (account registeration & login).

Search for destinations, ready-made packages, activities...ect.

Create and manage bookings.

Build custom travel packages "custom-cards".

View search results as lists and detailed views.

## Administrator Features:

Browes users.

Manage destinations, packages, activities ...ect, but not the custom-cards plan "Only a registed user".

View bookings in both list and detailed form.

Modify or cancel bookings.


The system is designed to be extendable with features such as ratings, recommendations, and travel history.


---

### Technology Stack

Language: C#

Framework: .NET Minimal API

Database: MySQL

Architecture: REST API

API Testing: Postman / Thunder Client

Version Control: Git & GitHub



---

### Project Structure

Resbyra/
├── Auth/                 # Authentication and user-related logic
├── Endpoints/            # API endpoints (Minimal API)
├── Properties/           # Project and launch settings
├── Test/                 # Test-related code
├── .gitignore            # Application tracking
├── appsettings.Development.json
├── appsettings.json
├── Config.cs             # Application configuration
├── Program.cs            # Application entry point
└── README.md            


---

### Requierments to use this package:
In order to use this system you need the following: 
1. Visual Stadio Code "1.104.2".
2. .NET "10.0.100".
3. Git installed and configured.

---

### How to use:
If you are using Windows you need to:

1. Clone the repository:
- Open "git bash" or "WindowsPowerShell".
- Write this command : "git clone https:the link of the project on GitHub".

2. Go in the application by this command : cd Resbra

3. Run the application using this command : dotnet run 
4. Once the application is run: you will find all the instruction about what to do as you run the previous command
5. There will be a choice at the end of the program letting you to close it  

### Cntribution Guide

1. Fork the repository.

2. Clone the repository:  on Git bash or WindowsPoerShell and write "git clone https://github.com/FatimaAlMurtadha/Resbyra".

3. Create a new branch using this command: "git switch -c your-new-branch-name".

4. Make your Changes.

5. Commit and push using following command on orderd:
- "git add .".
- "git commit -m "Your commit message" ".
- git push origin your-new-branch-name.

### Branching Strategy

The project follows a feature-branch workflow:

main – stable main branch

feat/* – feature-specific development branches (e.g. countries, custom-cards,packages, destinations, activities, amenities...ect)

TestBranch – testing and experimentation


All new functionality is developed in feature branches and merged into main when completed.


---

### Methods & Documentation

The project work is supported by:

User Stories for requirements

BDD / Gherkin scenarios for describing system behavior

ER diagrams for database design

Wireframes to visualize user interactions

Kanban board for task tracking and planning



---

### Running the Project

1. Clone the repository


2. Configure the database connection in appsettings.json


3. Ensure MySQL is running


4. Run the application using Visual Studio or dotnet run


5. Test the API using Postman or Thunder Client



---

### License & Context

This project is developed for educational purposes only as part of a school assignment. It is not intended for production use.


---

### Team & Collaboration

The project is developed collaboratively by a student team using agile values, shared responsibility, and continuous communication via GitHub.
