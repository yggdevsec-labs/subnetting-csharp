# Development Instructions

Apply these rules to development work in this repository, only where relevant to the task. Do not add architecture, dependencies, validation, logging, or tests merely to satisfy this document.

## Priorities

- Meet explicit requirements while preserving correctness, security, authorization, and data integrity. If requirements conflict with these safeguards, explain the conflict before proceeding with the affected action.
- Follow existing project conventions and architecture where practical.
- Prefer the simplest maintainable solution. Optimize for measured bottlenecks or stated performance requirements.

## Working Process

- Read relevant code, tests, configuration, and repository guidance before editing. Verify APIs, libraries, schemas, and commands instead of assuming they exist.
- Reuse existing implementations and business rules. Make the smallest complete change that satisfies the request; avoid unrelated refactoring.
- Preserve existing behavior and required compatibility unless the task calls for a change. Do not overwrite unrelated work.
- Resolve routine implementation choices using project conventions. Ask for clarification when an unresolved requirement materially affects correctness, security, data integrity, or public behavior.
- Never weaken tests, validation, authorization, or error handling merely to make a change pass.
- Remove temporary debugging output and dead code introduced by the change.

## Design and Readability

- Keep functions and modules cohesive, with clear responsibilities, explicit dependencies, and predictable state ownership. Minimize shared mutable state.
- Keep business rules in a clear location. Separate business logic from UI, persistence, and integrations when the boundary provides practical value.
- Add layers, services, interfaces, repositories, or dependencies only for a concrete need. Avoid pass-through layers and premature abstractions; superficial duplication alone does not justify an abstraction.
- Prefer composition over inheritance where it keeps the design simpler. Avoid circular dependencies and hidden global behavior.
- Use consistent domain terminology and intention-revealing names. Split code by responsibility, not arbitrary line limits.
- Write code comments in English. Explain constraints and non-obvious decisions rather than repeating the code.

## Security and External Boundaries

- Treat external input and responses as untrusted. Validate relevant types, formats, lengths, ranges, and size limits at system boundaries.
- Enforce authorization at a trusted application boundary for each requested resource, including tenant isolation where applicable. Authentication or UI restrictions alone are insufficient.
- Use parameterized queries and safe APIs. Never interpolate untrusted input into executable commands or queries; encode output for its destination context.
- For files and external URLs, prevent path traversal and unintended internal-resource access. Validate permitted types, destinations, and appropriate size and redirect limits.
- Keep secrets outside source control. Never expose credentials, tokens, or unnecessary sensitive data in logs, errors, or responses.
- Use established authentication, password hashing, cryptography, and secure transport. Do not disable certificate validation or silently fall back to insecure behavior.
- Apply least privilege and secure defaults. Keep environment configuration separate from business logic and validate required settings early.
- Minimize sensitive data storage and access, including backups and telemetry. Do not use production credentials, services, or data in tests without explicit authorization and appropriate protection.
- Add dependencies only when needed; review their maintenance, security, and compatibility, and follow the project's version-locking conventions.

## Errors and Integrations

- Represent expected validation, business, authorization, and concurrency failures explicitly using project conventions. Keep domain errors independent of transport details where practical.
- Catch errors only to handle them, translate them, add useful context, or release resources. Preserve the original cause; do not silently discard failures.
- Log unexpected failures once at a boundary with enough context to investigate. Do not expose sensitive implementation details to callers.
- Set timeouts for external operations and propagate cancellation where supported. Release resources on both success and failure.
- Retry only transient failures, with bounded attempts and suitable backoff. Check that retries cannot create unintended duplicate effects.
- Do not report partial or queued work as completed. Define recovery for important partial failures.

## Data Integrity and Database Changes

- Inspect the existing schema and migrations. Use appropriate data types, relationships, and database constraints to protect important invariants alongside application validation.
- Use short transactions for changes that must succeed or fail together. Do not hold them open across slow external calls or unrelated work.
- Protect concurrent updates using atomic operations, constraints, version checks, or locking as appropriate. Do not silently overwrite newer data where correctness matters.
- Make schema changes through reviewed, reproducible, version-controlled migrations. Preserve existing data and deployment compatibility; do not edit deployed migrations unless the project permits it.
- Execute destructive schema or data changes only when explicitly authorized. Establish appropriate backup and recovery arrangements before high-risk operations.
- Retrieve only needed data, avoid N+1 queries, and bound large result sets. Use batching or streaming for large operations and indexes justified by actual access patterns.

## Async Work and Concurrency

Apply when the task involves concurrent updates, background jobs, or messaging:

- Parallelize only independent work and bound concurrency to downstream capacity. Keep locks short and acquire multiple locks consistently.
- Assume messages may be duplicated and, unless guaranteed otherwise, arrive out of order. Make relevant operations idempotent using durable identifiers, constraints, or processing records.
- Observe background failures. Define bounded retries, handling for permanently failing messages, and recovery or replay where required.
- A database transaction does not make external API calls or message publication atomic. Use compensation or reliable publication patterns only when consistency requirements justify them.
- Define cancellation, timeout, progress, and restart behavior for important long-running work. Do not rely on process-local state for cross-instance correctness.
- Add queues, distributed locks, or event-driven architecture only for a concrete requirement.

## Performance and Observability

- Measure representative workloads before optimizing. Fix identified bottlenecks and compare results; avoid speculative scaling infrastructure.
- Bound memory, batches, queues, and resource usage where workloads can grow. Respect external rate limits and resource-pool capacity.
- Introduce caching only with a concrete benefit and defined lifetime, invalidation, size limits, acceptable staleness, and failure behavior.
- Use structured logs and relevant non-sensitive identifiers. Add metrics, tracing, and actionable alerts only to answer operational questions.
- Use protected audit records when accountability is required. Follow appropriate access and retention rules for telemetry.

## Testing and Completion

- Test observable behavior at the lowest level that verifies it reliably. Prioritize changed business rules, edge cases, failure paths, security boundaries, and regressions.
- For bug fixes, add a regression test when practical; reproduce the failure before applying the fix when feasible.
- Keep tests deterministic and independent. Control time, randomness, and external dependencies where relevant. Do not hide flaky tests with arbitrary sleeps or retries.
- Use realistic infrastructure when database or integration semantics matter. Avoid excessive mocking and tests of trivial framework behavior.
- For concurrency-sensitive changes, verify relevant conflicts, duplicate execution, ordering, retries, and partial failures.
- Run relevant tests and applicable formatting, linting, type, and build checks. Start with targeted checks and broaden when warranted by the change or project requirements.
- Never claim checks passed unless they actually ran successfully. State any checks that could not run and why.
- Report what changed, verification results, and material assumptions or remaining limitations. Keep the explanation proportional to the task.
