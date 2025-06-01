# Layered Architecture Todo API

This project is a simple Todo API built to demonstrate a layered architectural pattern using Node.js, Express, and TypeScript.

## Architecture Overview

The application is structured into distinct layers, each with its own set of responsibilities. This separation of concerns aims to create a more maintainable, scalable, and testable codebase.

The primary layers are:

1.  **API Layer (Presentation)**
2.  **Application Layer (Service)**
3.  **Domain Layer**
4.  **Infrastructure Layer**

### 1. API Layer

*   **Location:** `dist/src/api/`
*   **Responsibilities:**
    *   Handles incoming HTTP requests and outgoing HTTP responses.
    *   Defines API routes and endpoints.
    *   Performs initial request validation (e.g., checking for required fields, basic data types).
    *   Delegates business logic processing to the Application Layer.
    *   Formats data for the client using DTOs.
*   **Key Components:**
    *   `controller/todoController.js`: Contains handler functions for each route. It receives requests, calls the appropriate application service methods, and sends back responses.
    *   `routes.js`: Defines the Express router, mapping HTTP methods and URL patterns to controller actions. It also manually sets up and injects dependencies for the controllers and services.

### 2. Application Layer

*   **Location:** `dist/src/application/`
*   **Responsibilities:**
    *   Orchestrates the application's use cases and business workflows.
    *   Acts as a mediator between the API Layer and the Domain/Infrastructure Layers.
    *   Uses Data Transfer Objects (DTOs) to receive data from and return data to the API Layer.
    *   May call upon Domain Logic for specific business rule validations.
    *   Interacts with Repositories (from the Infrastructure Layer) for data persistence.
*   **Key Components:**
    *   `service/todo.service.js`: Implements the core application logic for managing todos (e.g., creating, retrieving, updating, deleting).
    *   `dtos/`: Contains DTOs like `create-todo.dto.js` and `todo.dto.js` which define the data contracts for operations.

### 3. Domain Layer

*   **Location:** `dist/src/domain/`
*   **Responsibilities:**
    *   Contains the core business logic, rules, and entities of the application.
    *   This layer should be independent of application and infrastructure concerns.
    *   Defines the structure and behavior of domain objects (e.g., the `ITodo` interface conceptually represents the Todo entity).
*   **Key Components:**
    *   `todo.logic.js`: Contains specific business validation rules (e.g., the `DomainValidation` class with its `validateText` method).
    *   `interfaces/ITodo.js`: (Conceptually) Defines the contract for a Todo domain object.

### 4. Infrastructure Layer

*   **Location:** `dist/src/infrastructure/`
*   **Responsibilities:**
    *   Handles all external concerns, such as data persistence, external API calls, messaging, etc.
    *   Implements interfaces defined by the Application or Domain layers (e.g., repositories).
*   **Key Components:**
    *   `data-access/todo.repo.js`: Implements the `InMemoryTodoRepository`, providing methods to interact with the data store for todos. This abstracts the actual data storage mechanism.
    *   `database/inMemoryDB.js`: A simple in-memory `Map` used as the database for this MVP.

## Dependency Flow

Dependencies generally flow in one direction:
`API Layer` → `Application Layer` → `Domain Layer` / `Infrastructure Layer`

This means higher-level layers (like API) depend on lower-level layers (like Application), but not vice-versa. The Domain Layer should ideally have no dependencies on other layers.

## Testing

The project includes unit tests for some components, demonstrating the testability benefits of a layered architecture:
*   `dist/src/domain/todo.logic.test.js`
*   `dist/src/infrastructure/data-access/todo.repo.test.js`

Each layer can be tested in isolation by mocking its dependencies.