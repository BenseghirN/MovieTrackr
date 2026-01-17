# 🎨 MovieTrackR - Frontend Application

[![Angular](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![PrimeNG](https://img.shields.io/badge/PrimeNG-20-40B983)](https://primeng.org/)
[![Node.js](https://img.shields.io/badge/Node.js-22-339933?logo=nodedotjs)](https://nodejs.org/)

> Application web SPA moderne construite avec Angular 20, implémentant Signals, lazy loading, et un design system cinématique.

---

## 🏗️ Architecture

Le frontend suit une architecture **feature-based** modulaire avec séparation claire des responsabilités :

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                       │
│  • Components (Smart & Presentational)                          │
│  • Templates & Styling                                          │
│  • User Interactions                                            │
└────────────────────┬────────────────────────────────────────────┘
                     │ Data Binding (Signals)
┌────────────────────┴────────────────────────────────────────────┐
│                       Services Layer                            │
│  • HTTP Services (API Communication)                            │
│  • State Management Services                                    │
│  • Authentication Service                                       │
│  • Business Logic Services                                      │
└────────────────────┬────────────────────────────────────────────┘
                     │ HTTP Interceptors
┌────────────────────┴────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│  • HTTP Client                                                  │
│  • Router                                                       │
│  • Guards & Interceptors                                        │
│  • Environment Configuration                                    │
└─────────────────────────────────────────────────────────────────┘
```

### Flux de données

```
User Action
    ↓
Component (Signal/Event)
    ↓
Service Method
    ↓
HTTP Request → Backend API
    ↓
Observable Stream
    ↓
Signal Update
    ↓
Template Re-render
```

---

## 📁 Structure du projet

```
src/
├── 📂 app/
│   ├── 📂 core/                                            # Singletons & core functionality
│   │   ├── 📂 guards/
│   │   │   ├── 📜 admin.guard.ts
│   │   │   └── 📜 auth.guard.ts
│   │   ├── 📂 interceptors/
│   │   │   ├── 📜 auth.interceptor.ts
│   │   │   └── 📜 error.interceptor.ts
│   │   ├── 📂 services/
│   │   │   ├── 📜 api.service.ts
│   │   │   ├── 📜 auth.service.ts
│   │   │   └── 📜 ...
│   │   └── 📂 models/
│   │       ├── 📜 api-error.model.ts
│   │       ├── 📜 genre.model.ts
│   │       ├── 📜 notification.model.ts
│   │       └── 📜 paginated-result.model.ts
│   │
│   ├── 📂 features/                                        # Feature modules
│   │   ├── 📂 movies/
│   │   │   ├── 📂 components/
│   │   │   │   └── 📂 movie-card/
│   │   │   │       ├── 📜 movie-card.component.ts
│   │   │   │       ├── 📜 movie-card.component.html
│   │   │   │       └── 📜 movie-card.component.scss
│   │   │   ├── 📂 pages/
│   │   │   │   ├── 📂 movie-details/
│   │   │   │   └── 📂 movies/
│   │   │   ├── 📂 services/
│   │   │   │   └── 📜 movie.service.ts
│   │   │   └── 📂 models/
│   │   │       ├── 📜 movie-details.model.ts
│   │   │       ├── 📜 movie.model.ts
│   │   │       └── 📜 streaming-offers.model.ts
│   │   │
│   │   ├── 📂 reviews/
│   │   │   ├── 📂 components/
│   │   │   │   ├── 📂 comments-modal/
│   │   │   │   ├── 📂 movie-reviews/
│   │   │   │   ├── 📂 review-card/
│   │   │   │   └── 📂 review-form-modal/
│   │   │   ├── 📂 services/
│   │   │   │   ├── 📜 review.service.ts
│   │   │   │   └── 📜 ...
│   │   │   └── 📂 models/
│   │   │       └── 📜 review.model.ts
│   │   ├── 📂 ai/
│   │   │   ├── 📂 components/
│   │   │   │   └── 📂 ai-chat-widget/
│   │   │   ├── 📂 models/
│   │   │   │   └── 📜 chat-request.model.ts
│   │   │   ├── 📂 services/
│   │   │   │   └── 📜 ai.service.ts
│   │   │   └── 📂 store/
│   │   │       └── 📜 ai.service.ts
│   │   └── 📂 ...
│   │
│   ├── 📂 shared/                                          # Shared components & utilities
│   │   ├── 📂 components/
│   │   │   ├── 📂 layout/
│   │   │   │   ├── 📂 footer/
│   │   │   │   └── 📂 header/
│   │   │   └── 📂 toast/
│   │   ├── 📂 pages/
│   │   │   ├── 📂 forbidden/
│   │   │   └── 📂 not-found
│   │   └── 📂 pipes/
│   │       └── 📜 safe-url.pipe.ts
│   │
│   ├── 📜 app.config.ts                                    # Application configuration
│   ├── 📜 app.html                                         # Application html
│   ├── 📜 app.routes.ts                                    # Application routing
│   ├── 📜 app.scss                                         # Application style
│   └── 📜 app.ts                                           # Root component
│
├── 📂 assets/
│   └── 📂 images/
│
├── 📂 environments/
│   ├── 📜 environment.ts                                   # Development config
│   └── 📜 environment.prod.ts                              # Production config
│
├──📜 index.html                                            # 
├──📜 main.ts                                               # 
└──📜 styles.scss                                           # 
```
---

## 🛠️ Technologies

### Core Framework
- **Angular 20** - Framework SPA avec signals
- **TypeScript 5** - Superset typé de JavaScript
- **RxJS 7** - Programmation réactive

### UI Components & Styling
- **PrimeNG 20** - Bibliothèque de composants UI riche
- **PrimeIcons** - Set d'icônes intégré
- **SCSS** - Préprocesseur CSS avec variables et mixins
- **CSS Grid & Flexbox** - Layout moderne responsive

### State Management
- **Angular Signals** - Gestion d'état réactive native
- **RxJS BehaviorSubject** - State management patterns

### HTTP & Communication
- **HttpClient** - Client HTTP natif Angular
- **Interceptors** - Middleware HTTP pour auth/errors

### Routing & Navigation
- **Angular Router** - Routing avec lazy loading
- **Route Guards** - Protection des routes

### Forms & Validation
- **Reactive Forms** - Formulaires typés et réactifs
- **Template-driven Forms** - Formulaires simples
- **Custom Validators** - Validation métier

### Build & Tooling
- **Angular CLI** - Outillage de développement
- **TypeScript Compiler** - Compilation TypeScript
- **Webpack** - Bundling (via Angular CLI)
- **ESLint** - Linting TypeScript/JavaScript
- **Prettier** - Formatage de code

---

## 📚 Ressources

### Documentation officielle
- [Angular Documentation](https://angular.dev/)
- [Angular Signals](https://angular.dev/guide/signals)
- [PrimeNG Components](https://primeng.org/)
- [RxJS Documentation](https://rxjs.dev/)

### Guides et références
- [Angular Style Guide](https://angular.dev/style-guide)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [SCSS Documentation](https://sass-lang.com/documentation)

---

<div align="center">
  <sub>Built with ❤️ using Angular 20 and modern web standards</sub>
</div>
