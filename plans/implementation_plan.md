# SelectLocation Redesign & Range Calendar Component

## Overview
This plan details the implementation of a new calendar component (`RangeCalendarView`) and the redesign of [`SelectLocation.xaml`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Views/SelectLocation.xaml) based on the user's mockup. The component replaces the two separate `DatePicker` controls with an integrated calendar that allows users to select a single day or a series of days (date range) constrained to a window from **Today** to **Today + 16 days**.

![SelectLocation Mockup](C:/Users/jtbos/.gemini/antigravity/brain/267c904a-7d06-486a-8095-3df4ca8a2acb/mockup_select_location.png)

---

## User Review Required
> [!IMPORTANT]
> - **Selection Interaction Model**:
>   1. **First tap** on an active day selects that day as `StartDate` and `EndDate` (single-day selection).
>   2. **Second tap** on a date on or after `StartDate` expands the range (`EndDate = selectedDate`).
>   3. **Tapping a date before `StartDate`** resets `StartDate` and `EndDate` to that new date.
>   4. **Tapping when a range is already selected** starts a fresh single-day selection on the tapped date.
> - **Screen Responsiveness**: The mockup displays a wide two-column layout (Calendar on the left, Location on the right). We will use a responsive `Grid` or wrap container that displays side-by-side on desktop/tablets and gracefully stacks on narrow mobile screens.

---

## Proposed Changes

### Component: RangeCalendarView (New Component)

Create a dedicated calendar control in `ThePhotographersForecast.Components` that encapsulates the visual layout and range selection logic matching the dark slate card in the mockup.

#### [NEW] [`RangeCalendarView.xaml`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Components/RangeCalendarView.xaml)
- **Container Styling**: Dark rounded card (`#2B2B2B` / `#282A2E`) with subtle border and rounded corners (`CornerRadius="16"`).
- **Header**:
  - Dynamically displays the month and year range spanning the 16-day window (e.g., `August 2026 - September 2026` or `September 2026`).
  - Days-of-week column headers: `Sun`, `Mon`, `Tue`, `Wed`, `Thu`, `Fri`, `Sat`.
- **Calendar Grid**:
  - 7-column layout displaying week rows covering the 17-day window (`Today` to `Today + 16 days`).
  - Days before `Today` in the initial week: Rendered as blank/dimmed rounded tiles matching the empty placeholders in the mockup.
  - Valid days (`Today` through `Today + 16 days`): Interactive rounded buttons showing the day number (e.g., `29`, `30`, `1`, `2`...).
  - Days after `Today + 16 days` to fill the final row: Rendered as blank/dimmed rounded tiles.
- **Selection Highlights**:
  - **Start & End Date**: Highlighted with accent brush (e.g., `#BEFCA6`) and bold dark text.
  - **In-Range Dates**: Soft tint background (e.g., `#406346` or light transparent green) connecting the range.
  - **Unselected Active Dates**: Crisp white rounded tiles with dark text, matching the mockup.

#### [NEW] [`RangeCalendarView.xaml.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Components/RangeCalendarView.xaml.cs)
- **Bindable Properties**:
  - `StartDate` (`DateTime`, `TwoWay`)
  - `EndDate` (`DateTime`, `TwoWay`)
  - `MinDate` (`DateTime`, default `DateTime.Today`)
  - `MaxDate` (`DateTime`, default `DateTime.Today.AddDays(16)`)
- **Internal State & Methods**:
  - `GenerateCalendar()`: Builds the day tile models based on `MinDate` and `MaxDate`.
  - `OnDateTapped(DateTime date)`: Implements the single-day and range selection state machine and updates visual states.

---

### View & ViewModel Updates

#### [MODIFY] [`SelectLocationViewModel.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/ViewModels/SelectLocationViewModel.cs)
- Update `MaxDate` from `AddDays(15)` to `AddDays(16)` to guarantee full 16-day offset from Today.
- Ensure `IsStartDateLoaded` and `IsEndDateLoaded` default to `true` (since the custom component binds directly without waiting on native platform DatePicker handler events).

#### [MODIFY] [`SelectLocation.xaml`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Views/SelectLocation.xaml)
- Replace the two `DatePicker` controls with `<components:RangeCalendarView ... />`.
- Restructure the page content into the two-column layout from the mockup:
  - **Left Column**:
    - Container box with title: `"Please select a date or date range of the photo shoot."`
    - `<components:RangeCalendarView StartDate="{Binding StartDate, Mode=TwoWay}" EndDate="{Binding EndDate, Mode=TwoWay}" MinDate="{Binding MinDate}" MaxDate="{Binding MaxDate}" />`
  - **Right Column**:
    - Title: `"Please provide me with the location of your photo shoot. Either click on the GPS button or enter a city and select the location from the suggestions list."`
    - GPS circular button with pin icon.
    - Search entry placeholder: `"Enter the location here..."`
    - Suggestions CollectionView with list dividers.

#### [MODIFY] [`SelectLocation.xaml.cs`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/Views/SelectLocation.xaml.cs)
- Remove deleted control references (`StartDatePicker.Loaded`, `EndDatePicker.Loaded`).

#### [MODIFY] [`ThePhotographersForecast.csproj`](file:///F:/source/ThePhotographersForecast/ThePhotographersForecast/ThePhotographersForecast.csproj)
- Register `Components\RangeCalendarView.xaml` with `<MauiXaml Update="Components\RangeCalendarView.xaml"><Generator>MSBuild:Compile</Generator></MauiXaml>`.

---

## Verification Plan

### Automated Build Verification
- Run compilation check:
  ```powershell
  dotnet build "ThePhotographersForecast\ThePhotographersForecast.csproj" -f net10.0-windows10.0.26100.0 -c Debug --no-restore
  ```
  Ensure 0 build errors.

### Manual Verification
1. **Launch App**: Open the `Select Location` tab.
2. **Visual Inspection**:
   - Verify the dark calendar card matches the mockup aesthetic (rounded corners, dark background, month title, 7-column header).
   - Verify days prior to today and after today + 16 days appear as empty rounded tiles.
   - Verify days from Today to Today + 16 days display correct day numbers.
3. **Single Day Selection**:
   - Tap a day tile (e.g., Today). Verify only that tile is selected and `StartDate == EndDate`.
4. **Range Selection**:
   - Tap a second day tile 4 days later. Verify the start, end, and all days in between are visually highlighted.
   - Check that subsequent searches or GPS fetches use the selected range.
5. **Boundary Enforcement**:
   - Verify dates past 16 days from today are not selectable.
