<!--
Sync Impact Report
Version change: template placeholders -> 1.0.0
Modified principles: Architecture First; MVVM & Fluent UI Enforcement; Modular Domain Design; Build vs Buy Strategy; Automation Platform Agnostic; AI Provider Agnostic; Multilingual First; Performance & Scalability
Added sections: none
Removed sections: none
Templates requiring updates: pending .specify/templates/plan-template.md, .specify/templates/spec-template.md, .specify/templates/tasks-template.md (reviewed; no content changes required for this constitution update)
Deferred items: none
-->

# Planova Constitution

## Core Principles

### I. Architecture First (NON-NEGOTIABLE)

Planova shall follow Clean Architecture.

Mandatory layers:

- UI
- Application
- Domain
- Infrastructure

Rules:

- Dependencies must always point inward.
- UI shall not contain business logic.
- Infrastructure shall not contain business rules.
- Domain shall remain independent of frameworks.
- Architectural boundaries shall never be bypassed.

Rationale:

Planova is a long-term enterprise platform and must remain maintainable,
testable, and scalable.

### II. MVVM & Fluent UI Enforcement (NON-NEGOTIABLE)

All user interface development shall follow MVVM.

Mandatory technologies:

- WPF
- Fluent UI WPF
- CommunityToolkit.Mvvm

Requirements:

- ViewModels contain presentation logic.
- Views remain declarative.
- Commands replace code-behind whenever possible.
- Dependency Injection is required.
- Large Ribbon-based interfaces are discouraged.

Official layout:

Navigation Rail + Multi-Tab Workspace.

Rationale:

Planova prioritizes workspace efficiency and engineering productivity.

### III. Modular Domain Design

Planova shall be developed as a collection of loosely coupled modules.

Official modules:

- Core
- BOQ Studio
- WBS Studio
- Activity Studio
- Resource Studio
- Cost Studio
- Reporting Center
- Primavera Studio
- Schedule Comparison Studio
- Delay Analysis Studio
- Claims Studio
- Chronology Studio
- Correspondence Center
- AI Copilot
- Knowledge Base
- Analytics Center
- Integration Hub

Requirements:

- Modules must expose clear contracts.
- Cross-module dependencies must be minimized.
- Shared abstractions belong in Shared or Application layers.

Rationale:

Independent evolution of modules is critical to long-term success.

### IV. Build vs Buy Strategy

Planova shall focus engineering effort on proprietary value.

Do NOT build:

- Workflow Engines
- Automation Designers
- Authentication Platforms
- Generic Search Engines
- Reporting Designers

Prefer mature solutions.

Examples:

- Microsoft Identity
- SQLite FTS5
- Qdrant
- QuestPDF
- n8n
- Power Automate
- Make
- Zapier

Engineering effort shall focus on:

- Primavera Studio
- Schedule Comparison
- Delay Analysis
- Claims Management
- Chronology Intelligence
- AI Project Controls

Rationale:

Competitive advantage comes from project controls expertise, not commodity
infrastructure.

### V. Automation Platform Agnostic

Planova shall remain independent of any automation platform.

Planova shall expose:

- REST APIs
- Webhooks
- Events

Workflow execution belongs to external platforms.

Recommended platforms:

- n8n
- Power Automate
- Make
- Zapier
- Node-RED
- CrewAI
- LangGraph
- AutoGen

Requirements:

- No internal workflow engine.
- No automation designer.
- No rule builder.

Rationale:

Automation ecosystems evolve faster than business applications.

### VI. AI Provider Agnostic

AI shall augment engineers, not replace engineering judgment.

Mandatory framework:

- Semantic Kernel

Supported providers:

- Ollama
- OpenAI
- Claude
- Gemini

Requirements:

- AI implementations must use abstractions.
- No module may directly depend on a specific vendor.
- Providers must be swappable through configuration.

Default provider:

- Ollama

Default development model:

- Llama 3.2

Rationale:

Provider independence protects long-term flexibility.

### VII. Multilingual First

Planova is multilingual from day one.

Mandatory languages:

- English
- Arabic

Requirements:

- Runtime language switching.
- RTL support.
- Localized resources.
- Localized reports.
- Localized prompts.
- No hardcoded user-facing strings.

Rationale:

Multilingual support is a core product requirement.

### VIII. Performance & Scalability

Requirements:

- Async by default.
- CancellationToken support.
- Background processing.
- Large dataset virtualization.
- Efficient database queries.
- UI thread blocking is prohibited.

Rationale:

Planning and scheduling datasets can become extremely large.

## Technology Standards

Planova shall use the following approved stack unless an architectural review
explicitly authorizes a substitution.

### Platform

- .NET 8
- WPF

### UI

- Fluent UI WPF
- CommunityToolkit.Mvvm

### Hosting

- Microsoft.Extensions.Hosting
- Dependency Injection

### Persistence

- SQLite
- EF Core

### Logging

- Serilog

### AI

- Semantic Kernel
- Ollama
- OpenAI

### Excel

- ClosedXML
- EPPlus
- Microsoft.Office.Interop.Excel

### Reporting

- QuestPDF

### Visualization

- LiveCharts2

### Database Standards

Primary database:

- SQLite

ORM:

- EF Core

Requirements:

- Code First Migrations
- Strong Entity Modeling
- Versioned schema
- Audit support where appropriate
- Repository pattern is prohibited unless justified by a documented
  architectural decision

Source of truth:

- Planova Database

Excel is an integration channel only.

## Development Workflow

Every feature shall follow:

1. Product Specification
2. Technical Design
3. Implementation Plan
4. Task Breakdown
5. Implementation
6. Validation
7. Documentation

Before implementation:

- Requirements must be understood.
- Architectural impact must be evaluated.
- Module ownership must be clear.

After implementation:

- Documentation updated.
- Tests added.
- Localization verified.

### Quality Gates

Every implementation must satisfy:

#### Architecture

- Clean Architecture compliant
- MVVM compliant
- Dependency Injection compliant

#### Quality

- Readable code
- Meaningful naming
- Single responsibility
- Minimal duplication

#### Localization

- English support
- Arabic support

#### Performance

- Async operations
- Efficient queries
- No unnecessary allocations

### Documentation Requirements

Every module must contain:

- Purpose
- Responsibilities
- Public interfaces
- Integration points
- Usage examples

Major architectural decisions require documentation.

### Security Requirements

Mandatory:

- Secure secret storage
- No credentials in source control
- API key protection
- Least privilege principle

Sensitive configuration must remain externalized.

## Governance

This constitution is the highest authority for development decisions.

Specifications, plans, tasks, source code, and architectural decisions must
comply with this constitution.

When conflicts occur:

1. Constitution
2. Approved Architecture
3. Technical Design
4. Implementation Plan
5. Source Code

Constitution amendments require:

- Documented rationale
- Architectural review
- Impact assessment

### Long-Term Vision

Planova shall evolve toward:

- Enterprise Deployment
- Multi-User Collaboration
- API Platform
- Knowledge Platform
- AI Project Intelligence Platform

All architectural decisions shall preserve this direction.

**Version**: 1.0.0 | **Ratified**: 2026-06-01 | **Last Amended**: 2026-06-01
