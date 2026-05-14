# Revit Feature Rules (Production)

## 🔥 CORE RULE

Each feature MUST:
- Have its own Service implementing IExternalEventHandler
- Own its own ExternalEvent
- Call Revit API ONLY inside Execute()

---

## ❌ FORBIDDEN

- No RevitDispatcher
- No CQRS / MediatR
- No Revit API in ViewModel
- No async/await

---

## 🧱 Architecture

Feature contains:
- Service (Revit logic)
- ViewModel (UI interaction)
- Window (WPF)
- Command (open UI)

---

## 💉 DI Rules

- Service = Singleton
- ViewModel = Transient
- Window = Transient

---

## 🎯 Generation Requirements

- Beam Section → must create ViewSection
- Column → must create Structural Column
- Must include Transaction
- Must include user selection
- Must handle cancel safely (try/catch)