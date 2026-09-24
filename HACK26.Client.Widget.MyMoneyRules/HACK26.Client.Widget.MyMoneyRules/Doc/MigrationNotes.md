# My Money Rules UI migration

## Purpose and migration phases

This ORB widget is the destination for the standalone My Money Rules concept.
The work is intentionally divided into three phases:

1. UI migration: complete Vue/Iris experience with fake in-memory data.
2. Controller integration: replace client-only mutations with MVC actions.
3. AI integration: replace the simulated rule parser with an approved AI service.

Only phase 1 is implemented. The UI is not yet persistent and must not be
treated as a production rule-management workflow.

## UI behavior carried forward

The migrated UI retains the following design decisions from the standalone
specification:

- The page has three vertical areas: the builder and automation summary,
  popular recipes, and existing rules.
- The builder has two modes: natural-language AI and a visible three-column
  When/If/Then step builder. The step builder collapses to one column at the
  Foundation 883px breakpoint.
- The AI path is simulated: it shows a two-second loading state, then presents
  a preview that can be saved in memory or edited in the step builder.
- A transfer action currently requires the member to enter a fixed amount.
  Recipes can prefill a transfer rule, but do not save it.
- A notification action displays registered contact choices as checkboxes.
  The fake UI contains one email contact and one SMS contact.
- Recipe selection switches to the step builder, pre-fills it, and scrolls to
  the builder for review. It does not create a rule.
- Existing rules use `iris-switch` for enabled/disabled state, not radio
  buttons. Historical YTD impact remains visible for inactive rules.
- The New rule action returns to the existing builder mode. When AI mode is
  active, it focuses the natural-language field.
- The automation summary opens a client-side slide-in drawer for savings,
  active rules, and completed actions.

The canonical protocol values for future server integration are:

| Field | Value | Meaning |
|---|---|---|
| `IfScenario` | `balance_over` | Account balance exceeds the configured amount |
| `IfScenario` | `spent_amount` | A transaction exceeds the configured amount |
| `IfScenario` | `specific_vendor` | Transaction merchant contains the configured value |
| `ThenAction` | `notify` | Send to the selected contact IDs |
| `ThenAction` | `transfer` | Transfer the configured amount to the configured account |

## Current implementation

The UI is now an Albus Vue 2 application:

- `Scripts/app.ts` mounts the root component.
- `Scripts/MyMoneyRulesApp.vue` contains the migrated UI and temporary fake
  data/state.
- `Styles/money-rules.css` contains only app-specific, token-based layout and
  interaction styling.
- `Views/HACK26MyMoneyRules/Index.cshtml` is a thin ORB Razor host containing
  the Vue mount point, anti-forgery token, and Albus injection markers.
- `Views/HACK26MyMoneyRules/Index.template.cshtml` is the Albus source template
  used to generate the injected Razor view.
- `albus.config.js` configures the Vue 2 preset, app entry point, and automatic
  asset injection.

The previous partial-based Razor implementation, its vanilla JavaScript, and
the view models used only by those views were removed. `Index()` currently
returns the host view without a page view model.

## Design system and build contract

The widget must use the Albus Vue 2 preset. It supplies:

- Vue 2 runtime support;
- current Iris Vue components;
- Iris Foundation utilities;
- theme-builder token stylesheet;
- compiled JavaScript and CSS injected into the Razor host.

Do not copy standalone CDN tags, token fallbacks, or ORB shell emulation into
this widget. ORB/Albus owns those assets and provides the themed widget shell.

Keep these Iris requirements when changing the Vue component:

- use `@button-click`, `@switch-change`, and `@checkbox-change`;
- use `is-disabled` and `is-selected`, not native `disabled` or switch
  `v-model`;
- keep `iris-select-dropdown` values as arrays;
- use explicit opening and closing tags for every `iris-*` component;
- use design tokens as RGB triplets, wrapped with `rgb()` or `rgba()`.

## Future controller integration

The next phase should add anti-forgery-protected actions to
`HACK26MyMoneyRulesController` and replace only the corresponding local state
mutations:

| Action | Intended operation |
|---|---|
| `SaveRule` | Create a rule from the AI preview or step builder |
| `ToggleRule` | Enable or disable an existing rule |
| `DeleteRule` | Soft-delete an existing rule |
| `GetSavingsDetail` | Supply savings detail for the drawer |
| `GetActionsDetail` | Supply per-rule action counts for the drawer |
| `GenerateRule` | Deferred until the AI integration phase |

All POST operations must send the request verification token rendered by
`@Html.AntiForgeryToken()`. Initial application state should later be provided
by a bootstrap model containing rules, recipes, contact preferences, options,
and statistics. Contacts should come from the member's
`AlertContactPreferences` records rather than the temporary values in Vue.

## Validation and current blocker

The .NET Framework widget build succeeded after the migration:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  "HACK26.Client.Widget.MyMoneyRules.csproj" `
  /t:Build /p:Configuration=Debug /p:PostBuildEvent=
```

The Albus frontend build has not been run. Company policy currently blocks
Albus, so the Vue source has not been compiled into `Scripts/app.min.js` and
`Styles/app.min.css`, and the generated Razor asset injection cannot yet be
browser-validated.

When an approved build environment is available, run:

```powershell
npm install
npm run albus -- init
npm run albus -- build
```

The project `.npmrc` intentionally routes unscoped packages to public npm and
`@alkami/*` packages to the company package feed. Do not work around an
organization policy or unavailable private feed by copying Albus packages from
unapproved sources.
