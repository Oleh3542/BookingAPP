BookingAPP — оренда конференц-залів

Веб-застосунок для управління конференц-залами: пошук доступних залів, бронювання з автоматичним розрахунком вартості (залежно від часу та обраних послуг), перегляд історії бронювань

Проєкт складається з двох незалежних частин:

BookingAPP/
BookingAPP_Backend/    → ASP.NET Core 8 Web API (C#)
BookingAPP_Frontend/   → Angular / TypeScript SPA

Технології

Бекенд:

ASP.NET Core 8 Web API
Entity Framework Core + SQLite (з міграціями)
Swagger / OpenAPI (Swashbuckle) — автоматична документація API
API-key автентифікація для адміністративних операцій
Rate limiting, централізована обробка помилок, CORS

Фронтенд:

Angular (TypeScript), NgModule-based
Reactive forms, HttpClient

Швидкий старт
Бекенд

Вимоги: .NET 8 SDK

bash
cd BookingAPP_Backend
dotnet restore
dotnet run

Фронтенд

Вимоги: Node.js 18+, npm

bash
cd BookingAPP_Frontend
npm install
npm start