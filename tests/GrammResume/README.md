# GRAMM multi-instance resume review

After an interrupted multi-instance run, results can contain several completed ranges separated by gaps. The previous launch path resumed after the first range and recalculated later completed situations. The new Windows path checks every expected result, assigns only pending situations, and shows each instance's progress.

## Scope and review order

This branch targets current V2701. It contains no source-group ID, filename codec, GRAL engine, GRAMM parser, CustomInit editor or scientific-input changes. The maintainer's comments on GRAL #54 and #56 informed the separation of the engine proposals; GUI #97's CustomInit proposal is excluded.

1. `GrammResumePlan`: validate expected wind/stability results, keep completed work, split pending counts so assignments differ by at most one. One assignment may contain several contiguous ranges. GRAMM receives each range through its existing start/end interface.
2. `GrammConsoleMonitor`, `GrammProgress`, `GrammResumeRunner`: retain console progress from the existing executable, launch ranges, detect input waits and nonzero exits, verify fresh results, cancel owned processes and preserve progress across range transitions. Hidden console monitoring is needed because the engine writes via console APIs. A process with missing input can otherwise appear to have stalled without a visible window.
3. `Main_GrammResume` and `GrammProgressForm`: connect the runner to existing controls, ignore first-instance-only watcher updates while it owns progress, and display English status using the original system colors and standard controls. The dashboard adapts from one to four columns and scrolls for larger instance counts.
4. `GrammCpuScheduler`: optional Windows CPU Set preferences, isolated from weather assignment. Automatic uses reported performance classes and preferred-core ranks. An explicit larger-L3 option supports asymmetric cache CPUs. Completing workers release preferences so remaining running processes can move to better cores without a restart. Unsupported topology, oversubscription or API failure falls back to Windows scheduling. CPU Sets are preferences, not exclusive reservations.

The independent-situation runner is used for supported multi-instance runs only. Time-dependent and incompatible output modes retain the original path. Existing scientific input/result locks remain enforced. New code and hooks are excluded from the Mono compilation path; that path has not been built or run in this validation.

## Validation

Run `tests/GrammResume/Run.ps1 -WorkDirectory <new-empty-directory>` on Windows with .NET 10 SDK. Four suites cover 30 planner cases, 40 CPU checks, 77 runner checks, and 12 GUI tests with 835 assertions. GUI tests render actual WinForms controls at multiple widths and instance counts, including 64 instances, separated assignments and the CPU settings dialog.

The runner tests check gap preservation, exactly-once range execution, cancellation and restart, false completion, partial/stale results, failure exit codes, input waits and missing initialization. They use a controlled test child. The test harness also exposes `--real-core` for a small independent GRAMM fixture; the unchanged engine was exercised during local package validation, but full-domain completion is not claimed.

Native CPU tests on a Ryzen 9 9950X3D detect 96 MB and 32 MB L3 domains and move an existing test child with the same PID. Intel P/E selection and promotion use synthetic topologies; no physical Intel test or production speedup is claimed. A larger cache is not assumed to be faster for every GRAMM run.

This is submitted as a draft so the maintainer can review the runner design and optional CPU policy separately before choosing the merge sequence.
