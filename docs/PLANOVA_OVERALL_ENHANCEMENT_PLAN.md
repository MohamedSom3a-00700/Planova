# Planova Overall Enhancement Plan

**Status**: Draft  
**Source**: Converted and organized from `0.Planova overall.txt`

## Purpose

This document consolidates the requested UI and feature updates across several Planova areas into one structured plan:

- Dashboard
- Projects
- Parties
- BOQ Studio
- WBS Studio

The focus is on cleaning up the raw notes into a readable implementation plan that can later be split into separate phase or feature specs.

## Planning Notes

- Several items request a visual refresh using icons and Fluent UI styling.
- Some items are explicitly XAML-only.
- BOQ and WBS are tightly connected and should be treated as related workstreams.
- WBS is intended to become the central structure that connects projects, BOQ, activities, resources, costs, documents, and future AI support.

---

## 1. Dashboard

### Goals

- Redesign the dashboard with a stronger visual hierarchy.
- Use a light glass-effect presentation where appropriate.
- Introduce Fluent UI cards only when they improve readability.
- Add icons to improve scanability.
- Show all studio details and summary charts on the dashboard.

### Notes

- Keep the redesign consistent with the existing Planova visual language.
- Prioritize clarity and quick status visibility over decoration.

---

## 2. Projects

### Goals

- Redesign the Projects screen with icons.
- Keep the redesign XAML-only.
- Add richer project metadata and status indicators.

### Requested Data

- Progress percentage
- Health indicator
- Current data date
- Budget
- Number of activities
- Last updated date
- Project logo
- Project cover image
- Connected XER
- Connected database
- Last import date
- Last export date

### Project Controls

- Add a button to open the project folder.
- Add a preview panel for project documents.
- Support opening documents in Excel or PDF where applicable.
- Add a project layout selector for images, DWG, or PDF.
- Add Google Maps link support and a location preview after saving the link.

### Cost and Performance Cards

- Original budget
- Current budget
- Actual cost
- Earned value
- CPI
- SPI

### Project Health Panel

- Schedule health percentage
- Cost health percentage
- Risk level
- Critical activities count

### Folder Auto-Creation

When a project folder is chosen, automatically create the following structure:

1. Contract
2. BOQ
3. Drawing
4. Specification
5. Schedule
6. Claim
7. Reports
8. Correspondence
9. Photo
10. Meeting & Presentations
11. Archive
12. Lookahead
13. Logos

### Layout and Navigation

- Keep parties visible in a separate rail under the project area.
- Include clients, main contractor, subcontractors, and consultant in the project experience.

---

## 3. Parties

### Goals

- Redesign the Parties area for:
  - Clients
  - Main contractor
  - Subcontractors
  - Consultant
- Show full details and a logo for each party.
- Integrate parties with projects.

### Notes

- Party data should be easy to reuse across the project workflow.
- Keep the relationship between parties and projects explicit.

---

## 4. BOQ Studio

### Goals

- Redesign BOQ Studio with icons.
- Keep the redesign XAML-only.
- Remove outdated or redundant tabs and streamline the import flow.
- Tighten integration with Projects.

### Requested Changes

- Detect headers automatically when importing BOQ from Excel or project documents.
- Use AI to identify the correct column mapping.
- Remove the tab editor.
- Remove the validate BOQ tab and merge validation into the import flow.
- Remove the libraries tab.
- Integrate BOQ Studio with ProjectId so BOQ data stays project-specific.
- Add split view in the tree tab.
- Add a preview grid before BOQ import in a list view with split view.
- Add BOQ summary to reports.
- Add cost summary to reports.
- Add trade summary to reports.
- Add CSI summary to reports.
- Add Tender BOQ export.
- Add Client BOQ export.
- Add Excel export.
- Add PDF export.
- Add BOQ <-> Schedule integration.

### Multi-Sheet Excel Import

BOQ import should support multiple worksheets and handle large files efficiently.

#### Import Rules

1. When an Excel file is selected, scan all worksheets.
2. Automatically detect BOQ worksheets using common columns such as:
   - Code
   - Description
   - Unit
   - Qty
   - Quantity
   - Rate
   - Amount
   - Minimum number of populated rows
3. Ignore non-BOQ worksheets such as:
   - Cover
   - Summary
   - Index
   - Instructions
   - Notes
   - Calculation
4. Show all detected BOQ worksheets in a list.
5. Add import mode options:
   - Import selected sheet only
   - Import multiple sheets separately
   - Merge all BOQ sheets into one BOQ
6. When merging all BOQ sheets:
   - Create a root BOQ.
   - Create one section node per worksheet.
   - Import all rows under their worksheet section.
   - Preserve the original sheet order from Excel.
7. Display an import summary:
   - Total sheets imported
   - Total sections created
   - Total BOQ items imported
   - Total amount
8. Save the original worksheet name as `SourceSheet` for traceability.
9. Handle different column layouts by applying the column mapping engine per worksheet.
10. Support very large Excel files without loading unnecessary worksheets into memory.

---

## 5. WBS Studio

### Vision

WBS Studio should become the central Work Breakdown Structure management module inside Planova. It should not behave like a standalone editor. Instead, it should be the structural backbone that connects:

- Projects Module
- BOQ Studio
- Activity Studio
- Resource Studio
- Cost Studio
- Documents Module
- AI Services

### Core Goals

