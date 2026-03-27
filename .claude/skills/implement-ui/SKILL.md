---
name: implement-ui
description: Implement UI work using the shared component library with consistent styling enforcement
user_invocable: true
args: description_of_ui_work
---

# Implement UI

Implement UI work using registered primitives from the component library. Enforces consistent styling across all pages.

## Steps

1. **Read the component registry:**
   ```
   docs/component-library.md
   ```

2. **Read the page/component context** — understand what exists, what needs to change.

3. **Draft a short plan** for the UI work:
   - Which primitives will be used
   - Which elements need to change
   - Any new primitives needed
   - Ask the user to confirm before proceeding

4. **Implement using registered primitives:**
   - Replace raw Bootstrap button classes with `<Button>` or `<CircularButton>`
   - Replace raw `<i class="bi bi-*">` with `<Icon>`
   - Replace raw card markup with `<Card>`
   - Follow existing page patterns for layout and structure

5. **WCAG AA check:**
   - All interactive elements are keyboard-focusable
   - `aria-label` on icon-only buttons (CircularButton enforces this)
   - Contrast meets 4.5:1 for text, 3:1 for large text and UI components
   - Semantic HTML elements where appropriate
   - No color-only indicators

6. **Responsive check:**
   - Desktop primary, mobile supported
   - Use Bootstrap breakpoints (`col-sm-`, `col-md-`, `col-lg-`) for layout
   - Test that content doesn't overflow on small screens

7. **If a small primitive is missing** (e.g., a Badge, Divider, or similar):
   - Pause implementation
   - Propose the primitive with 2-3 design questions:
     - What variants does it need?
     - What sizes?
     - Any accessibility requirements?
   - After user confirms, create it in `src/Feirb.Web/Components/UI/`
   - Add styles to `app.css` with `feirb-` prefix
   - Update `docs/component-library.md`
   - Continue implementation

8. **If a large primitive is missing** (e.g., Modal, DataTable, Toolbar redesign):
   - Propose a GitHub issue for the primitive
   - User decides: create issue and defer, or build it now
   - If building now, follow the same pattern as step 7 but with full design exploration

9. **If UX exploration is needed** (interaction patterns, layout decisions, responsive behavior):
   - Suggest using `/designer-grill-me` for design exploration
   - Wait for design decisions before implementing

## Hard Rules

- **If a primitive exists, use it** — never emit raw Bootstrap classes for elements covered by the library
- Non-covered elements (modals, forms, tables, menus): use Bootstrap classes but encourage componentization
- If a non-covered pattern appears 2+ times, create a refactoring issue
- All icon-only buttons MUST use `<CircularButton>` with `AriaLabel`
- All icons MUST use `<Icon>` component
- All standalone buttons MUST use `<Button>` component
- Cards for content containers MUST use `<Card>` component

## Conventions

- Follow all conventions from CLAUDE.md (file-scoped namespaces, primary constructors, etc.)
- Follow `/new-blazor-component` conventions for any new components
- All user-facing strings through `IStringLocalizer<SharedResources>`
- Add `.resx` keys to all locale files when adding new strings
- Scoped CSS (`.razor.css`) for page-specific layout; global `app.css` for primitive styles only

## After Implementation

- List which primitives were used
- List any new primitives created
- List any `.resx` keys added
- Note any migration opportunities spotted (existing pages using raw Bootstrap for covered elements)
