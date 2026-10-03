# SProcWrapper

**SProcWrapper** is a .NET library (targeting `.NET Framework 4.6.1`) that simplifies executing Firebird stored procedures by allowing you to map them to standard C# interfaces. It uses `Castle.Core` for dynamic proxies and `Dapper` for object mapping, removing the need to write boilerplate ADO.NET code.

## Key Features

* **Declarative Mapping**: Map C# interfaces directly to Firebird stored procedures using `[SProcService]`, `[SProcCall]`, and `[SProcParam]` attributes.
* **Smart Execution**: Automatically generates `SELECT * FROM Procedure(...)` for `IEnumerable<T>` return types, and `EXECUTE PROCEDURE Procedure(...)` for scalar or single-row objects.
* **Custom Columns & Types**: Supports mapping specific columns (`SelectModeEnum.ByColumnName`) and automatically handles monetary values via `[MoneyAttribute]`.
* **Session Context**: Automatically injects a session ID parameter (`aSessID`) when configured.

## Usage Example

Define your contract:

```csharp
[SProcService(Namespace = "ERP")] // Automatically prepends "ERP_" to all procedure names
public interface IUserService
{
    // Executes: SELECT * FROM ERP_GetUser (@id)
    [SProcCall("GetUser", SelectMode = SelectModeEnum.All)]
    IEnumerable<UserDto> GetUsers([SProcParam] int id);
}
```

Implement the service by inheriting from `AbstractSProcService`:

```csharp
public class UserService : AbstractSProcService<IUserService>
{
    // The base class automatically generates the proxy using the provided IDataContext
    public UserService(IDataContext context) : base(context) { }

    public IEnumerable<UserDto> GetUsers(int id) => Sproc.GetUsers(id);
}
```

## Important Notes

* **Attribute Requirement**: Every C# method argument must be decorated with `[SProcParam]`. Arguments missing this attribute will throw a configuration exception.
* **Firebird Coupling**: The underlying `DataContext` relies specifically on `FirebirdSql.Data.FirebirdClient` and its transaction behaviors.
* **Parameter Naming**: The SQL parameter name is determined by the `Name` property of `[SProcParam]`. If omitted, it falls back to the C# argument name.

## Building and Testing

You will need the .NET SDK supporting `net461`.

```bash
# Build the solution
dotnet build SProcWrapper.sln

# Run unit tests
dotnet test src/SProcWrapper.Tests/SProcWrapper.Tests.csproj
```
