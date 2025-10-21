# 💞 DatingApp

A modern, full-stack dating application built with **.NET**, **Angular**, and **PostgreSQL**.
Users can create profiles, upload photos, browse potential matches, and connect through mutual interest — all secured by **JWT authentication** and real-time communication via **SignalR**.


## 🧠 Overview

**DatingApp** is a full-stack web application that demonstrates how to build a scalable, secure, and interactive dating platform using modern web technologies.
It includes user management, photo uploads, real-time communication, and secure matching logic.

## ❤️ Features

- **User Registration & Login** — Secure authentication using JWT tokens
- **Profile Creation & Editing** — Add personal info, preferences, and photos
- **Photo Upload & Storage** — Store and manage user profile images
- **Browse / Swipe Matches** — Discover potential partners
- **Like / Pass System** — Express interest or skip users
- **Mutual Match Detection** — Matches are only created when both users “like” each other
- **SignalR Communication** — Real-time updates and interactions
- **Security & Validation** — Input validation, protected routes, and role-based access


## 🏗️ Tech Stack & Architecture

Layer |	Technology
------|-----------
Backend	| .NET (C#), ASP.NET Core
Frontend | Angular, TypeScript, RxJS
Database | PostgreSQL
Authentication | JWT (JSON Web Tokens)
Real-Time Communication | SignalR
Dependency Injection | Built-in DI in .NET
Testing | xUnit

Architecture Highlights

- Follows a clean, service-oriented architecture

- Uses Entity Framework Core for ORM

- SignalR handles real-time communication between users

- Angular client consumes REST APIs for standard operations and SignalR for live updates

## ⚙️ Getting Started

Make sure you have installed:

- .NET SDK 8+
- Node.js & npm
- Angular CLI
- PostgreSQL

**Installation**
Clone the repository:
``` bash
git clone https://github.com/Gekonisko/DatingApp.git
cd DatingApp
```

**Backend Setup**
``` bash
cd API
dotnet restore
```

Create your database and update the connection string in `appsettings.Development.json`:
``` bash
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=datingapp;Username=postgres;Password=yourpassword"
}
```

Then run migrations and start the API:
``` bash
dotnet ef database update
dotnet run
```

**Frontend Setup**
``` bash
cd ../client
npm install
ng serve
```

## 🔧 Configuration

Environment variables and app settings are defined in:

- API/appsettings.json

- client/src/environments/environment.ts

Key settings include:

- ConnectionStrings:DefaultConnection

- TokenKey (JWT secret)

- AllowedHosts

- CloudStorage (if using cloud photo storage)

## 🗃️ Database & Migrations

Using Entity Framework Core:

``` bash
# Add a new migration
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```