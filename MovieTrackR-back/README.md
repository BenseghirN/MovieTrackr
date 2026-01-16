# 🔧 MovieTrackR - Backend API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Azure](https://img.shields.io/badge/Azure-Cloud-0078D4?logo=microsoftazure)](https://azure.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)](https://docs.microsoft.com/ef/)
[![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-1.68-00A4EF?logo=microsoft)](https://learn.microsoft.com/semantic-kernel/)
[![MediatR](https://img.shields.io/badge/MediatR-14-68217A)](https://github.com/jbogard/MediatR)

> API RESTful moderne construite avec .NET 9, implémentant Clean Architecture, CQRS, et orchestration multi-agents IA.

---

## 🏗️ Architecture

Le backend suit les principes de **Clean Architecture** avec une séparation stricte en couches concentriques :

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer                               │
│  • Minimal API Endpoints                                        │
│  • Middleware (Auth, CORS, Error Handling)                      │
│  • Backend For Frontend (BFF)                                   │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Dependency Injection
┌─────────────────────┴───────────────────────────────────────────┐
│                    Application Layer                            │
│  • CQRS Commands & Queries                                      │
│  • MediatR Handlers                                             │
│  • DTOs & Mapping Profiles                                      │
│  • Validation Rules                                             │
│  • Application Interfaces                                       │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Business Logic Orchestration
┌─────────────────────┴───────────────────────────────────────────┐
│                       Domain Layer                              │
│  • Entities (Movie, Review, User, etc.)                         │
│  • Domain Events                                                │
│  • Business Rules                                               │
│  • Domain Interfaces                                            │
│  • Value Objects                                                │
└─────────────────────┬───────────────────────────────────────────┘
                      │ Infrastructure Abstraction
┌─────────────────────┴───────────────────────────────────────────┐
│                  Infrastructure Layer                           │
│  • EF Core DbContext & Repositories                             │
│  • External API Clients (TMDB)                                  │
│  • Azure Services Integration                                   │
│  • Persistence & Caching                                        │
└─────────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────────┐
│                      AI Layer (Optional)                        │
│  • Semantic Kernel Configuration                                │
│  • Multi-Agent Orchestration                                    │
│  • Intent Extraction & Routing                                  │
│  • Specialized Agents (DiscoverMovies, PersonSeeker, etc.)      │
└─────────────────────────────────────────────────────────────────┘
```

### Flux de traitement d'une requête

```
HTTP Request
    ↓
Minimal API Endpoint
    ↓
[Authentication Middleware]
    ↓
[Authorization Check]
    ↓
MediatR Command/Query
    ↓
Handler (Application Layer)
    ↓
Domain Logic + Repository
    ↓
EF Core → PostgreSQL
    ↓
AutoMapper → DTO
    ↓
HTTP Response (JSON)
```

---

## 📁 Structure du projet

```
📂 MovieTrackR/
├── 📂 MovieTrackR.API/                               # Point d'entrée API
│   ├── 📂 Endpoints/                                 # Minimal API endpoints
│   │   ├── 📂 Movies/
│   │   ├── 📂 Reviews/
│   │   ├── 📂 Users/
│   │   ├── 📂 UserLists/
│   │   └── 📂 .../
│   ├── 📂 Middleware/                                # Middleware personnalisés
│   ├── 📂 Filters/                                   # Filtres d'action
│   ├── 📂 Configuration/                             # Extensions de configuration
│   ├── 📂 wwwroot/                                   # Frontend compilé (Angular)
│   ├── 📜 Program.cs                                 # Configuration de l'application
│   └── 📜 appsettings.json                           # Configuration de base
│
├── 📂 MovieTrackR.Application/                       # Logique applicative    
│   ├── 📂 Configuration/                             # Extensions de configuration
│   ├── 📂 Movies/                       
│   │   ├── 📂 Commands/                              # CQRS Commands
│   │   │   ├── 📜 CreateMovieCommand.cs
│   │   │   ├── 📜 DeleteMovieCommand.cs
│   │   │   └── 📜 ...
│   │   ├── 📂 Queries/                               # CQRS Queries
│   │   │   ├── 📜 DiscoverMoviesQuery.cs
│   │   │   ├── 📜 GetMovieByIdQuery.cs
│   │   │   └── 📜 ...
│   ├── 📂 People/                       
│   │   ├── 📂 Commands/                              # CQRS Commands
│   │   │   ├── 📜 CreatePersonCommand.cs
│   │   │   ├── 📜 DeletePersonCommand.cs
│   │   │   └── 📜 ...
│   │   ├── 📂 Queries/                               # CQRS Queries
│   │   │   ├── 📜 GetAllPeopleQuery.cs
│   │   │   ├── 📜 SearchPeopleQuery.cs
│   │   │   └── 📜 ...
│   ├── 📂 Other Entities/    
│   ├── 📂 DTOs/                                      # Data Transfer Objects
│   │   ├── 📜 MovieDto.cs
│   │   ├── 📜 ReviewDto.cs
│   │   ├── 📜 UserDto.cs
│   │   └── 📜 ...
│   ├── 📂 Mappings/                                  # AutoMapper Profiles
│   │   └── 📜 MappingProfile.cs
│   ├── 📂 Interfaces/                                # Application Interfaces
│   │   ├── 📜 IGenreSeeder.cs
│   │   ├── 📜 IMovieTrackRDbContext.cs
│   │   └── 📜 IReviewContentSanitizer.cs
│
├── 📂 MovieTrackR.Domain/                            # Logique métier
│   ├── 📂 Configuration/                             # Extensions de configuration
│   ├── 📂 Entities/                                  # Entités métier
│   │   ├── 📜 Movie.cs
│   │   ├── 📜 Review.cs
│   │   ├── 📜 User.cs
│   │   ├── 📜 Genre.cs
│   │   ├── 📜 Person.cs
│   │   └── 📜 ...
│   ├── 📂 Helpers/                                   # Domain Helpers
│   └── 📂 Enums/                                     # Énumérations
│
├── 📂 MovieTrackR.Infrastructure/                    # Implémentations techniques
│   ├── 📂 Configuration/                             # Extensions de configuration
│   ├── 📂 Persistence/                               # EF Core
│   │   └── MovieTrackRDbContext.cs
│   ├── 📂 Migrations/                                # EF Core Migrations
│   ├── 📂 TMDB/    
│   │   ├── 📂 Services                               # Services externes
│   │   └── 📜 TmdbOptions.cs                         # Options de configuration TMDB

└── 📂 MovieTrackR.AI/                                # Intelligence artificielle
    ├── 📂 Configuration/                             # Extensions de configuration
    ├── 📂 Agents/                                    # Agents spécialisés
    │   ├── 📂 DiscoverMoviesAgent
    │   │   ├── 📂 Plugins                            # Agent Plugins (Kernel Functions)
    │   │   ├── 📜 DiscoverMoviesAgent.cs             # Agent Implementation
    │   │   └── 📜 DiscoverMoviesAgentPropeties.cs    # Agent Properties (Name, Description, Prompt,...)
    │   ├── 📂 IntentExtractorAgent
    │   ├── 📂 PersonSeekerAgent
    │   └── 📂 ...
    ├── 📂 Builder/                                   # Main Kernel Builder
    │   └── SemanticKernelBuilder.cs
    ├── 📂 Interfaces/                                # Agent management interfaces
    └── 📂 Utils/                                     # AI Options (Azure)
```

---

## 🛠️ Technologies

### Framework & Runtime
- **.NET 9** - Framework moderne haute performance

### Accès aux données
- **Entity Framework Core 9** - ORM
- **Npgsql** - Provider PostgreSQL

### Patterns & Architecture
- **MediatR 14** - CQRS et Mediator pattern
- **AutoMapper 16** - Object mapping
- **FluentValidation 12** - Validation des commandes

### Intelligence Artificielle
- **Semantic Kernel 1.x** - Framework d'orchestration IA
- **Azure.AI.OpenAI** - Intégration Azure OpenAI

### Services Azure
- **Microsoft.Identity.Web** - Authentification Microsoft Entra ID
- **Azure.Storage.Blobs** - Stockage des images
- **Azure.Monitor.OpenTelemetry** - Application Insights

### Tests
- **xUnit** - Framework de tests
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions expressives

---

## 🎯 Patterns et principes

### Clean Architecture
- **Indépendance du framework** : Le domaine ne dépend pas de technologies externes
- **Testabilité** : Logique métier isolée et facilement testable
- **Indépendance de la base de données** : Possibilité de changer de SGBD
- **Indépendance de l'UI** : Le domaine ne connaît pas le frontend

### CQRS (Command Query Responsibility Segregation)
### DTO Pattern
---

## 🔌 Endpoints API

### Movies

```http
GET    /api/movies                     # Liste des films (pagination)
GET    /api/movies/{id}                # Détails d'un film
GET    /api/movies/search?q={query}   # Recherche de films
POST   /api/movies                     # Créer un film [Auth]
PUT    /api/movies/{id}                # Modifier un film [Auth]
DELETE /api/movies/{id}                # Supprimer un film [Admin]
```

### Reviews

```http
GET    /api/reviews                    # Liste des critiques
GET    /api/reviews/{id}               # Détails d'une critique
POST   /api/reviews                    # Créer une critique [Auth]
PUT    /api/reviews/{id}               # Modifier sa critique [Auth]
DELETE /api/reviews/{id}               # Supprimer sa critique [Auth]
POST   /api/reviews/{id}/like          # Liker une critique [Auth]
DELETE /api/reviews/{id}/like          # Retirer son like [Auth]
```

### Comments

```http
GET    /api/reviews/{id}/comments      # Commentaires d'une critique
POST   /api/reviews/{id}/comments      # Commenter [Auth]
PUT    /api/comments/{id}              # Modifier son commentaire [Auth]
DELETE /api/comments/{id}              # Supprimer son commentaire [Auth]
```

### Users

```http
GET    /api/users/me                   # Profil utilisateur [Auth]
PUT    /api/users/me                   # Modifier son profil [Auth]
GET    /api/users/{id}                 # Profil public
GET    /api/users/{id}/reviews         # Critiques d'un utilisateur
GET    /api/users/{id}/lists           # Listes d'un utilisateur
```

### User Lists

```http
GET    /api/lists                      # Listes publiques
GET    /api/lists/{id}                 # Détails d'une liste
POST   /api/lists                      # Créer une liste [Auth]
PUT    /api/lists/{id}                 # Modifier sa liste [Auth]
DELETE /api/lists/{id}                 # Supprimer sa liste [Auth]
POST   /api/lists/{id}/movies/{movieId} # Ajouter un film [Auth]
DELETE /api/lists/{id}/movies/{movieId} # Retirer un film [Auth]
```

### AI Assistant

```http
POST   /api/ai/chat                    # Conversation avec l'assistant IA [Auth]
GET    /api/ai/suggestions             # Suggestions personnalisées [Auth]
```

### Admin

```http
GET    /api/admin/users                # Liste des utilisateurs [Admin]
PUT    /api/admin/users/{id}/role      # Modifier le rôle [Admin]
GET    /api/admin/reviews/flagged      # Critiques signalées [Admin]
DELETE /api/admin/reviews/{id}         # Supprimer critique [Admin]
GET    /api/admin/statistics           # Statistiques globales [Admin]
```

### Exemple d'endpoint (Minimal API)

```csharp
app.MapGet("/api/movies/{id}", async (
    Guid id, 
    IMediator mediator,
    CancellationToken cancellationToken) =>
{
    var query = new GetMovieByIdQuery { Id = id };
    var movie = await mediator.Send(query, cancellationToken);
    
    return movie is not null 
        ? Results.Ok(movie) 
        : Results.NotFound();
})
.WithName("GetMovieById")
.WithTags("Movies")
.Produces<MovieDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);
```

---

## 🤖 Intelligence artificielle

### Architecture multi-agents

Le système IA utilise **Semantic Kernel** pour orchestrer plusieurs agents spécialisés :

#### 1. Intent Extractor
Analyse le message utilisateur et détermine l'intention
#### 2. Agent Dispatcher
Route la requête vers l'agent approprié
#### 3. Agents spécialisés
**DiscoverMovies Agent** :
- Recherche de films selon critères (genre, année, acteurs)
- Intégration TMDB API et base locale
- Filtrage intelligent

**PersonSeeker Agent** :
- Informations sur acteurs/réalisateurs
- Anecdotes et biographie

**SimilarMovieSeeker Agent** :
- Recommandations basées sur similarité
- Analyse de genres, casting, thématiques
- Score de pertinence

**Redactor Agent** :
- Aide à la rédaction de critiques
- Suggestions de structure
- Amélioration stylistique

### Kernel Functions

Les agents utilisent des fonctions pour interagir avec le backend

---

## 📚 Ressources

### Documentation officielle
- [.NET 9](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/)
- [MediatR](https://github.com/jbogard/MediatR)
- [Semantic Kernel](https://learn.microsoft.com/semantic-kernel/)

### Articles et références
- [Clean Architecture par Jason Taylor](https://jasontaylor.dev/clean-architecture-getting-started/)
- [CQRS Pattern](https://docs.microsoft.com/azure/architecture/patterns/cqrs)

---

<div align="center">
  <sub>Built with ❤️ using .NET 9 and Clean Architecture principles</sub>
</div>
