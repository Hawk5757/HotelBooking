# Hotel Booking System API

---

## 🛠️ Architectural Highlights & Compliance

This solution demonstrates strict adherence to production-grade engineering standards and explicitly satisfies the following requirements:

### 1. Domain-Driven Design & Rich Domain Model (SRP)
* Shifted from an Anemic Domain Model to a **Rich Domain Model**. Business and financial calculations are encapsulated exactly where they belong—inside the `RatePlan` record (e.g., `CalculateTotalPrice`, `GetCancellationPolicy`).
* Class and service layers comply fully with the **Single Responsibility Principle (SRP)**. The `BookingSearchService` acts strictly as an orchestrator, while parameter validation and contract mapping are cleanly decoupled into standalone layers.

### 2. Strong Typing & Precise Data Representations
* **Financial Integrity:** All pricing fields (`PricePerNight`, `TotalPrice`) utilize the 128-bit `decimal` type to guarantee absolute mathematical precision and completely eliminate binary floating-point rounding errors.
* **Distributed Keys:** Identity markers (`HotelId`, `RoomId`, `RatePlanId`) utilize `Guid` structurally, preventing sequential enumeration risks and aligning with cloud-native microservice standards.
* **Timezone Safety:** Dynamic cancellations utilize `DateTimeOffset` to accurately map hard cutoff points globally across varying regional server nodes and user timezones.

### 3. Null Safety & Record Immutability
* Built entirely with the explicit **`<Nullable>enable</Nullable>`** context. Complete eradication of standard `NullReferenceException` vectors via structural Null Suppression patterns and fallback default object instantiations.
* Utilizes modern C# positional **`record`** variants for all DTOs and Contracts, ensuring structural immutability, thread-safe memory allocations, and value-based equality checking optimized for unit testing.

### 4. Advanced Asynchrony & Cancellation Propagation
* Fully end-to-end asynchronous processing using non-blocking I/O (`async/await`).
* Strict propagation of **`CancellationToken`** starting from the HTTP request endpoint directly down through to the EF Core query execution layer to prevent wasting server resources on aborted client connections.

### 5. Resilient Infrastructure & Fault Tolerance
* Configured using **Polly V8** via a centralized `ResilientInitializer` registered as a `Singleton` for stateless resource management.
* Implements an **Exponential Backoff** retry strategy (`MaxRetryAttempts = 3`, initial `Delay = 2s`) to elegantly mitigate Transient Faults (such as cold database container warmups) without causing *Thundering Herd* system degradation.

## 🐳 Quick Start & Manual Testing

### 1. Run the Application
* **Via JetBrains Rider:** Select the `HotelBooking.Api` run configuration from the top Toolbar and click the **Run** button (`Shift + F10`).
* **Via .NET CLI:** Execute the following command in your terminal:
  ```bash
  dotnet run --project HotelBooking.Api
  
### 2. Retrieve Generated IDs
A temporary diagnostic helper endpoint has been exposed to easily extract the stable deterministic Bogus seed values:
* **Endpoint:** `GET https://localhost:7009/api/test-hotels`

### 3. Test Availability (Postman / cURL)
Replace `HotelId` with a GUID retrieved from the previous step.


---

## 🧪 Unit Testing Validation

The test suite validates past/future dates, overflow duration constraints, polymorphic cancellation rules, and operation abort propagation.

### Running via JetBrains Rider (Recommended)
1. Open the `Unit Tests` tool window (`Ctrl + Alt + U` / `Cmd + Alt + U` on macOS).
2. Right-click on the `HotelBooking.Tests` project and select **Run Unit Tests** (`Ctrl + U, R`).
3. You can also view code coverage or run individual test cases directly from the gutter icons next to the test methods.

### Running via .NET CLI
```bash
dotnet test
