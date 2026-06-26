# 📦 SubLog — 구독 서비스 관리 앱

> **WPF + MVVM + EF Core** 기반의 구독 서비스 통합 관리 데스크탑 애플리케이션

![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=flat-square&logo=windows)
![Framework](https://img.shields.io/badge/.NET_8-WPF-512BD4?style=flat-square&logo=dotnet)
![Pattern](https://img.shields.io/badge/Pattern-MVVM-1D9E75?style=flat-square)
![DB](https://img.shields.io/badge/DB-SQLite-003B57?style=flat-square&logo=sqlite)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## 📌 프로젝트 개요

SubLog는 넷플릭스, 유튜브 프리미엄, Adobe 등 구독형 서비스를 한 곳에서 관리할 수 있는 **Windows 데스크탑 앱**입니다.

월별 지출 현황 시각화, 결제일 알림, 카테고리별 분류 등의 기능을 통해 구독 서비스의 무분별한 지출을 방지하고 체계적으로 관리할 수 있습니다.

---

## ✨ 주요 기능

| 기능 | 설명 |
|------|------|
| 📊 **대시보드** | 월별 총 지출, 카테고리 비율 도넛 차트, 최근 결제 목록 |
| 📋 **구독 목록** | 전체 구독 서비스 조회 / 필터 / 정렬 |
| 🎯 **구독 카탈로그** | 넷플릭스 · 유튜브 · Adobe 등 인기 서비스 원클릭 빠른 추가 |
| ➕ **구독 추가/수정** | 서비스명, 금액, 결제 주기, 결제일, 카테고리 등록 |
| 🗂️ **카테고리 관리** | 커스텀 카테고리 생성 / 수정 / 삭제 |
| 🔔 **결제일 알림** | N일 전 알림 배지 표시 |
| 📈 **통계 분석** | 월별 지출 추이 막대 차트 |
| ⚙️ **설정** | 다크/라이트 테마, 통화 단위 변경 |
| 💱 **환율 자동 변환** | 달러 구독(Netflix · Adobe 등) 실시간 원화 환산 — 한국수출입은행 Open API |

---

## 🛠️ 기술 스택

### 핵심 기술

| 분류 | 기술 |
|------|------|
| **언어** | C# 12 |
| **UI 프레임워크** | WPF (.NET 8) |
| **아키텍처 패턴** | MVVM (Model-View-ViewModel) |
| **데이터베이스** | SQLite (EF Core 8.0 Code-First) |
| **외부 API** | 한국수출입은행 Open API (실시간 USD → KRW 환율) |
| **HTTP / JSON** | System.Net.HttpClient · System.Text.Json (.NET 8 내장) |

### NuGet 패키지

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.0 | SQLite DB 연동 |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.0 | Code-First Migrations |
| `CommunityToolkit.Mvvm` | 8.2.2 | RelayCommand, ObservableProperty |
| `Microsoft.Xaml.Behaviors.Wpf` | 최신 | 이벤트 → Command 바인딩 |
| `LiveChartsCore.SkiaSharpView.WPF` | 최신 | 도넛 / 막대 차트 |

---

## 🏗️ 아키텍처

### MVVM 레이어 구조

```
SubLog/
├── 📁 View/                        # XAML 화면 (UI 전용, 코드 없음)
│   ├── MainWindow.xaml
│   ├── DashboardView.xaml
│   ├── SubscriptionListView.xaml
│   ├── AddEditSubscriptionDialog.xaml
│   ├── CatalogDialog.xaml          # 구독 카탈로그 선택 팝업
│   ├── CategoryManagementView.xaml
│   └── SettingsView.xaml
│
├── 📁 ViewModel/                   # 비즈니스 로직 (View ↔ Model 중재)
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── SubscriptionListViewModel.cs
│   ├── AddEditSubscriptionViewModel.cs
│   ├── CatalogViewModel.cs
│   └── SettingsViewModel.cs
│
├── 📁 Model/                       # 데이터 구조 (DB 테이블과 1:1 매핑)
│   ├── Subscription.cs
│   ├── Category.cs
│   ├── BillingCycle.cs             # enum: Monthly / Yearly / Weekly
│   └── CatalogItem.cs              # 카탈로그 프리셋 정적 데이터
│
├── 📁 Data/                        # EF Core 설정
│   ├── SubLogDbContext.cs
│   └── Migrations/
│
├── 📁 Repository/                  # DB 접근 계층 (CRUD 추상화)
│   ├── ISubscriptionRepository.cs
│   ├── SubscriptionRepository.cs
│   ├── ICategoryRepository.cs
│   └── CategoryRepository.cs
│
├── 📁 Services/                    # 외부 서비스 연동
│   └── ExchangeRateService.cs      # 한국수출입은행 환율 API
│
└── App.xaml                        # 앱 진입점 (StartupUri → View/MainWindow.xaml)
```

### 데이터 흐름

```
[사용자 클릭]
     ↓
[View (XAML)]  ──── Data Binding (INotifyPropertyChanged) ────▶  [View 자동 갱신 ✨]
     ↓                                                                  ▲
[ViewModel (C#)]  ─── RelayCommand 처리 / 유효성 검사                   │
     ↓                                                                  │
[Repository (C#)]  ─── CRUD 인터페이스 (ISubscriptionRepository)        │
     ↓                                                                  │
[EF Core]  ─── SQL 자동 생성 (INSERT / UPDATE / DELETE / SELECT)        │
     ↓                                                                  │
[SQLite DB]  ─── sublog.db 파일에 영구 저장  ───────────────────────────┘
```

---

## 🚀 설치 및 실행

### 사전 요구 사항

- Windows 10 이상
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (WPF 개발 워크로드 포함)

### 실행 방법

```bash
# 1. 저장소 클론
git clone https://github.com/[your-username]/SubLog.git

# 2. 솔루션 열기
# Visual Studio 2022에서 SubLog.sln 파일 열기

# 3. NuGet 패키지 복원
# 도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔

# 4. DB 마이그레이션 적용 (최초 1회)
Update-Database

# 5. 실행 (F5 또는 Ctrl+F5)
```

> 💡 **참고:** EF Core Code-First 방식을 사용하므로, 처음 실행 시 `sublog.db` 파일이 자동으로 생성됩니다. 별도 DB 설치가 필요 없습니다.

---

## 📸 스크린샷

| 대시보드 | 구독 목록 |
|:-------:|:--------:|
| *개발 중* | *개발 중* |

| 구독 추가/수정 | 통계 분석 |
|:----------:|:--------:|
| *개발 중* | *개발 중* |

---

## 📅 개발 로드맵

- [x] **EPIC 1** — 프로젝트 기반 구축 (MVVM 폴더 구조 ✅, EF Core✅, Repository 패턴✅) ✅
- [x] **EPIC 2** — 핵심 화면 개발 (Dashboard ✅ · SubscriptionList ✅ · AddEdit Dialog · 카탈로그 ✅) ✅
- [ ] **EPIC 3** — 고급 기능 (카테고리 관리 ✅, 다크/라이트 모드 테마 적용 ✅, 결제일 알림 ✅, 통계 분석✅, 환율 API⏳)🔁
- [ ] **EPIC 4** — 완성도 향상 + 포트폴리오 배포 (MSIX 패키징⏳, GIF 데모⏳)⏳

---

## 📚 학습 내용 (과목 연계)

본 프로젝트는 **C# 윈도우 프로그래밍 강의** (Ch17~Ch22) 내용을 실무 수준으로 적용한 포트폴리오 프로젝트입니다.

| 챕터 | 주제 | 프로젝트 적용 |
|------|------|--------------|
| Ch17 | 윈도우 프로그래밍 기초 | WPF 프로젝트 구조, App.xaml 이해 |
| Ch18 | 레이아웃 컨트롤 | Grid, StackPanel, DockPanel 활용 |
| Ch19 | WPF 컨트롤 | Button, TextBox, ComboBox, DataGrid |
| Ch20 | 고급 컨트롤과 다이얼로그 | Modal Dialog, MessageBox |
| Ch21 | 리소스와 데이터 바인딩 | INotifyPropertyChanged, {Binding} |
| Ch22 | MVVM 패턴 | ViewModel, RelayCommand, DataContext |

---

## 🤝 기여

이 프로젝트는 학습 및 포트폴리오 목적의 개인 프로젝트입니다.  
피드백이나 제안은 Issues 탭에 남겨주세요.

---

## 📝 라이선스

MIT License — 자유롭게 사용, 수정, 배포 가능합니다.

---

<div align="center">

Made with ❤️ as a portfolio project

**C# · WPF · MVVM · EF Core · SQLite · LiveCharts2**

</div>
