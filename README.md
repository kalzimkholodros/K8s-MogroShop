# 🛍️ MogroShop Microservices

A cloud-native e-commerce platform leveraging Docker and Kubernetes orchestration, with Redis caching and RabbitMQ message broker for high-performance microservices architecture.

<div align="center">

[![.NET](https://img.shields.io/badge/Built%20with-.NET%208-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-Powered-326CE5?logo=kubernetes)](https://kubernetes.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

</div>

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Services](#-services)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Deployment](#-deployment)

## 🎯 Overview

MogroShop is a robust, cloud-native e-commerce platform designed with scalability and resilience in mind. Built using modern microservices architecture, it leverages the power of .NET 8, Docker, and Kubernetes to provide a seamless shopping experience.

## 🏗️ System Architecture

```mermaid
graph TB
    Client[Client Applications] --> API[API Gateway]
    
    subgraph Microservices
        API --> Auth[Auth Service]
        API --> Product[Product Service]
        API --> Cart[Cart Service]
        API --> Payment[Payment Service]
    end
    
    subgraph Databases
        Auth --> AuthDB[(PostgreSQL<br/>Auth DB)]
        Product --> ProductDB[(PostgreSQL<br/>Product DB)]
        Cart --> CartDB[(PostgreSQL<br/>Cart DB)]
        Cart --> Redis[(Redis Cache)]
        Payment --> PaymentDB[(PostgreSQL<br/>Payment DB)]
    end
    
    subgraph Message Broker
        Auth --> RMQ[RabbitMQ]
        Product --> RMQ
        Cart --> RMQ
        Payment --> RMQ
    end

    style Client fill:#f9f,stroke:#333,stroke-width:2px
    style API fill:#bbf,stroke:#333,stroke-width:2px
    style Auth fill:#dfd,stroke:#333,stroke-width:2px
    style Product fill:#dfd,stroke:#333,stroke-width:2px
    style Cart fill:#dfd,stroke:#333,stroke-width:2px
    style Payment fill:#dfd,stroke:#333,stroke-width:2px
    style RMQ fill:#fdd,stroke:#333,stroke-width:2px
```

## 🔄 Service Communication Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as Auth Service
    participant P as Product Service
    participant CA as Cart Service
    participant PA as Payment Service
    
    Note over C,PA: Authentication Flow
    C->>A: Login Request
    A->>C: JWT Token
    
    Note over C,PA: Shopping Flow
    C->>P: Get Products
    P->>C: Product List
    
    C->>CA: Add to Cart
    CA->>P: Verify Product
    P->>CA: Product Details
    CA->>C: Cart Updated
    
    Note over C,PA: Payment Flow
    C->>PA: Create Payment
    PA->>A: Verify User
    PA->>CA: Get Cart
    PA->>C: Payment Result
```

## 🚀 Services

### 🔐 Auth Service (Port: 5062)
User management and authentication service.

```mermaid
classDiagram
    class AuthController {
        +Register(RegisterDto)
        +Login(LoginDto)
        +ValidateToken(string)
    }
    class AuthService {
        +CreateUser(User)
        +ValidateUser(string, string)
        +GenerateToken(User)
    }
```

### 📦 Product Service (Port: 5167)
Product catalog and inventory management.

```mermaid
classDiagram
    class ProductController {
        +GetProducts()
        +GetProduct(string)
        +CreateProduct(ProductDto)
        +UpdateProduct(string, ProductDto)
    }
    class ProductService {
        +GetProductById(string)
        +UpdateStock(string, int)
        +ValidateProduct(string)
    }
```
