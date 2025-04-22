# Bucephalus MVCS Framework

**Bucephalus** — это легковесный и масштабируемый фреймворк для создания UI в Unity на основе архитектурного паттерна **MVCS: Model - View - Controller - ServicesMediator**. Он помогает разделять логику, упрощает сопровождение и масштабирование пользовательского интерфейса.


## 🔧 Что такое MVCS?
**MVCS** расширяет классическую модель MVC, добавляя слой **Service Mediator**:

- **Model (Модель)** — содержит данные и бизнес-логику.
- **View (Представление)** — отображает интерфейс и реагирует на действия пользователя.
- **Controller (Контроллер)** — обрабатывает действия пользователя, обновляет модель и представление.
- **ServicesMediator** — предоставляет зависимости и вспомогательные сервисы контроллеру.

В Bucephalus этот паттерн используется **исключительно для UI**.


## 🚀 Возможности
- Поддержка Addressables для асинхронной загрузки префабов  
- Асинхронные жизненные циклы через UniTask  
- Enum-основанная регистрация View с генерацией кода  
- Автоматическое связывание зависимостей через атрибуты  
- Система очистки (dispose) для всех MVCS компонентов  
- Минимализм и высокая производительность


## ⚠️ Важно: UniRx не устанавливается автоматически!

> Хотя `UniTask` и `Addressables` **подтягиваются автоматически** через `package.json`, **`UniRx` — нет**. Из-за ограничений Unity Package Manager его нужно установить вручную.

**Как установить UniRx:**
- Скачать с GitHub: [https://github.com/neuecc/UniRx](https://github.com/neuecc/UniRx)
- Или импортировать `.unitypackage` в проект вручную


## 📦 Установка

### 🔸 Способ 1: через Package Manager (GUI)
1. Открой `Window > Package Manager`
2. Нажми **+ > Add package from Git URL...**
3. Вставь: `https://github.com/Inheo/Bucephalus.git`
4. Установи UniRx вручную (см. выше)

### 🔸 Способ 2: через `manifest.json`
```json
"dependencies": {
"com.anastoi.mvcs": "https://github.com/Inheo/Bucephalus.git",
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git",
"com.unity.addressables": "1.19.19"
}
```

### 🔸 Способ 3: вручную
Вы также можете клонировать проект напрямую из репозитория GitHub:
```
https://github.com/Inheo/Bucephalus.git
```
И помести содержимое `Bucephalus/` в `Assets/` или `Packages/`, в зависимости от предпочтений.


## 🧱 Структура проекта
```
Bucephalus/
├── MVCS/                  # Базовые классы: BaseView, BaseController, BaseModel, BaseServicesMediator
├── Attributes/            # Атрибуты: ViewAttribute, ControllerAttribute, ViewIdsAttribute
├── Preprocessor/          # Генерация ViewId (AsmFinder, ViewIdCodeGenerator, Generated.cs)
├── Editor/                # Генераторы кода на этапе редактора
├── Enums/                 # Перечисления, такие как ViewType
```


## 🧩 Как пользоваться
1. Создайте префабы UI
   - Создайте нужные UI префабы
   - Добавьте их в Addressables
   - Название префаба должно совпадать с именем в enum (без `.prefab`).

Например, если ваш префаб называется `Settings.prefab`, он должен быть зарегистрирован в enum как:
```
[ViewIds]
public enum MyViews {
    MainMenu,
    Settings,
    Popup
}
```

2. Создайте View и Controller
   - Наследуйтесь от `BaseView`, `BaseController<T>`, `BaseModel`, `BaseServicesMediator`
   - Добавь аттрибут для наследника `BaseView` и укажи его контроллер `[View(typeof(MyController))]`
   - Добавь аттрибут для наследника `BaseController` и укажи его модель и сервис медиатор [Controller(typeof(MyModel), typeof(MyServicesMediator))]

3. Инициализируйте через `Director`
```
Director.ShowView("MainMenu");
```


## 🧠 Свойства View
В `BaseView` доступны:
```
public virtual ViewType Type { get; protected set; } = ViewType.Dynamic;
public virtual ushort SortingOrder { get; protected set; } = 0;
public virtual byte Priority { get; protected set; } = 0;
public virtual bool IsModal { get; protected set; } = false;
```
- **Type:** `Dynamic` или `Static`  — влияет на логику создания/удаления

- **SortingOrder:** порядок отображения UI

- **Priority:** приоритет отображения (например, при очереди окон)

- **IsModal:** если true, окно блокирует взаимодействие с другими элементами UI


## ⚙️ Генерация кода

Перед билдом или вручную через редактор вызывается ViewIdCodeGenerator, который:

- ищет все enum с аттрибутом `[ViewIds]`

- создает файл `Generated.cs` с маппингом имен префабов
  
Это исключает рефлексию во время выполнения и ускоряет запуск.


## ✅ Рекомендации

- View должны быть "тупыми": только визуал и события

- Вся логика — в контроллере и сервисах

- Состояние — в модели

- Используй `Dispose()` для очистки

- Addressables: загружай и выгружай ассеты корректно


## 📌 Лицензия

MIT — свободно для коммерческого и личного использования.
