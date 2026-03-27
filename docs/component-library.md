# Feirb Component Library

Shared UI primitives in `src/Feirb.Web/Components/UI/`.
Global styles in `src/Feirb.Web/wwwroot/css/app.css` (prefixed `feirb-`).

## Icon

Renders a Bootstrap Icon with consistent sizing.

```razor
<Icon Name="gear" />
<Icon Name="pencil" Size="IconSize.Small" />
<Icon Name="envelope" Size="IconSize.Large" />
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Name` | `string` (required) | — | Icon name without `bi-` prefix |
| `Size` | `IconSize` | `Default` | `Small` (0.875rem), `Default` (1.25rem), `Large` (2rem) |
| `Class` | `string?` | `null` | Extra CSS classes |

## Heading

Semantic heading with optional icon and subtitle.

```razor
<Heading Level="HeadingLevel.Large" Icon="gear">Settings</Heading>
<Heading Level="HeadingLevel.Medium" Subtitle="Configure your account">Profile</Heading>
<Heading Level="HeadingLevel.Small">Section Title</Heading>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Level` | `HeadingLevel` | `Large` | `Large` (h1), `Medium` (h2), `Small` (h3) |
| `Icon` | `string?` | `null` | Icon name (without `bi-` prefix) |
| `Subtitle` | `string?` | `null` | Subtitle text below the heading |
| `ChildContent` | `RenderFragment` (required) | — | Heading text/content |
| `Class` | `string?` | `null` | Extra CSS classes |

## Button

Standard button with optional icon.

```razor
<Button Variant="ButtonVariant.Primary" OnClick="Save">Save</Button>
<Button Variant="ButtonVariant.Danger" Size="ButtonSize.Small" Icon="trash">Delete</Button>
<Button Icon="download" IconPosition="IconPosition.Right">Export</Button>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Variant` | `ButtonVariant` | `Primary` | `Primary`, `Secondary`, `Danger`, `Warning` |
| `Size` | `ButtonSize` | `Medium` | `Small`, `Medium`, `Large` |
| `Icon` | `string?` | `null` | Icon name (without `bi-` prefix) |
| `IconPosition` | `IconPosition` | `Left` | `Left` or `Right` |
| `ChildContent` | `RenderFragment?` | — | Button text/content |
| `OnClick` | `EventCallback<MouseEventArgs>` | — | Click handler |
| `Disabled` | `bool` | `false` | Disabled state |
| `Type` | `string` | `"button"` | HTML button type |
| `Title` | `string?` | `null` | Tooltip |
| `Class` | `string?` | `null` | Extra CSS classes |

## CircularButton

Icon-only circular button. Requires `AriaLabel` for WCAG AA.

```razor
<CircularButton AriaLabel="Settings" Variant="ButtonVariant.Primary" OnClick="OpenSettings">
    <Icon Name="gear" />
</CircularButton>
<CircularButton AriaLabel="Delete" Variant="ButtonVariant.Danger" Size="ButtonSize.Small" OnClick="Remove">
    <Icon Name="x" Size="IconSize.Small" />
</CircularButton>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaLabel` | `string` (required) | — | Accessible name |
| `Variant` | `ButtonVariant` | `Primary` | Visual variant |
| `Size` | `ButtonSize` | `Medium` | `Small` (2rem), `Medium` (2.5rem), `Large` (3rem) |
| `ChildContent` | `RenderFragment?` | — | Typically an `<Icon>` |
| `OnClick` | `EventCallback<MouseEventArgs>` | — | Click handler |
| `Disabled` | `bool` | `false` | Disabled state |
| `Title` | `string?` | `null` | Tooltip (falls back to AriaLabel) |
| `Class` | `string?` | `null` | Extra CSS classes |

## Card

Minimal card container.

```razor
<Card>
    <h3>Title</h3>
    <p>Content goes here.</p>
</Card>
<Card Class="p-4">
    <p>Card with extra padding.</p>
</Card>
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment` (required) | — | Card content |
| `Class` | `string?` | `null` | Extra CSS classes |

## Enums

Defined in `UIEnums.cs`:

- **`IconSize`**: `Small`, `Default`, `Large`
- **`ButtonVariant`**: `Primary`, `Secondary`, `Danger`, `Warning`
- **`ButtonSize`**: `Small`, `Medium`, `Large`
- **`IconPosition`**: `Left`, `Right`
- **`HeadingLevel`**: `Large`, `Medium`, `Small`

## Design Tokens

All primitives use CSS custom properties from `app.css`. Key tokens:

| Token | Value | Usage |
|-------|-------|-------|
| `--bs-primary` | `#b6004f` | Primary actions |
| `--feirb-on-surface` | `#2d2d43` | Secondary actions, text |
| `--feirb-error` | `#b31b25` | Danger actions |
| `--bs-warning` | `#FFE04A` | Warning actions |
| `--feirb-shadow` | `0 8px 24px rgba(0,0,0,0.06)` | Card shadow |
| `--bs-border-radius` | `1rem` | Card corners |
