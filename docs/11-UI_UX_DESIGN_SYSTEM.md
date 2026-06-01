# PLANOVA UI/UX DESIGN SYSTEM

## Purpose

This document defines the official user experience, visual design language, layout system, navigation structure, theme architecture, and workspace behavior for Planova.

The goal is to provide a modern engineering-focused experience optimized for planning engineers, project controls engineers, delay analysts, and claims consultants.

---

# Design Philosophy

Planova is not a traditional ERP.

Planova is not a spreadsheet replacement.

Planova is not another Primavera clone.

Planova is an Engineering Intelligence Platform.

The UI must prioritize:

* Maximum workspace
* Minimum visual noise
* Fast navigation
* Data density
* AI assistance
* Enterprise usability

---

# Design Principles

## Workspace First

Every screen should maximize usable workspace.

Avoid wasting space on large ribbons.

---

## Information First

Data is more important than decoration.

Charts and visualizations must support decision making.

---

## Fluent Design

Follow Microsoft Fluent Design principles.

Use:

* Rounded corners
* Elevation
* Acrylic effects where useful
* Modern spacing
* Consistent iconography

---

## AI First

AI must be visible but never intrusive.

AI should appear as an assistant.

Not as a replacement for the engineer.

---

# Theme Architecture

## Default Theme

Dark Theme

---

## Supported Themes

### Dark

Background:

#0D1117

Surface:

#161B22

Card:

#1F2937

Border:

#2A3441

Primary Accent:

#00BFFF

Secondary Accent:

#0078D4

---

### Light

Background:

#F8F9FB

Surface:

#FFFFFF

Card:

#FFFFFF

Border:

#D6DCE5

Primary Accent:

#0078D4

Secondary Accent:

#00BFFF

---

# Typography

Primary Font

Segoe UI Variable

Fallback

Inter

---

# Icon System

Source:

Fluent UI Icons

Style:

Monochrome

Outlined

Minimal

---

# Layout Architecture

## Application Shell

┌─────────────────────────────────────┐
│ Title Bar                           │
├───────┬─────────────────────────────┤
│ Nav   │ Main Workspace              │
│ Rail  │                             │
│       │                             │
├───────┴─────────────────────────────┤
│ Status Bar                          │
└─────────────────────────────────────┘

---

# Navigation System

Navigation Rail

Left Side

Collapsible

Icons + Labels

---

## Main Navigation

Dashboard

Projects

BOQ Studio

WBS Studio

Activity Studio

Resource Studio

Cost Studio

Reports

Primavera Studio

Schedule Compare

Delay Analysis

Claims

Chronology

Correspondence

Knowledge Base

Analytics

Integration Hub

Settings

---

# Workspace Model

Planova uses a multi-tab workspace.

Example:

Project A

BOQ

Schedule

Claims

Delay Analysis

Each opens in a separate tab.

---

# Dashboard Design

Dashboard is not a report page.

Dashboard is a Command Center.

---

## Sections

Project Health

Recent Activities

Pending Tasks

Recent Claims

Recent Delays

Recent Reports

AI Suggestions

Quick Actions

---

# Home Screen

Welcome User

Today's Projects

Recent Files

Quick Actions

AI Assistant

Recent Activity

---

# Quick Actions

Generate WBS

Generate Activities

Import BOQ

Import XER

Analyze Delay

Create Claim

Generate Report

Open Project

---

# AI Copilot Panel

Right Side Panel

Collapsible

Persistent Across Modules

---

## Features

Ask Questions

Generate WBS

Analyze Schedule

Analyze Delay

Generate Claim Narrative

Generate Reports

Search Knowledge Base

---

# Data Grids

Must support:

Filtering

Sorting

Grouping

Export

Column Templates

Multi Selection

Frozen Columns

---

# Cards

Used for:

KPIs

Status

Project Health

Notifications

Analytics

---

# Reporting Screens

Use:

Filters

Preview

Export

Templates

History

---

# Primavera Studio

Dedicated Workspace

Not a dialog

Not a wizard

---

## Areas

Activities

Relationships

Resources

Calendars

Codes

Baselines

UDFs

Validation

---

# Delay Analysis Studio

Dedicated Professional Workspace

---

## Areas

Events

Windows

Schedules

Results

Narratives

Evidence

---

# Claims Studio

Claim Register

Claim Details

Attachments

Chronology

Analysis

Narrative

---

# Chronology Studio

Timeline View

Document View

Evidence View

AI Summary

---

# Integration Hub

Connected Systems

API Keys

Webhooks

Logs

Automation Platforms

---

# Localization

Supported Languages

English

Arabic

---

## Requirements

RTL Support

Dynamic Language Switching

Localized Reports

Localized AI Prompts

---

# Responsive Behavior

Desktop First

Large Monitors Optimized

Multi Monitor Friendly

---

# UI Technology

WPF

Fluent UI WPF

CommunityToolkit.Mvvm

LiveCharts2

QuestPDF

---

# Official Layout Choice

Selected Concept:

Concept 03

Navigation Rail + Multi Tab Workspace

Reason:

Maximum Workspace

Engineering Focused

Supports Complex Project Controls Workflows

Scales To Enterprise Features

Consistent With Planova Branding

---

# Future UI Vision

Planova should feel like a combination of:

Modern Engineering Software

Microsoft Fluent Applications

AI Copilot Experience

Enterprise Project Controls Platform

Without becoming another Primavera clone.
