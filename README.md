# 🛒 Grocery Management System

A full-stack, 3-tier Role-Based Access Control (RBAC) web application built with **ASP.NET Core MVC**, **Entity Framework Core**, and **SQLite**. Designed to handle public storefront browsing, secure customer checkout, cash-register POS transactions for store associates, and administrative inventory and staff management.

---

## ✨ Key Features & Architecture

* **Role-Based Access Control (RBAC):** Distinct portals and permission boundaries for **Admins**, **Store Associates**, and **Customers** with smart post-login redirection.
* **Public Storefront & Protected Cart:** Visitors can browse the catalog freely without an account; signing in or adding items to the cart initiates secure checkout and real-time inventory deduction.
* **Associate POS Terminal:** Cashiers can log transactions, process customer purchases, and manage sales flow efficiently.
* **Admin Dashboard & Management:** Centralized metrics tracking inventory catalog size, low-stock alerts, employee management, and purchase audit histories.
* **Predefined Relational Categories & CSV Seeding:** 13 structured, alphabetically ordered categories mapped via foreign key constraints (`CategoryId`) paired with an extensive emoji-tagged product catalog.

---

## 🛠️ Tech Stack

* **Backend:** C#, ASP.NET Core MVC, LINQ, ASP.NET Core Identity
* **Database & ORM:** SQLite, Entity Framework Core (EF Core), Code-First Migrations
* **Frontend:** Bootstrap 5, HTML5, CSS3, Razor Views
* **Development Environment:** macOS, .NET SDK, Visual Studio Code / JetBrains Rider

---

## 📂 Project Structure

```text
Grocery Management/
├── Controllers/
│   ├── AccountController.cs        # Authentication, login/register & role redirection
│   ├── AdminController.cs          # Dashboard, stock overview, and employee management
│   ├── AssociateController.cs      # POS terminal and sales processing
│   ├── CustomerController.cs       # Public storefront and secure shopping cart/checkout
│   └── HomeController.cs           # Landing page and general navigation
├── Data/
│   ├── AppDbContext.cs             # EF Core database context
│   ├── DbInitializer.cs            # Database seeding logic
│   └── products_seed.csv           # Initial relational product dataset
├── Migrations/                     # EF Core migration snapshots
├── Models/
│   ├── ViewModels/                 # Presentation and form view models
│   ├── ApplicationUser.cs          # Extended user identity model
│   ├── Category.cs                 # Relational category model
│   ├── Product.cs                  # Product catalog model with emoji support
│   ├── Purchase.cs                 # Order tracking model
│   └── Purchaseitem.cs             # Individual line items per purchase
├── Views/
│   ├── Account/                    # Login, Register, AccessDenied views
│   ├── Admin/                      # Dashboard, stock, and employee views
│   ├── Associate/                  # POS sales terminal views
│   ├── Customer/                   # Storefront catalog, cart, and confirmation views
│   ├── Home/                       # Home index and privacy views
│   └── Shared/                     # Layout, navigation bar, and error views
├── appsettings.json                # Connection strings and configuration
└── Program.cs                      # Service registration, routing, and middleware

##🚀 Getting Started Locally
Prerequisites
.NET 8.0 SDK or later installed on your machine.
IDE like Visual Studio Code, Visual Studio, or JetBrains Rider. 
```
## Installation & Setup

1. Clone the repository:
```text git clone https://github.com/estinec/Grocery_Management.git
cd Grocery_Management
```

3. Restore NuGet dependencies:
```text
 dotnet restore
```

5. Update the Database:
 ```text
 dotnet ef database update
```

7. Run The Application
```text
 dotnet run
```

   
