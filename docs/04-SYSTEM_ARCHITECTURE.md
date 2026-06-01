# PLANOVA SYSTEM ARCHITECTURE

## Technology Stack

* .NET 8
* WPF
* Fluent UI WPF
* CommunityToolkit.Mvvm
* EF Core
* SQLite
* Serilog
* Semantic Kernel
* OpenAI
* Ollama
* ClosedXML
* EPPlus
* Excel Interop
* QuestPDF
* LiveCharts2

---

## Solution Structure

Planova.sln

Planova.UI

Planova.Application

Planova.Domain

Planova.Infrastructure

Planova.Persistence

Planova.Excel

Planova.Primavera

Planova.DelayAnalysis

Planova.Claims

Planova.Chronology

Planova.Reporting

Planova.AI

Planova.API

Planova.Localization

Planova.Integrations

Planova.Shared

---

## Architectural Style

Clean Architecture

UI
↓
Application
↓
Domain
↓
Infrastructure

---

## Database

Primary:

SQLite

Future:

SQL Server

PostgreSQL

---

## Localization

Primary Languages:

English
Arabic

Features:

* RTL Support
* Dynamic Language Switching
* Resource Based Localization

---

## Automation Strategy

Planova is Automation Platform Agnostic.

Recommended:

* n8n

Supported:

* Power Automate
* Make
* Zapier
* Node-RED
* CrewAI
* LangGraph
* AutoGen
* Custom Agents

---

## Integration Model

Planova exposes:

* REST APIs
* Webhooks
* Events

External platforms execute workflows.

Planova focuses on business intelligence and project controls.