- Redesign WBS Studio with icons.
- Keep the redesign XAML-only.
- Support a central WBS lifecycle and richer integration points.

### Tabs

- List
- Tree
- Editor
- Mapping
- Templates
- AI Generation
- Reports
- Settings

Each tab should have one clear responsibility.

### 5.1 List Tab

#### Purpose

Acts as the master repository for all WBS definitions created within the project.

#### Sources

A WBS can be generated from:

- BOQ Mapping
- Template
- AI Generation
- Manual Creation
- Import
- Future Primavera Import

#### Grid Columns

- Name
- Description
- Status
- Source
- Revision
- Items Count
- Weight
- Created By
- Created Date
- Modified Date

#### Actions

- Create WBS
- Duplicate WBS
- Rename WBS
- Delete WBS
- Archive WBS
- Export WBS

#### Source Types

- BOQ
- Template
- AI
- Manual
- Imported
- Primavera

#### Status

- Draft
- Under Review
- Approved
- Archived

### 5.2 Tree Tab

#### Purpose

Read-only visual viewer of a selected WBS.

#### Controls

- Select WBS
- Expand All
- Collapse All
- Search Node

#### View Modes

- Standard
- Primavera Colors
- Weight View
- Responsibility View

### 5.3 Editor Tab

#### Purpose

Primary WBS editing workspace.

#### Layout

- WBS Tree
- Properties Panel

#### Top Controls

- Select WBS
- Load Template

#### Editing Actions

- Add Root
- Add Child
- Add Sibling
- Delete
- Duplicate
- Rename

#### Reordering

Support both:

- Button-based ordering:
  - Move Up
  - Move Down
  - Indent
  - Outdent
- Drag and drop:
  - Change order
  - Change parent
  - Change level

#### Context Menu

- Add Child
- Add Sibling
- Duplicate
- Rename
- Delete
- Move Up
- Move Down
- Indent
- Outdent

#### Properties Panel

- Code
- Name
- Description
- Level
- Weight
- Owner
- Discipline
- Deliverable
- Start Date
- Finish Date
- Notes

#### Auto Code Generation

Support structured codes such as:

- 1
- 1.1
- 1.2
- 1.2.1
- 1.2.2
- 2
- 2.1

Codes should regenerate automatically after structure changes.

### 5.4 Mapping Tab

#### Purpose

Generate WBS structures from BOQ Studio.

#### Workflow

1. Select existing BOQ.
2. Select mapping method.
3. Generate preview.
4. Create WBS.

#### Mapping Methods

- By Section
- By CSI
- By Cost Code
- By Trade
- By Discipline

#### BOQ Integration

WBS generation can use:

- BOQ
- Sections
- Cost Codes
- Trade Codes
- CSI Codes

### 5.5 Templates Tab

Keep the existing implementation and add support for:

- Building
- Infrastructure
- Landscape
- Roads
- Water Network
- Sewer Network
- Industrial
- Power Plant

#### Actions

- Apply Template
- Export Template
- Import Template
- Save As Template

### 5.6 AI Generation Tab

#### Remove

- Import Excel

Excel import belongs exclusively to BOQ Studio.

#### AI Sources

**Source 1: BOQ Studio**

- Sections
- Items
- Trade Codes
- Cost Codes
- CSI

**Source 2: Project Documents**

- Contract
- Scope
- Specifications
- Drawings
- Method Statements

**Source 3: BOQ + Documents**

- Recommended mode

#### Workflow

1. Select project.
2. Select BOQ.
3. Select documents.
4. Generate WBS hierarchy.
5. Accept the result and create a new WBS in the List tab.

### 5.7 Planova Integration

#### Projects Module

Each WBS belongs to a `ProjectId`.

#### BOQ Studio

Relationship:

- One Project
  - Many BOQs
- One BOQ
  - Many WBS Structures

#### Activity Studio

Future integration:

- WBS Node
  - Activities

Example:

- Roads
  - Excavation
  - Subbase
  - Asphalt

#### Resource Studio

Future integration:

- WBS Node
  - Resources

#### Cost Studio

Future integration:

- WBS Node
  - Cost Accounts

#### Documents Module

Future integration:

- WBS Node
  - Linked Documents

### 5.8 Reports Tab

Reports:

- WBS Dictionary
- WBS Summary
- WBS Weight Report
- Responsibility Matrix
- Hierarchy Report

### 5.9 Export Formats

- Excel
- PDF
- CSV
- JSON

### 5.10 Settings Tab

#### Default Values

- Default WBS Level
- Default Duration
- Default Weight

#### Behavior

- Auto Generate Codes
- Redistribute Weights
- Enable Drag Drop
- Enable AI Suggestions
- Use Primavera Colors

### 5.11 Future Phase

#### WBS Versioning

Support revisions such as:

- Infrastructure Rev0
- Infrastructure Rev1
- Infrastructure Rev2

#### Compare WBS

Compare two revisions and show:

- Added Nodes
- Deleted Nodes
- Modified Nodes

---

## Final Workflow

```text
Project
  ->
BOQ Studio
  ->
WBS Studio
  ->
Activity Studio
  ->
Resource Studio
  ->
Cost Studio
  ->
Reports & Analytics
```

WBS is the central project structure and the foundation for planning, cost control, resources, reporting, and AI-assisted project management inside Planova.

## Next Step

If needed, this plan can be split into smaller implementation specs for each studio so each area can be tracked independently.
