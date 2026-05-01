name: revit-addin
description: 'Template for Revit Addin'

# Agent Skills — Revit Add-in (.NET) với Vertical Slice, CQRS, WPF MVVM, DI, MediatR

## 1) Mục tiêu của agent
Agent này có nhiệm vụ hướng dẫn và sinh mã cho các Revit Add-in theo các ràng buộc sau:

- Ngôn ngữ: C# / .NET
- UI: WPF
- Pattern UI: MVVM
- Không dùng CommunityToolkit.Mvvm
- Kiến trúc: Vertical Slice Architecture + CQRS
- DI container: Microsoft.Extensions.DependencyInjection 8.0.1
- Mediator: MediatR 12.4.1
- Mỗi feature mới phải có tối thiểu:
  - `FeatureView.xaml`
  - `FeatureView.xaml.cs`
  - `FeatureViewModel.cs`
  - `FeatureRequest.cs`

---

## 2) Nguyên tắc bắt buộc

### 2.1 Kiến trúc
- Mỗi tính năng là một vertical slice độc lập.
- Không gom toàn bộ logic vào các thư mục dùng chung kiểu god-folder.
- Mỗi feature tự chứa:
  - View
  - ViewModel
  - Request
  - Handler
  - Result (nếu cần)
  - Validator / Mapper (nếu cần)

### 2.2 CQRS
- Command: thao tác ghi / thay đổi dữ liệu trong Revit.
- Query: chỉ đọc dữ liệu, không side effect.
- `FeatureRequest` là entry point cho MediatR.
- Không đưa business logic nặng vào ViewModel.
- ViewModel chỉ điều phối UI và gọi `IMediator.Send(...)`.

### 2.3 MVVM
- View không chứa business logic.
- ViewModel triển khai `BaseViewModel` thủ công hoặc qua base class tự viết.
- Command dùng `ICommand` tự cài đặt (`RelayCommand`, `AsyncRelayCommand` riêng).
- Không dùng CommunityToolkit.

### 2.4 Revit API
- Mọi thao tác sửa đổi `Document` phải nằm trong `Transaction`.
- Không gọi Revit API từ background thread.
- Nếu UI là modeless, cân nhắc `ExternalEvent`.
- Luôn kiểm tra `UIApplication`, `UIDocument`, `Document` trước khi xử lý.

---

## 3) Package bắt buộc

```powershell
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.1
dotnet add package MediatR --version 12.4.1
