# Плагин «Опора с пазом» для КОМПАС-3D

Курсовая работа по дисциплине «Основы разработки САПР» (ТУСУР, кафедра КСУП).

Программа на C# (WinForms, .NET Framework 4.7.2), которая через COM-интерфейс
Kompas6API5 строит в КОМПАС-3D 3D-модель опоры с пазом по заданным параметрам.

## Параметры модели

| Параметр | Диапазон, мм | Имя в коде | Зависимость |
|---|---|---|---|
| L — длина опоры | 60–150 | `SupportLength` | — |
| W — ширина опоры | 50–100 | `SupportWidth` | — |
| H — высота опоры | 100–300 | `SupportHeight` | — |
| d — длина паза | 20–100 | `GrooveLength` | d ≤ L − 2·M₁ (M₁ = 10) |
| h — высота паза | 20–80 | `GrooveHeight` | h ≤ H − 2·M₂ (M₂ = 20) |
| p — глубина паза | 20–80 | `GrooveDepth` | p ≤ W − M₃ (M₃ = 10) |

Дополнительная функциональность: **сквозной паз** (флажок в интерфейсе) — паз
вырезается на всю ширину W, проверка глубины отключается.

## Валидация

Все диапазоны и зависимости параметров собраны в `SupportParameters.Validate()`:

- диапазоны — `SupportLengthRange`, `SupportWidthRange`, `SupportHeightRange`,
  `GrooveLengthRange`, `GrooveHeightRange`, `GrooveDepthRange`;
- отступы — константы `GrooveEndMargin` (M₁), `GrooveVerticalMargin` (M₂),
  `GrooveBackMargin` (M₃).

`Validate()` возвращает `IReadOnlyList<ValidationError>`. Каждая ошибка содержит
идентификатор параметра `ValidationError.Parameter` и готовый текст сообщения.
`SupportParametersControl` хранит словарь `Dictionary<SupportParameter, NumericUpDown>`
и по идентификатору находит нужное поле: подсвечивает его и показывает текст
ошибки в ToolTip. Строковые сравнения имён полей не используются.

## Сборка

Решение: `Code/SupportWithSlot.sln` (Visual Studio 2022+, .NET Framework 4.7.2).
Требуется установленный КОМПАС-3D (COM-сервер `KOMPAS.Application.5`).
Сборка: Visual Studio или MSBuild (`dotnet build` не поддерживает ResolveComReference).

## Тесты

Модульные тесты бизнес-логики (xUnit): проект `Code/SupportTests`.
Запуск: Visual Studio Test Explorer или `vstest.console.exe SupportTests.dll`.

Нагрузочное тестирование: консольный проект `Code/StressTesting` — строит серию
моделей, пишет в `log.txt` время построения и потребление ОЗУ/ЦП.

## Обработка ошибок

Все ошибки построения (в том числе ошибки API КОМПАС-3D) представляются единым
типом `BuildException`. `MainForm` перехватывает его и показывает одно и то же
стандартное окно ошибки WinForms. Отдельного типа «ошибки API» нет.

## Структура репозитория

- `Code/SupportApp` — приложение: `SupportParameters`, `SupportParametersControl`,
  `SupportBuilder`, `Kompas3DWrapper`, `KompasLauncher`, `MainForm`;
- `Code/SupportTests` — модульные тесты;
- `Code/StressTesting` — нагрузочный тест;
- `docs/` — техническое задание и проект системы (docx + pdf), диаграммы и макеты.
