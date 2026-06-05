# PLANOVA DATABASE STRATEGY

## Philosophy

Database is the single source of truth.

Excel is an integration layer only.

---

# Primary Database

SQLite

Provider:

Microsoft.EntityFrameworkCore.Sqlite

---

# Architecture

UI

↓

Application

↓

Domain

↓

Persistence

↓

SQLite

---

# Schema Categories

## Master Data

Projects

Clients

Contracts

Users

Companies

---

## Planning Data

WBS

Activities

Calendars

Relationships

Milestones

Baselines

---

## Resource Data

Resources

Resource Types

Crews

Assignments

---

## Cost Data

Budgets

Costs

Cash Flow

Cost Loading

---

## Primavera Data

XER Projects

XER Activities

XER Resources

XER Calendars

XER Codes

---

## Claims Data

Claims

Claim Events

Claim Costs

Claim Delays

---

## Chronology Data

Events

Timeline Records

Evidence

Documents

---

## Correspondence Data

RFIs

Letters

Instructions

Emails

Submittals

---

## AI Data

Prompts

Responses

Knowledge Chunks

Embeddings

---

# Naming Standards

Table:

PascalCase

Entity:

PascalCase

Primary Key:

Id

Foreign Keys:

EntityId

---

# Migrations

EF Core Migrations

Code First

Version Controlled
