## Neon
Neon is a comprehensive background job scheduler that demonstrates enterprise-level distributed systems engineering. 

### Key Engineering Highlights

- Clean Architecture: Domain-driven design with clear separation of concerns
- Extensible Storage Layer: Abstract IJobStorage - interface for pluggable persistence
- Concurrent Processing: Thread-safe workers with configurable concurrency
- Retry Logic: Exponential backoff with dead letter queue handling
- Lease-Based Execution: Prevents duplicate processing in distributed scenarios
- Type Safety: Compile-time job registration with reflection-based execution
- Observability: Comprehensive logging and monitoring APIs


### Architecture


Job Scheduling & Execution System
=================================

This diagram shows the architecture of the job system and how different components interact.

            ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
            │   Job Client    │    │  Job Scheduler  │    │   Job Worker    │
            │   (Public API)  │    │  (Delayed/Cron) │    │  (Execution)    │
            └─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
                      │                      │                      │
                      ▼                      ▼                      ▼
                ┌─────────────────────────────────────────────────────────┐
                │                     IJobManager                         │
                │              (Extensibility Layer)                      │
                └─────────────────────────────────────────────────────────┘
                      │                                              │
                      ▼                                              ▼
             ┌─────────────────┐                           ┌─────────────────┐
             │ InMemoryStorage │                           │ SqlServerStorage│
             │       (V1)      │                           │       (V2)      │
             └─────────────────┘                           └─────────────────┘



- Job Client: Submits jobs through the public API.
- Job Scheduler: Handles delayed or cron-based job scheduling.
- Job Worker: Picks up jobs and executes them.
- IJobStorage: Abstraction layer for storing jobs, allows swapping implementations.
- InMemoryStorage: Simple in-memory storage 
- SqlServerStorage: Persistent storage using SQL Server (production-ready).
