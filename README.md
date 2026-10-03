# SProcWrapper

## Описание проекта
SProcWrapper - это .NET-библиотека для упрощения работы с хранимыми процедурами базы данных Firebird. Она избавляет от написания рутинного кода для вызова хранимых процедур через ADO.NET или Dapper. Разработчик объявляет C#-интерфейсы, и вызовы методов этих интерфейсов автоматически транслируются в SQL-запросы к БД с помощью динамических прокси (Castle.Core) и маппера Dapper.

## Основные возможности и сценарии использования
* Декларативное описание хранимых процедур: маппинг C#-интерфейсов на хранимые процедуры Firebird с помощью атрибутов `[SProcService]`, `[SProcCall]`, `[SProcParam]`.
* Автоматическое определение типа вызова:
  * Если метод возвращает `IEnumerable<T>`, генерируется запрос `SELECT * FROM Procedure(...)` (Selectable процедура).
  * Если тип скалярный (примитивы, `string`), генерируется `EXECUTE PROCEDURE Procedure(...)` с получением скалярного результата.
  * Если метод возвращает объект или `void`, генерируется `EXECUTE PROCEDURE`, а результат маппится через выходные параметры.
* Выборочный маппинг колонок: поддержка `SelectModeEnum.ByColumnName`, где запрашиваются только нужные колонки на основе свойств объекта (поддерживается атрибут `[Column]`).
* Обработка денежных типов: атрибут `[MoneyAttribute]` автоматически трансформирует выборку поля, деля его на 100.00 (`ColumnName/100.00 as ColumnName`).
* Контекст сессии: автоматическая передача идентификатора сессии `aSessID` для методов (включается через `WithSessionId = true` в `[SProcCall]`).
* Кастомная сериализация JSON: включает утилитарные резолверы для Newtonsoft.Json (сериализация приватных полей `IncludePrivateStateContractResolver` и только доступных для записи свойств `WritablePropertiesOnlyResolver`).

## Архитектура и ключевые компоненты
* `SProcProxy` и `SProcAttributesHandler`: ядро библиотеки. Использует `Castle.DynamicProxy` для перехвата вызовов интерфейса. Анализирует методы и атрибуты, создавая словарь маппинга.
* `StoredProcedure`: описывает параметры выполнения процедуры (таймауты, выборка колонок, буферизация). Генерирует итоговый SQL-запрос и параметры (на базе `DynamicParameters` Dapper).
* `DataContext` и `IDataContext`: обертка над подключением к БД. Жестко завязана на Firebird (использует `FbConnection` и специфические параметры транзакций, такие как `FbTransactionBehavior.NoWait`). Инкапсулирует вызовы к `SqlMapper` Dapper.
* `AbstractSProcService<TInterface>`: базовый класс для сервисов-потребителей, автоматически инициализирующий прокси-объект при внедрении `IDataContext`.

## Структура репозитория
* `src/SProcWrapper/` - основной проект библиотеки (Target: `net461`).
  * `Data/` - классы для работы с подключением (`DataContext`) и транзакциями БД.
  * `Proxy/` - логика генерации динамического прокси и анализа атрибутов.
  * `Extensions/` - методы расширения объектов и строк.
  * `Serialization/` - резолверы для `Newtonsoft.Json`.
* `src/SProcWrapper.Tests/` - проект с Unit-тестами (NUnit, Moq).

## Основные зависимости и технологии
* Target Framework: `.NET Framework 4.6.1` (`net461`)
* Castle.Core (4.2.1) - генерация динамических прокси-классов
* Dapper (1.50.5) - микро-ORM для маппинга данных из БД
* FirebirdSql.Data.FirebirdClient (5.12.1) - ADO.NET провайдер для СУБД Firebird
* Newtonsoft.Json (13.0.1) - JSON сериализация

## Требования для локального запуска
* .NET SDK (с поддержкой `net461` / .NET Framework 4.6.1).
* СУБД Firebird (опционально, для запуска в интеграционном окружении, тесты изолированы через Moq).
* Переменные окружения не требуются. Конфигурация БД передается в `DataContext` через строку подключения.

## Как собрать проект и запустить тесты
Проект собирается стандартными средствами .NET CLI.
```bash
# Сборка решения
dotnet build SProcWrapper.sln

# Запуск тестов
dotnet test src/SProcWrapper.Tests/SProcWrapper.Tests.csproj
```

## Типичный workflow разработки
1. Создайте интерфейс, описывающий контракт с хранимыми процедурами.
2. Добавьте атрибут `[SProcService]`. Опционально укажите `Namespace`, который станет префиксом для имен всех процедур.
3. Опишите методы, помечая их атрибутом `[SProcCall("NameInDb")]`.
4. Пометьте аргументы методов атрибутом `[SProcParam]`. Параметры без этого атрибута игнорируются при вызове.
5. Создайте класс сервиса, унаследованный от `AbstractSProcService<TInterface>`, либо используйте `SProcProxy.Build<TInterface>(dataContext)` для получения прокси.

**Пример:**
```csharp
[SProcService(Namespace = "ERP")]
public interface IUserService
{
    // Вызовет: SELECT * FROM ERP_GetUser (@id)
    [SProcCall("GetUser", SelectMode = SelectModeEnum.All)]
    IEnumerable<UserDto> GetUsers([SProcParam] int id);
}

public class UserService : AbstractSProcService<IUserService>
{
    public UserService(IDataContext context) : base(context) { }

    public IEnumerable<UserDto> GetUsers(int id) => Sproc.GetUsers(id);
}
```

## Важные архитектурные и эксплуатационные особенности
* Привязка к Firebird: библиотека не является универсальной абстракцией SQL. В `DataContext.cs` явно используется приведение к `FbConnection` для управления транзакциями (задействуются опции вроде `FbTransactionBehavior.NoWait`).
* Имена параметров БД: хотя атрибут `[SProcParam]` имеет свойство `Name`, фактическое формирование SQL-параметров в `StoredProcedureParameter` его игнорирует. Имя параметра в SQL-запросе всегда совпадает с именем аргумента в C#-методе.
* Игнорирование неразмеченных аргументов: если аргумент C#-метода не помечен атрибутом `[SProcParam]`, он молча пропускается при формировании списка параметров для хранимой процедуры.
* Обработка MoneyAttribute: если свойство DTO помечено `[MoneyAttribute]`, библиотека (при `SelectMode = SelectModeEnum.ByColumnName`) генерирует SQL `ИмяПоля/100.00 as ИмяПоля`. Это предполагает, что БД хранит деньги в минимальных целых единицах (копейки/центы).
