---
name: revit-addin
description: 'Rule to write new feature for revit addin.'
---

# Revit MVVM Vertical Slice (Service = ExternalEventHandler)

## 🔥 CRITICAL RULE

Each Service MUST:
- Implement IExternalEventHandler
- Own its own ExternalEvent
- Call Revit API ONLY inside Execute()

---

## ❌ FORBIDDEN

- Do NOT use RevitDispatcher
- Do NOT use CQRS / MediatR
- Do NOT call Revit API in ViewModel
- Do NOT use async/await with Revit API

---

## 🧱 Architecture

Each feature contains:
- Service (ExternalEventHandler)
- ViewModel
- Window (WPF)
- Command (open UI)

---

## 💉 DI Rules

- Service MUST be Singleton (because it owns ExternalEvent)
- ViewModel = Transient
- Window = Transient

---

## 🧠 ViewModel Rules

- Call service.Raise()
- No Revit API
- No Transaction

---

## ⚙️ Service Rules

- Implement IExternalEventHandler
- Create ExternalEvent in constructor
- Expose Raise() method
- Handle Transaction inside Execute()

---

## 🎯 Naming

Feature: CreateBeamSection  
Service: CreateBeamSectionService  
ViewModel: CreateBeamSectionViewModel  
Command: OpenCreateBeamSectionCommand  

---

## 📦 Folder Structure

Features/
  FeatureName/
    FeatureNameService.cs
    FeatureNameViewModel.cs
    FeatureNameWindow.xaml
    FeatureNameWindow.xaml.cs

Commands/
  OpenFeatureNameCommand.cs

---

## 🚀 Generation Requirements

When generating a feature:

1. Service implements IExternalEventHandler
2. Service contains ExternalEvent
3. ViewModel calls service.Raise()
4. Window uses constructor injection
5. Command resolves Window via DI