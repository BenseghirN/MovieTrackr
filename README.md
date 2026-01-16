# 🎬 MovieTrackR

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoftazure)](https://azure.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> Plateforme collaborative moderne de suivi et de critique de films, développée comme projet de fin d'études (TFE) pour le Bachelier en Informatique de Gestion.

MovieTrackR est une application web full-stack qui permet aux cinéphiles de découvrir, organiser et partager leurs critiques de films. Le projet met en œuvre une architecture moderne et professionnelle avec Clean Architecture, CQRS, et intégration d'intelligence artificielle multi-agents.

---

## ✨ Fonctionnalités principales

### Pour les utilisateurs
- 🔍 **Recherche avancée** de films avec intégration TMDB
- 📝 **Système de critiques** avec notes, commentaires et likes
- 👤 **Profils personnalisés** avec gestion de collections
- 💬 **Interactions communautaires** (commentaires, discussions)
- 🤖 **Assistant IA intelligent** pour recommandations et découverte

### Pour les administrateurs
- 🛡️ **Modération** des critiques et commentaires
- 👥 **Gestion des utilisateurs** et des rôles
- 📊 **Validation des propositions** de nouveaux films
- 🔍 **Contrôle du catalogue** et des métadonnées

---

## 🏗️ Architecture

MovieTrackR suit une architecture **Clean Architecture** moderne avec séparation stricte des responsabilités :

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend (Angular 20)                │
│  Components • Services • Guards • Signals • Lazy Loading    │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/REST
┌────────────────────────┴────────────────────────────────────┐
│                    Backend API (.NET 9)                     │
│                     Minimal API Endpoints                   │
├─────────────────────────────────────────────────────────────┤
│                  Application Layer (CQRS)                   │
│          MediatR • Commands • Queries • Handlers            │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│              Entities • Business Rules • Interfaces         │
├─────────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                       │
│     EF Core • PostgreSQL • TMDB API • Azure Services        │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│               AI Layer (Semantic Kernel)                    │
│  Intent Extractor • DiscoverMovies • PersonSeeker •         │
│  SimilarMovieSeeker • Redactor                              │
└─────────────────────────────────────────────────────────────┘
```

### Principes architecturaux
- **Clean Architecture** : séparation Domain/Application/Infrastructure
- **CQRS** avec MediatR pour séparer lectures et écritures
- **Code-First** avec Entity Framework Core 9
- **Repository Pattern** pour l'abstraction d'accès aux données
- **DTO Pattern** pour isoler les entités métier

---

## 🚀 Stack technique

### Backend
- **.NET 9** - Framework applicatif
- **Entity Framework Core 9** - ORM avec approche Code-First
- **MediatR** - Implémentation CQRS
- **AutoMapper** - Mapping entités/DTOs
- **PostgreSQL 16** - Base de données relationnelle
- **Semantic Kernel** - Orchestration multi-agents IA
- **Azure OpenAI** - Service GPT-4o-mini

### Frontend
- **Angular 18** - Framework SPA moderne
- **TypeScript 5** - Typage statique
- **PrimeNG** - Bibliothèque de composants UI
- **Signals** - Gestion réactive de l'état
- **RxJS** - Programmation réactive

### Infrastructure & DevOps
- **Azure App Service** - Hébergement unifié
- **Azure PostgreSQL Flexible Server** - Base de données managée
- **Azure Blob Storage** - Stockage des images
- **Azure OpenAI Service** - Intelligence artificielle
- **Azure Application Insights** - Monitoring et télémétrie
- **Microsoft Entra ID** - Authentification et gestion des identités
- **GitHub Actions** - CI/CD automatisé
- **Docker** - Conteneurisation pour développement local

### APIs externes
- **TMDB API** - Données cinématographiques complètes
- **Azure OpenAI API** - Modèle GPT-4o-mini

---

## 🤖 Système d'Intelligence Artificielle

MovieTrackR intègre une architecture **multi-agents** inspirée des systèmes d'orchestration modernes, implémentée avec **Semantic Kernel**.

### Architecture des agents

```
User Query → Intent Extractor → Dispatcher → Specialized Agent → Response
                                      │
                                      ├─> DiscoverMovies Agent
                                      ├─> PersonSeeker Agent
                                      ├─> SimilarMovieSeeker Agent
                                      └─> Redactor Agent
```

Chaque agent spécialisé :
- Possède un **rôle clairement défini** et un prompt optimisé
- Accède à des **kernel functions spécifiques** (recherche locale, API TMDB, génération de texte)
- Utilise le **contexte conversationnel** pour des réponses pertinentes
- S'intègre avec le backend via **CQRS handlers**

---

## 🧪 Tests

Le projet implémente une stratégie de tests ciblée sur les fonctionnalités critiques :

```bash
# Tests unitaires
dotnet test tests/MovieTrackR.Application.Tests

# Tests d'intégration
dotnet test tests/MovieTrackR.Integration.Tests

# Tous les tests
dotnet test
```

### Couverture
- Tests unitaires sur les **handlers CQRS**
- Tests d'intégration sur les **endpoints API**
- Validation fonctionnelle des **parcours utilisateurs**
- Tests obligatoires dans le **pipeline CI/CD**

---

## 🚢 Déploiement

### Architecture cloud Azure

L'application est déployée sur **Azure** avec une stratégie unifiée :
- **App Service** : Hébergement backend + frontend (wwwroot)
- **PostgreSQL Flexible Server** : Base de données managée
- **Blob Storage** : Stockage des images/posters
- **Application Insights** : Monitoring et logs
- **Managed Identity** : Authentification entre services Azure

### Pipeline CI/CD

GitHub Actions automatise :
1. **Build** : Compilation backend + frontend
2. **Tests** : Exécution de la suite de tests
3. **Quality Gates** : Validation obligatoire
4. **Deploy** : Déploiement sur Azure App Service
5. **Health Check** : Vérification post-déploiement

### Localisation
- **Région principale** : Sweden Central
- **Subscription** : Azure for Students

---

## 📊 Modèle de données

Base de données **PostgreSQL** avec modélisation **MERISE** complète :

### Entités principales
- **Users** : Utilisateurs et profils
- **Movies** : Catalogue de films
- **Reviews** : Critiques utilisateurs
- **Review_Comments** : Commentaires sur critiques
- **Review_Likes** : Système de valorisation
- **User_Lists** : Collections personnalisées
- **Genres**, **People**, **Movie_Cast**, **Movie_Crew** : Métadonnées

### Contraintes d'intégrité
- Clés primaires et étrangères strictes
- Contraintes d'unicité (email, slug)
- Cascades contrôlées
- Index optimisés pour les performances

---

## 📚 Documentation détaillée

Pour des informations complètes sur chaque composant :

- [📖 Backend README](./MovieTrackR-back/README.md) - Architecture API, CQRS, endpoints
- [📖 Frontend README](./MovieTrackR-front/README.md) - Composants Angular, services, routing

---

## 🎯 Perspectives d'évolution

### Court terme
- ✅ Amélioration de la couverture de tests
- ✅ Optimisation des performances EF Core
- ✅ Extension du système multi-agents IA
- ✅ Certaines features mineures

### Moyen terme
- 🔄 Séparation frontend/backend pour scalabilité
- 🔄 Mise en cache intelligente (Redis)
- 🔄 Tests de charge et monitoring avancé
- 🔄 Support multi-langues complet

### Long terme
- 🔮 Recommandations IA personnalisées
- 🔮 Notifications temps réel (SignalR)
- 🔮 Application mobile native
- 🔮 Export de données et API publique

---

## 🙏 Remerciements

- **IRAM Mons** pour l'encadrement académique
- **TMDB** pour l'API de données cinématographiques
- **Microsoft** pour Azure for Students et les outils .NET
- **Communauté open-source** pour les bibliothèques et frameworks utilisés

---

## 📞 Support

Pour toute question ou suggestion :
- 🐛 [Issues GitHub](https://github.com/BenseghirN/MovieTrackr/issues)
- 📧 Email : norisbenseghir@gmail.com

---

<div align="center">
  <sub>Développé avec ❤️ pour les cinéphiles par Noris BENSEGHIR</sub>
</div>
