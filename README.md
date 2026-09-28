# 📝 TaskTracker API

Une API REST robuste et sécurisée de gestion de tâches, conçue en **.NET 10** et basée sur la **Clean Architecture** et le patron **CQRS** via MediatR.

---

## 🚀 Fonctionnalités Principales

* **Authentification & Sécurité :**
  * Génération de jetons **JWT** (Access Token éphémère de 15 minutes).
  * **Refresh Token avec rotation automatique** et révocation en base de données SQL Server.
  * Stockage ultra-sécurisé du Refresh Token dans un **Cookie HttpOnly / SameSite=Strict** (Inviolable contre les attaques XSS).
  * Endpoint de **Déconnexion (`/logout`)** avec nettoyage immédiat des cookies et invalidation du token.

* **Contrôle d'Accès basé sur les Rôles (RBAC) :**
  * Support des rôles **Admin** et **User**.
  * Sécurisation des routes d'administration (`[Authorize(Roles = "Admin")]`).
  * Attribution des tâches par les administrateurs à des membres spécifiques ou à eux-mêmes.

* **Gestion des Tâches (CRUD) :**
  * Consultation des tâches personnelles.
  * Création et suppression sécurisées par un Administrateur.

* **Architecture & Gestion des Erreurs :**
  * Separation stricte des responsabilités (Domain, Application, Infrastructure, API).
  * **Global Exception Handler** centralisé renvoyant des réponses standardisées au format **`ProblemDetails`** (RFC 7807).
  * Documentation interactive **Swagger / OpenAPI v3** avec support de l'en-tête `Bearer`.

---

## 🛠️ Stack Technique

* **Framework :** .NET 10 / C#
* **Architecture :** Clean Architecture + CQRS
* **Bibliothèques principales :**
  * `MediatR` (Découplage Commandes/Requêtes)
  * `Entity Framework Core` (ORM & SQL Server)
  * `Microsoft.AspNetCore.Authentication.JwtBearer`
  * `Swashbuckle` / OpenAPI v3

---

## 📐 Structure du Projet

```text
TaskTracker/
├── TaskTracker.Domain/          # Entités, Interfaces & Règles métier pures
├── TaskTracker.Application/     # CQRS (Commands, Queries, DTOs, Validation)
├── TaskTracker.Infrastructure/  # EF Core, BDD, JWT Provider, Repositories
└── TaskTracker.API/             # Controllers, Middlewares, Configuration OpenAPI# TaskTracker
