# Procyon NuGet Packages

A modular collection of reusable .NET libraries designed for building scalable, production-ready systems.

## 📌 Overview

This repository contains all official **Procyon NuGet packages**, built with a focus on:

- **Modularity** — each concern is isolated into its own package
- **Extensibility** — plug-and-play providers (S3, future storage backends, etc.)
- **Framework alignment** — built on top of standard .NET abstractions
- **Minimal assumptions** — works across any ASP.NET Core or .NET project

---

## 📦 Packages

### Core Media Stack

| Package                      | Description                    |
| ---------------------------- | ------------------------------ |
| `Procyon.Media.Abstractions` | Core interfaces and contracts  |
| `Procyon.Media`              | Core media service logic       |
| `Procyon.Media.S3`           | AWS S3 provider implementation |

---

## 🏗️ Architecture

```text
Procyon.Media.Abstractions
        ↑
Procyon.Media (Core)
        ↑
Procyon.Media.S3 (Provider)
```

### Design Principles

- **Abstractions-first**
- **Provider-based architecture**
- **Dependency injection friendly**
- **Config-driven but not config-dependent**

---

## 🚀 Getting Started

Each package can be used independently.

Example:

```bash
dotnet add package Procyon.Media
dotnet add package Procyon.Media.S3
```

Then in your app:

```csharp
builder.Services.AddProcyonMedia(options =>
{
    options.EnableHashing = true;
});
```

---

## ⚙️ Configuration

Supports both:

### `appsettings.json`

```json
{
  "Procyon": {
    "Media": {
      "EnableHashing": true
    }
  }
}
```

### Environment Variables

```env
Procyon__Media__EnableHashing=true
```

---

## 🧪 Example Project

See:

```text
examples/Procyon.Example
```

This demonstrates:

- File upload
- S3 integration
- API usage
- Config setup

---

## 🧱 Repository Structure

```text
src/
  Procyon.Media.Abstractions/
  Procyon.Media/
  Procyon.Media.S3/

examples/
  Procyon.Example/

tests/
  Procyon.Media.Tests/
```

---

## 🧠 Philosophy

Procyon libraries aim to:

- reduce repeated infrastructure code
- standardize patterns across projects
- remain lightweight and composable

---

## 🚧 Roadmap

- Signed URL uploads
- Background processing hooks
- Image transformations
- Additional providers (Azure Blob, local disk)
- Optional EF persistence layer

---

## 🤝 Contributing

Contributions are welcome. Keep changes:

- modular
- well-scoped
- provider-friendly

---

## 📄 License

MIT (or your chosen license)

---

## 🔗 Repository

https://github.com/rudrprasad05/nuget
