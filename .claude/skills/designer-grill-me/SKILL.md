---
name: designer-grill-me
description: UI/UX design exploration for responsive, accessible, and consistent interfaces
user_invocable: true
args: design_topic
---

# Designer Grill Me — UI/UX Design Exploration

Explore UI/UX design decisions through focused questioning. Produces design decisions only — no code. Implementation is handled by `/implement-ui`.

## Steps

1. **Get the design topic:**
   - If `{design_topic}` is provided, use it as the starting point
   - Otherwise, ask the user what UI/UX area they want to explore

2. **Read context:**
   - Read `docs/component-library.md` for available primitives
   - Read `docs/DESIGN.md` for design system tokens and rules
   - Read relevant existing pages/components to understand current state

3. **Explore design dimensions** (1-3 questions per message):

   **Layout & Structure**
   - Content hierarchy and information architecture
   - Grid and spacing decisions
   - Desktop vs. mobile layout differences

   **Interaction Patterns**
   - User flows and navigation
   - Form behavior (inline edit, modal, page)
   - Loading, empty, and error states
   - Feedback mechanisms (toasts, inline messages, progress)

   **Visual Design**
   - Color usage within the existing token palette
   - Typography choices
   - Iconography and visual indicators
   - Card/surface usage and elevation

   **Accessibility**
   - Keyboard navigation flow
   - Screen reader experience
   - Focus management
   - Color contrast and non-color indicators

   **Responsiveness**
   - Desktop primary layout
   - Mobile adaptations (NAS stats use case)
   - Breakpoint behavior
   - Touch targets

   **Component Needs**
   - Which existing primitives apply
   - What new primitives might be needed
   - Patterns that should become shared components

4. **Keep it focused:**
   - Work through one dimension at a time
   - Skip dimensions that are obviously not relevant
   - Challenge assumptions and point out inconsistencies
   - Reference existing patterns in the codebase rather than asking the user
   - Summarize decisions before moving on

5. **Produce a design summary:**
   - Decisions made, grouped by dimension
   - Primitives to use / create
   - Wireframe description (text-based layout sketch if helpful)
   - Open questions (if any)

6. **Offer next steps:**
   - Implement with `/implement-ui`
   - Create a GitHub issue for larger work
   - Continue exploring another area

## Principles

- Design decisions, not code — `/implement-ui` handles implementation
- Respect the existing design system (DESIGN.md tokens, Electric Iris palette)
- Desktop-first but mobile-aware
- WCAG AA is non-negotiable
- Prefer existing primitives over new ones
- Concise questions, specific trade-offs
