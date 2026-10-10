# 🎯 Upgrade SunCalcSharp to .NET 10

## Understanding
We will upgrade the SunCalcSharp solution to the confirmed target framework net10.0. Assessment shows both projects are already compatible: SunMoonCalculator is already multi-targeting net10.0 platforms and SunCalcSharp is a netstandard2.0 library that is compatible with .NET 10 consumers. The goal of this plan is to ensure project TFMs are explicit and consistent, apply any recommended package updates, build and validate the solution, and close any small compatibility gaps discovered during execution.

## Assumptions
- The assessment (assessment.md) is accurate and up-to-date for the repository state on branch upgrade-dotnet-10.
- The SunCalcSharp library can remain as netstandard2.0 or be multi-targeted to include net10.0; either approach is acceptable for compatibility with net10.0 consumers.
- The SunMoonCalculator MAUI project already targets net10.0 platform TFM variants and only needs package/version sanity checks and build validation.
- Tests were removed from the repo earlier; no test-run step is required. If tests exist later, they will be executed during validation steps.
- Commit strategy: After Each Task (per scenario-instructions.md)

## Approach
- Use a minimal, conservative upgrade: prefer to keep library compatibility by multi-targeting SunCalcSharp to include net10.0 where beneficial; otherwise leave netstandard2.0 in place if no API changes are needed. Update packages only when recommended by the assessment or to resolve build/warning issues. Validate by building the full solution and fixing compile-time errors and warnings (treat warnings as errors per workflow rules).
- Create explicit tasks in plan.md for: (1) align project TFMs, (2) update MAUI project packages and project properties, (3) build & fix, (4) final validation and clean-up. Each task is small and verifiable so commits are made after each task.

## Key Files
- src\SunCalcSharp\SunCalcSharp.csproj - library project (netstandard2.0)
- SunMoonCalculator\SunMoonCalculator.csproj - MAUI app (net10.0-*)
- .github\upgrades\scenarios\dotnet-version-upgrade\assessment.md - assessment we used to create plan
- .github\upgrades\scenarios\dotnet-version-upgrade\scenario-instructions.md - confirmed preferences

## Risks & Open Questions
- DECISION: Should we multi-target the library (netstandard2.0;net10.0) or leave it as netstandard2.0? Default: multi-target to add net10.0 alongside netstandard2.0 to maximize compatibility. If you prefer to leave it as netstandard2.0, tell me now.
- Risk: Package update may introduce minor API changes; we will update only when necessary.

**Progress**: 33% [███░░░░░░░]

**Last Updated**: 2026-07-24 22:50:09

## 📝 Plan Steps
- ✅ **start-plan-md: Create plan.md describing tasks and success criteria**
- ✅ **task-01-align-library: Multi-target or confirm SunCalcSharp library**
- 🔄 **task-02-verify-maui: Validate MAUI project and packages**
-  **task-03-build-and-fix: Build solution and fix compilation/warning issues**
-  **task-04-finalize: Final validation, cleanup, commits**
-  **finish: Complete scenario and provide a summary report**

