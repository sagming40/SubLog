# 📦 SubLog WORKFLOW — 사공민규

| 항목 | 내용 |
| --- | --- |
| 카테고리 | 포트폴리오 |
| 파일형태 | 문서 |
| 버전 | v1.1 |
| 생성일 | 2026년 6월 22일 |
| 수정일 | 2026년 6월 24일 |
| 담당자 | 사공민규 |
| 기술 스택 | C# · WPF (.NET 8) · MVVM · EF Core 8.0 · SQLite · LiveCharts2 · 한국수출입은행 API |

---

> 💡 **전체 흐름 한 줄 요약**
> 기반 구축 → 핵심 화면 개발 → 고급 기능 → 완성 & 포트폴리오

> 🗺️ **사용자 플로우**
> 앱 실행 → 대시보드 (지출 요약 + 차트) → 구독 목록 조회 → 구독 추가 / 수정 / 삭제 → 결제일 알림 확인 → 통계 분석

> 💼 **포트폴리오 포인트**
>
> - MVVM 패턴 (Ch22 교과서 내용 실무 수준 적용)
> - EF Core Code-First (SQL 없이 C# 클래스만으로 DB 설계)
> - Repository 패턴 (실무 레벨 아키텍처)
> - LiveCharts2 데이터 시각화 (도넛 차트 + 막대 차트)
> - CommunityToolkit.Mvvm (국내 SI / 솔루션 기업 실제 사용 패키지)
> - 외부 REST API 연동 + async/await 비동기 처리 + JSON 파싱 (환율 API → 실무 HTTP 통신 경험 어필)
> - 구독 카탈로그 프리셋 (사용자 불편 직접 발견 → UX 주도적 개선 → 면접 스토리텔링 포인트)

---

## 📁 폴더 구조

```
SubLog/
├── App.xaml                               # 앱 진입점 — StartupUri → View/MainWindow.xaml
├── App.xaml.cs
│
├── 📁 View/                               # XAML 화면 (UI 전용, 비즈니스 로직 없음)
│   ├── MainWindow.xaml                    # 메인 창 — 사이드바 + 콘텐츠 영역
│   ├── DashboardView.xaml                 # 대시보드 (차트 + 요약 카드)
│   ├── SubscriptionListView.xaml          # 구독 목록 (DataGrid + 필터)
│   ├── AddEditSubscriptionDialog.xaml     # 구독 추가/수정 팝업
│   ├── CatalogDialog.xaml                 # 구독 카탈로그 선택 팝업 🆕
│   ├── CategoryManagementView.xaml        # 카테고리 관리
│   └── SettingsView.xaml                  # 설정 (테마, 통화)
│
├── 📁 ViewModel/                          # 비즈니스 로직 (View ↔ Model 중재)
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── SubscriptionListViewModel.cs
│   ├── AddEditSubscriptionViewModel.cs
│   ├── CatalogViewModel.cs                # 카탈로그 팝업 ViewModel 🆕
│   ├── CategoryManagementViewModel.cs
│   └── SettingsViewModel.cs
│
├── 📁 Model/                              # 데이터 구조 (DB 테이블과 1:1 매핑)
│   ├── Subscription.cs                    # 구독 서비스 엔티티
│   ├── Category.cs                        # 카테고리 엔티티
│   ├── BillingCycle.cs                    # enum: Monthly / Yearly / Weekly
│   └── CatalogItem.cs                     # 카탈로그 프리셋 데이터 클래스 🆕
│
├── 📁 Data/                               # EF Core 설정
│   ├── SubLogDbContext.cs                 # DB 연결 + 테이블 정의
│   └── Migrations/                        # EF Core 자동 생성 마이그레이션
│
├── 📁 Repository/                         # DB 접근 계층 (CRUD 추상화)
│   ├── ISubscriptionRepository.cs         # 인터페이스 (계약)
│   ├── SubscriptionRepository.cs          # 실제 구현
│   ├── ICategoryRepository.cs
│   └── CategoryRepository.cs
│
└── 📁 Services/                           # 외부 서비스 연동 🆕
    └── ExchangeRateService.cs             # 한국수출입은행 환율 API (EPIC 3)
```

---

## ✅ EPIC 1. 프로젝트 기반 구축 — 완료

> 💡 **이 EPIC의 목표**
> 개발을 본격적으로 시작하기 위한 기반 다지기. EPIC 1이 완료되면 DB 연결과 데이터 저장이 가능한 뼈대가 완성됨.

### ✅ Task 1-1 · WPF 프로젝트 생성 (Visual Studio 2022)

> ✅ **WinForm 대신 WPF를 선택한 이유**
>
> - WinForm은 코드에서 직접 UI를 조작해야 함 (`label1.Text = "값"`)
> - WPF는 **Data Binding** 덕분에 데이터가 바뀌면 UI가 자동으로 따라옴
> - WPF는 XAML로 UI를 선언적으로 작성 → 디자인과 로직 분리 가능
> - MVVM 패턴을 완벽 지원 → 포트폴리오에서 차별화 가능

- [x]  Visual Studio 2022 실행 → `새 프로젝트 만들기`
- [x]  `WPF 애플리케이션` 선택 (Windows Forms 앱이 아닌 것 주의 ⚠️)
- [x]  프로젝트 이름: `SubLog` / 솔루션 이름: `SubLog`
- [x]  프레임워크: `.NET 8.0` 선택
- [x]  `Ctrl + F5` 로 빈 창 정상 실행 확인

### ✅ Task 1-2 · MVVM 폴더 구조 설정 + App.xaml 수정

> ✅ **이 폴더 구조를 선택한 이유**
>
> - 교수님 Ch22 예제의 `View / ViewModel / Model` 구조를 그대로 사용
> - 실무 기업에서도 동일한 구조 사용 → 코드 리뷰 시 바로 이해 가능
> - 화면(View)과 로직(ViewModel)이 분리되어 나중에 UI를 바꿔도 ViewModel 코드는 그대로 유지

```xml
<!-- App.xaml — StartupUri를 View 폴더로 수정 -->
<Application x:Class="SubLog.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="View/MainWindow.xaml">
</Application>
```

> ⚠️ **MainWindow.xaml 이동 후 App.xaml 수정 필수**
> 파일을 View/ 폴더로 이동하면 StartupUri 경로가 달라짐. 수정하지 않으면 앱이 실행되지 않음.

- [x]  솔루션 탐색기에서 폴더 5개 생성: `View` `ViewModel` `Model` `Data` `Repository`
- [x]  `MainWindow.xaml` + `MainWindow.xaml.cs` 를 `View/` 폴더로 이동
- [x]  `App.xaml` 의 `StartupUri` 를 `View/MainWindow.xaml` 로 변경
- [x]  `Ctrl + F5` 로 정상 실행 확인 (폴더 이동 후에도 에러 없어야 함)

### ✅ Task 1-3 · NuGet 패키지 5종 설치

> ✅ **CommunityToolkit.Mvvm을 추가로 사용하는 이유**
>
> - 교과서(Ch22)는 수동으로 `INotifyPropertyChanged` 를 구현
> - CommunityToolkit은 `[ObservableProperty]` 속성 하나로 자동 생성
> - 코드량이 절반으로 줄어들고 실수가 적어짐
> - 국내 SI / 솔루션 기업에서 실제로 사용하는 패키지

> ✅ **SQL Server 대신 SQLite를 선택한 이유**
>
> - SQL Server는 별도 설치 및 서버 실행이 필요함
> - SQLite는 `.db` 파일 하나로 동작 → 설치 없이 어느 PC에서나 실행 가능
> - 포트폴리오 제출 / 발표 시 별도 DB 설치 안내 불필요

```powershell
# 도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔에서 실행

Install-Package Microsoft.EntityFrameworkCore.Sqlite -Version 8.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.0
Install-Package CommunityToolkit.Mvvm -Version 8.2.2
Install-Package Microsoft.Xaml.Behaviors.Wpf
Install-Package LiveChartsCore.SkiaSharpView.WPF
```

> ⚠️ **CommunityToolkit.Mvvm 사용 시 주의사항**
> `[ObservableProperty]` 또는 `[RelayCommand]` 를 사용하는 클래스는 반드시 `partial` 키워드를 추가해야 함.
> 빠지면 "속성을 찾을 수 없습니다" 에러 발생.
> ```csharp
> // ❌ 잘못된 예
> public class DashboardViewModel : ObservableObject { }
>
> // ✅ 올바른 예
> public partial class DashboardViewModel : ObservableObject { }
> ```

- [x]  패키지 관리자 콘솔 열기 (도구 → NuGet 패키지 관리자 → 패키지 관리자 콘솔)
- [x]  위 5개 명령어 순서대로 실행
- [x]  솔루션 탐색기 → 종속성 → 패키지 에서 5개 확인
- [x]  `Ctrl + Shift + B` 빌드 → 에러 없음 확인

### ✅ Task 1-4 · Model 클래스 정의 (Subscription, Category, BillingCycle)

> 💡 **Model이란?**
> DB에 저장되는 데이터의 설계도. C# 클래스 하나 = DB 테이블 하나.
> EF Core가 이 클래스를 보고 테이블을 자동으로 만들어 줌.

```csharp
// Model/BillingCycle.cs — 결제 주기 enum
namespace SubLog.Model
{
    public enum BillingCycle
    {
        Weekly,    // 매주
        Monthly,   // 매월
        Yearly     // 매년
    }
}
```

```csharp
// Model/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SubLog.Model
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;   // 예: 영상, 음악, 업무

        public string ColorHex { get; set; } = "#4A90E2";  // UI 표시 색상

        public List<Subscription> Subscriptions { get; set; } = new();
    }
}
```

```csharp
// Model/Subscription.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace SubLog.Model
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;   // 예: Netflix, YouTube Premium

        public decimal Price { get; set; }                  // 결제 금액
        public BillingCycle BillingCycle { get; set; }      // 결제 주기
        public int BillingDay { get; set; }                 // 결제일 (1~31)
        public DateTime StartDate { get; set; }             // 구독 시작일
        public bool IsActive { get; set; } = true;          // 활성화 여부

        [MaxLength(500)]
        public string? Memo { get; set; }                   // 메모 (선택)

        // 카테고리 외래키 관계 (EF Core가 자동으로 JOIN 처리)
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
```

- [x]  `Model/BillingCycle.cs` 생성 및 코드 작성
- [x]  `Model/Category.cs` 생성 및 코드 작성
- [x]  `Model/Subscription.cs` 생성 및 코드 작성
- [x]  `Ctrl + Shift + B` 빌드 후 에러 없음 확인

### ✅ Task 1-5 · EF Core DbContext + SQLite 설정 + 첫 Migration

> ✅ **Database-First 대신 Code-First를 선택한 이유**
>
> - Database-First: DB 테이블 먼저 만들고 → C# 클래스 자동 생성 (SQL을 먼저 알아야 함)
> - **Code-First: C# 클래스 먼저 만들고 → DB 테이블 자동 생성** ← 훨씬 자연스러움
> - 클래스를 수정하면 `Add-Migration` 명령어 한 번으로 DB도 자동으로 업데이트됨

```csharp
// Data/SubLogDbContext.cs
using Microsoft.EntityFrameworkCore;
using SubLog.Model;

namespace SubLog.Data
{
    public class SubLogDbContext : DbContext
    {
        // DbSet<T> 하나 = DB 테이블 하나
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // sublog.db 파일을 실행 파일(.exe) 옆에 생성
            options.UseSqlite("Data Source=sublog.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 기본 카테고리 데이터 자동 삽입 (앱 첫 실행 시)
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "영상", ColorHex = "#E74C3C" },
                new Category { Id = 2, Name = "음악", ColorHex = "#3498DB" },
                new Category { Id = 3, Name = "업무", ColorHex = "#2ECC71" },
                new Category { Id = 4, Name = "게임", ColorHex = "#9B59B6" },
                new Category { Id = 5, Name = "기타", ColorHex = "#95A5A6" }
            );
        }
    }
}
```

```powershell
# 패키지 관리자 콘솔에서 순서대로 실행

Add-Migration InitialCreate   # ① Migration 파일 생성 (Data/Migrations/ 폴더에 생성됨)
Update-Database               # ② 실제 DB 파일(sublog.db) 생성 및 테이블 생성
```

> ⚠️ **Migration 실행 순서 주의**
> `Add-Migration` → `Update-Database` 순서를 반드시 지켜야 함.
> `Update-Database` 먼저 실행하면 "No migrations have been applied" 에러 발생.

- [x]  `Data/SubLogDbContext.cs` 생성 및 코드 작성
- [x]  패키지 관리자 콘솔에서 `Add-Migration InitialCreate` 실행
- [x]  `Data/Migrations/` 폴더에 파일 2개 자동 생성됨 확인
- [x]  `Update-Database` 실행
- [x]  프로젝트 폴더에 `sublog.db` 파일 생성됨 확인
- [x]  (선택) DB 브라우저 for SQLite 앱으로 테이블 구조 시각적 확인

### ✅ Task 1-6 · Repository 패턴 구현

> ✅ **Repository 패턴을 사용하는 이유**
>
> - ViewModel이 `DbContext` 를 직접 사용하면 View-ViewModel-DB가 강하게 결합됨
> - Repository가 중간에서 DB 접근을 추상화 → ViewModel은 DB가 SQLite인지 SQL Server인지 몰라도 됨
> - 실무 면접에서 자주 나오는 질문: "Repository 패턴을 써봤나요?" → "네" 라고 답할 수 있음

```csharp
// Repository/ISubscriptionRepository.cs — 인터페이스 (계약서 역할)
using SubLog.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubLog.Repository
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllAsync();
        Task<Subscription?> GetByIdAsync(int id);
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        Task DeleteAsync(int id);
    }
}
```

```csharp
// Repository/SubscriptionRepository.cs — 실제 구현체
using Microsoft.EntityFrameworkCore;
using SubLog.Data;
using SubLog.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubLog.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SubLogDbContext _context;

        public SubscriptionRepository(SubLogDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetAllAsync()
        {
            return await _context.Subscriptions
                .Include(s => s.Category)   // Category 정보도 함께 가져옴 (JOIN)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByIdAsync(int id)
        {
            return await _context.Subscriptions
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(Subscription subscription)
        {
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sub = await _context.Subscriptions.FindAsync(id);
            if (sub != null)
            {
                _context.Subscriptions.Remove(sub);
                await _context.SaveChangesAsync();
            }
        }
    }
}
```

관련 파일: `Repository/ISubscriptionRepository.cs`, `Repository/SubscriptionRepository.cs`, `Repository/ICategoryRepository.cs`, `Repository/CategoryRepository.cs`

- [x]  `Repository/ISubscriptionRepository.cs` 생성
- [x]  `Repository/SubscriptionRepository.cs` 생성
- [x]  `Repository/ICategoryRepository.cs` 생성 (Subscription과 동일한 패턴)
- [x]  `Repository/CategoryRepository.cs` 생성
- [x]  `Ctrl + Shift + B` 빌드 후 에러 없음 확인
- [x]  **✅ EPIC 1 완료 → GitHub Desktop으로 첫 커밋 Push**

---

## ✅ EPIC 2. 핵심 화면 개발 — 완료

> 💡 **이 EPIC의 목표**
> 앱의 핵심 화면 5개 개발. EPIC 2가 완료되면 구독 추가/조회/수정/삭제(CRUD) + 카탈로그 빠른 추가가 완전히 동작하는 앱 완성.

### ✅ Task 2-1 · MainWindow 레이아웃 (사이드바 네비게이션)

> 💡 **화면 전환 방식 — ContentControl + DataTemplate**
> WPF에서 가장 MVVM스러운 화면 전환 방식.
> ViewModel에서 `CurrentView` 속성을 바꾸면, XAML이 DataTemplate 규칙에 따라 자동으로 맞는 View를 표시함.
> Navigator.Push 처럼 직접 화면을 호출하지 않아도 됨.

```xml
<!-- View/MainWindow.xaml — 핵심 구조 (간략화) -->
<Window.Resources>
    <!-- ViewModel 타입 → View 자동 매핑 규칙 -->
    <DataTemplate DataType="{x:Type vm:DashboardViewModel}">
        <view:DashboardView />
    </DataTemplate>
    <DataTemplate DataType="{x:Type vm:SubscriptionListViewModel}">
        <view:SubscriptionListView />
    </DataTemplate>
</Window.Resources>

<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="200"/>   <!-- 사이드바 -->
        <ColumnDefinition Width="*"/>     <!-- 메인 콘텐츠 -->
    </Grid.ColumnDefinitions>

    <!-- 사이드바 -->
    <StackPanel Grid.Column="0" Background="#2C3E50">
        <Button Content="📊 대시보드"  Command="{Binding NavigateDashboardCommand}"/>
        <Button Content="📋 구독 목록" Command="{Binding NavigateSubscriptionCommand}"/>
        <Button Content="⚙️ 설정"     Command="{Binding NavigateSettingsCommand}"/>
    </StackPanel>

    <!-- 현재 선택된 화면이 자동으로 여기 표시됨 -->
    <ContentControl Grid.Column="1" Content="{Binding CurrentView}"/>
</Grid>
```

- [x]  `View/MainWindow.xaml` 레이아웃 작성 (사이드바 + ContentControl)
- [x]  `ViewModel/MainViewModel.cs` 작성 (`CurrentView` 속성 + Navigate 커맨드들)
- [x]  `MainWindow.xaml.cs` 에서 DataContext 연결 (`DataContext = new MainViewModel()`)
- [x]  버튼 클릭 시 화면 전환 동작 확인

### ✅ Task 2-2 · DashboardView (요약 카드 + LiveCharts2 차트)

> ⚠️ **LiveCharts2 바인딩 주의사항**
> `Series` 속성에 일반 `List<ISeries>` 를 바인딩하면 데이터가 바뀌어도 차트가 갱신되지 않음.
> 반드시 `ObservableCollection<ISeries>` 를 사용해야 함.

```csharp
// ViewModel/DashboardViewModel.cs 핵심 부분
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;

namespace SubLog.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private decimal _totalMonthlySpend;      // 월 총 지출 (Binding: TotalMonthlySpend)

        [ObservableProperty]
        private int _activeSubscriptionCount;    // 활성 구독 수

        [ObservableProperty]
        private int _upcomingBillingCount;       // 7일 내 결제 예정 건수

        // 도넛 차트 데이터 (카테고리별 비율)
        public ObservableCollection<ISeries> DonutSeries { get; set; } = new();

        private readonly ISubscriptionRepository _repo;

        public DashboardViewModel(ISubscriptionRepository repo)
        {
            _repo = repo;
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var subs = await _repo.GetAllAsync();
            TotalMonthlySpend = subs.Where(s => s.IsActive && s.BillingCycle == BillingCycle.Monthly)
                                    .Sum(s => s.Price);
            ActiveSubscriptionCount = subs.Count(s => s.IsActive);
            // 차트 데이터 구성 ...
        }
    }
}
```

- [x]  `View/DashboardView.xaml` 작성 (요약 카드 3개 + 도넛 차트)
- [x]  `ViewModel/DashboardViewModel.cs` 작성
- [x]  LiveCharts2 도넛 차트 바인딩 및 데이터 표시 확인
- [x]  요약 카드 (월 지출 합계 / 구독 수 / 이번 달 결제 예정) 표시 확인

### ✅ Task 2-3 · SubscriptionListView (DataGrid + 필터 / 정렬)

> 💡 **DataGrid + ObservableCollection = 자동 UI 갱신**
> WPF의 `DataGrid` 는 `ItemsSource` 에 `ObservableCollection` 을 바인딩하면,
> 컬렉션에 항목을 추가/삭제할 때 자동으로 화면이 갱신됨.
> WinForm처럼 `dataGridView1.Rows.Add(...)` 를 직접 호출할 필요가 없음.

```xml
<!-- View/SubscriptionListView.xaml — DataGrid 핵심 부분 -->
<DataGrid ItemsSource="{Binding Subscriptions}"
          SelectedItem="{Binding SelectedSubscription}"
          AutoGenerateColumns="False"
          IsReadOnly="True">
    <DataGrid.Columns>
        <DataGridTextColumn Header="서비스명" Binding="{Binding Name}"                         Width="*"/>
        <DataGridTextColumn Header="금액"    Binding="{Binding Price, StringFormat={}{0:N0}원}" Width="110"/>
        <DataGridTextColumn Header="결제일"  Binding="{Binding BillingDay}"                   Width="70"/>
        <DataGridTextColumn Header="주기"    Binding="{Binding BillingCycle}"                  Width="80"/>
        <DataGridTextColumn Header="카테고리" Binding="{Binding Category.Name}"                Width="100"/>
    </DataGrid.Columns>
</DataGrid>
```

- [x]  `View/SubscriptionListView.xaml` 작성 (DataGrid + 필터 TextBox + 추가/수정/삭제 버튼)
- [x]  `ViewModel/SubscriptionListViewModel.cs` 작성 (ObservableCollection + 필터 로직)
- [x]  구독 목록 로드 및 DataGrid 표시 확인
- [x]  키워드 검색 필터 동작 확인 (서비스명 기준)
- [x]  카테고리 / 활성화 상태 필터 동작 확인

### ✅ Task 2-4 · AddEditSubscriptionDialog (구독 추가/수정 폼)

> ⚠️ **입력값 유효성 검사 — CanExecute 패턴**
> 서비스명이 비어있거나 금액이 0 이하이면 저장 버튼이 자동으로 비활성화됨.
> `[RelayCommand(CanExecute = nameof(CanSave))]` 패턴으로 구현.

```csharp
// ViewModel/AddEditSubscriptionViewModel.cs 핵심 부분
public partial class AddEditSubscriptionViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]  // Name 바뀔 때마다 버튼 상태 재확인
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private decimal _price;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        var subscription = new Subscription
        {
            Name         = Name,
            Price        = Price,
            BillingDay   = BillingDay,
            CategoryId   = SelectedCategory!.Id,
            BillingCycle = SelectedBillingCycle,
            StartDate    = StartDate
        };

        if (_isEditMode)
            await _repo.UpdateAsync(subscription);
        else
            await _repo.AddAsync(subscription);

        RequestClose?.Invoke(true);   // 다이얼로그 닫기 신호
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(Name) &&
        Price > 0 &&
        BillingDay is >= 1 and <= 31 &&
        SelectedCategory != null;
}
```

- [x]  `View/AddEditSubscriptionDialog.xaml` 작성 (TextBox, ComboBox, DatePicker, 저장/취소 버튼)
- [x]  `ViewModel/AddEditSubscriptionViewModel.cs` 작성 (유효성 검사 포함)
- [x]  추가 모드 / 수정 모드 구분 처리 (`_isEditMode` 플래그)
- [x]  저장 후 목록 화면 DataGrid 자동 갱신 확인

### ✅ Task 2-5 · 구독 카탈로그 (빠른 추가 프리셋) 🆕

> 💡 **이 Task를 EPIC 2에 추가한 이유**
> "수동 입력이 귀찮아서 앱을 안 쓰게 되지 않을까?"라는 사용자 경험 문제를 개발자 스스로 발견하고 해결한 사례.
> 넷플릭스, 유튜브 프리미엄 등 자주 쓰는 구독 서비스를 클릭 한 번으로 이름 / 가격 / 카테고리를 자동 입력.
> 면접에서 **"UX 개선을 주도적으로 고민하고 구현했다"** 는 스토리로 어필 가능.

> ✅ **구현 방식 — DB 없이 C# 정적 리스트만 사용**
> 카탈로그 데이터는 DB 저장이 필요 없음. `Model/CatalogItem.cs` 에 C# 정적 리스트로 정의.
> 사용자가 항목을 클릭하면 `AddEditSubscriptionDialog` 의 입력 필드에 값이 자동으로 채워짐.

```csharp
// Model/CatalogItem.cs — 카탈로그 프리셋 데이터
namespace SubLog.Model
{
    // 카탈로그 항목 하나를 표현하는 간단한 클래스
    public class CatalogItem
    {
        public string Name          { get; init; } = string.Empty;  // 서비스명
        public decimal Price        { get; init; }                   // 기본 가격
        public string CurrencyCode  { get; init; } = "KRW";         // 통화 (KRW / USD)
        public BillingCycle Cycle   { get; init; } = BillingCycle.Monthly;
        public string CategoryName  { get; init; } = string.Empty;  // 기본 카테고리명
        public string Emoji         { get; init; } = "📦";           // 목록 표시용 이모지
    }

    // 카탈로그 데이터 저장소 (정적 클래스 — 인스턴스 생성 없이 사용)
    public static class SubscriptionCatalog
    {
        public static readonly List<CatalogItem> Items = new()
        {
            new() { Name="넷플릭스",        Price=15.99m,   CurrencyCode="USD", Cycle=BillingCycle.Monthly,  CategoryName="영상", Emoji="🎬" },
            new() { Name="유튜브 프리미엄",  Price=14900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="영상", Emoji="📺" },
            new() { Name="티빙",            Price=13900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="영상", Emoji="🎭" },
            new() { Name="웨이브",          Price=13900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="영상", Emoji="🌊" },
            new() { Name="왓챠",            Price=12900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="영상", Emoji="🎥" },
            new() { Name="스포티파이",       Price=10900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="음악", Emoji="🎵" },
            new() { Name="멜론",            Price=10900m,   CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="음악", Emoji="🍈" },
            new() { Name="Adobe CC",        Price=54.99m,   CurrencyCode="USD", Cycle=BillingCycle.Monthly,  CategoryName="업무", Emoji="🎨" },
            new() { Name="Notion",          Price=10m,      CurrencyCode="USD", Cycle=BillingCycle.Monthly,  CategoryName="업무", Emoji="📝" },
            new() { Name="ChatGPT Plus",    Price=20m,      CurrencyCode="USD", Cycle=BillingCycle.Monthly,  CategoryName="업무", Emoji="🤖" },
            new() { Name="iCloud 200GB",    Price=3900m,    CurrencyCode="KRW", Cycle=BillingCycle.Monthly,  CategoryName="클라우드", Emoji="☁️" },
            new() { Name="Xbox Game Pass",  Price=14.99m,   CurrencyCode="USD", Cycle=BillingCycle.Monthly,  CategoryName="게임", Emoji="🎮" },
        };
    }
}
```

관련 파일: `Model/CatalogItem.cs`, `View/CatalogDialog.xaml`, `ViewModel/CatalogViewModel.cs`

- [x]  `Model/CatalogItem.cs` 생성 (CatalogItem 클래스 + SubscriptionCatalog 정적 데이터)
- [x]  `View/CatalogDialog.xaml` 생성 (카탈로그 목록 ListBox + 선택/취소 버튼)
- [x]  `ViewModel/CatalogViewModel.cs` 생성 (Items 바인딩 + 선택 커맨드)
- [x]  `AddEditSubscriptionDialog` 에 "카탈로그에서 선택" 버튼 추가
- [x]  카탈로그 항목 클릭 시 AddEdit 폼 자동 입력 확인
- [x]  **✅ EPIC 2 완료 → GitHub Desktop으로 커밋 Push**

---

## 🔁 EPIC 3. 고급 기능 구현 — 진행중

> 💡 **이 EPIC의 목표**
> 기본 CRUD를 넘어서는 고급 기능 추가. EPIC 3이 완료되면 포트폴리오에서 "실무 수준의 완성도"를 보여줄 수 있음.

### ✅ Task 3-1 · CategoryManagementView (카테고리 CRUD)

- [x]  `View/CategoryManagementView.xaml` 작성 (목록 + 추가/수정/삭제)
- [x]  `ViewModel/CategoryManagementViewModel.cs` 작성
- [x]  카테고리 색상 선택 기능 (색상 버튼 그룹 또는 TextBox로 HEX 직접 입력)
- [x]  카테고리 삭제 시 연결된 구독 처리 확인 (EF Core Cascade 또는 경고 메시지)

### ⏳ Task 3-2 · SettingsView (다크/라이트 테마 + 통화 단위)

> 💡 **WPF 테마 전환 — ResourceDictionary 런타임 교체**
> `App.xaml` 의 `ResourceDictionary` 를 런타임에 교체하는 방식으로 테마 전환 구현.
> HTML/CSS에서 `body` 의 class를 바꾸는 것과 동일한 개념.

```csharp
// 테마 전환 핵심 코드 (SettingsViewModel.cs 안에 작성)
public static void ChangeTheme(bool isDark)
{
    var themePath = isDark ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
    var dict = new ResourceDictionary
    {
        Source = new Uri(themePath, UriKind.Relative)
    };
    Application.Current.Resources.MergedDictionaries.Clear();
    Application.Current.Resources.MergedDictionaries.Add(dict);
}
```

- [ ]  `Themes/LightTheme.xaml` 작성 (공통 색상 / 폰트 ResourceDictionary)
- [ ]  `Themes/DarkTheme.xaml` 작성
- [ ]  `View/SettingsView.xaml` 작성 (테마 토글 스위치, 통화 단위 콤보박스)
- [ ]  `ViewModel/SettingsViewModel.cs` 작성
- [ ]  설정값 SQLite에 저장/불러오기 (앱 재시작 후에도 유지)

### ⏳ Task 3-3 · 결제일 알림 시스템 (D-Day 배지)

> 💡 **알림 판단 로직**
> 앱 시작 시 모든 구독의 `BillingDay` 와 오늘 날짜를 비교.
> 7일 이내 결제 예정인 구독은 `DaysUntilBilling` 값을 계산하여 목록에 표시.

```csharp
// 결제 D-Day 계산 — Subscription 확장 메서드
public static int CalcDaysUntilBilling(this Subscription sub)
{
    var today = DateTime.Today;
    var billingDate = new DateTime(today.Year, today.Month, sub.BillingDay);
    if (billingDate < today)
        billingDate = billingDate.AddMonths(1);  // 이미 지났으면 다음달로
    return (billingDate - today).Days;
}
```

- [ ]  `Subscription` 확장 메서드 또는 ViewModel 계산 속성으로 D-Day 계산
- [ ]  DataGrid에 D-Day 열 추가 (7일 이내 → 빨간색 강조 표시)
- [ ]  Dashboard에 "이번 달 결제 예정" 카드 표시

### ⏳ Task 3-4 · 통계 분석 뷰 (월별 지출 막대 차트)

- [ ]  `View/StatisticsView.xaml` 작성 (월별 막대 차트 + 카테고리별 도넛 차트)
- [ ]  `ViewModel/StatisticsViewModel.cs` 작성
- [ ]  월별 지출 집계 쿼리 (LINQ + EF Core)

### ⏳ Task 3-5 · 한국수출입은행 환율 API 연동 (달러 구독 원화 자동 환산)

> 💡 **이 Task를 추가한 이유 — 포트폴리오 핵심 차별화 포인트**
> Netflix($15.99), Adobe($54.99) 등 달러 구독 서비스를 원화로 자동 환산.
> 단순 CRUD를 넘어 "외부 REST API 연동 + async/await 비동기 처리 + JSON 파싱"을 함께 보여줄 수 있음.
> 면접에서 "실무에서 자주 쓰는 HTTP 통신 경험"을 자연스럽게 어필할 수 있는 핵심 포인트.

> 🔑 **한국수출입은행 Open API 준비 방법**
> 1. https://www.koreaexim.go.kr → 오픈 API → 회원가입 → API 인증키 발급 (무료, 당일 발급)
> 2. 영업일 기준 매일 오전 11시 이후 당일 환율 데이터 갱신

```csharp
// Services/ExchangeRateService.cs
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SubLog.Services
{
    public class ExchangeRateService
    {
        private const string API_KEY  = "YOUR_API_KEY_HERE";   // 한국수출입은행 발급 키
        private const string BASE_URL =
            "https://www.koreaexim.go.kr/site/program/openapi/selectOpenApi";

        private readonly HttpClient _http = new();

        public async Task<decimal?> GetUsdToKrwAsync()
        {
            var date = DateTime.Today.ToString("yyyyMMdd");
            var url  = $"{BASE_URL}?apiType=AP01&authkey={API_KEY}&searchdate={date}&data=AP01";

            var json  = await _http.GetStringAsync(url);
            var rates = JsonSerializer.Deserialize<List<ExchangeRateDto>>(json);
            var usd   = rates?.FirstOrDefault(r => r.CurUnit == "USD");

            if (usd == null) return null;

            // API 응답이 "1,334.50" 형태 → 쉼표 제거 후 decimal 파싱
            return decimal.TryParse(
                usd.DealBasR.Replace(",", ""), out var rate) ? rate : null;
        }
    }

    // JSON 응답 매핑용 DTO (C# 9+ record 신기능 활용)
    public record ExchangeRateDto(
        [property: JsonPropertyName("cur_unit")]   string CurUnit,   // "USD"
        [property: JsonPropertyName("deal_bas_r")] string DealBasR,  // "1,334.50"
        [property: JsonPropertyName("cur_nm")]     string CurNm      // "미국 달러"
    );
}
```

> ⚠️ **API 호출 관련 주의사항**
> - 주말 / 공휴일에는 직전 영업일 환율 반환 → 날짜 분기 처리 필요
> - 네트워크 오류 / API 키 만료 시 SQLite 저장 환율로 Fallback 처리 필수
> - API 키를 코드에 직접 하드코딩 금지 → 별도 설정 파일 또는 환경변수로 관리

```csharp
// 환율 캐싱 + Fallback 패턴 (ExchangeRateService.cs 에 추가)
public async Task<decimal> GetRateWithFallbackAsync(ISettingsRepository settingsRepo)
{
    try
    {
        var rate = await GetUsdToKrwAsync();
        if (rate.HasValue)
        {
            // 최신 환율을 SQLite에 저장 → 오프라인 시 이 값을 사용
            await settingsRepo.SaveExchangeRateAsync(rate.Value, DateTime.Today);
            return rate.Value;
        }
    }
    catch (HttpRequestException)
    {
        // 네트워크 오류 → 저장된 마지막 환율 사용
    }

    return await settingsRepo.GetLastExchangeRateAsync() ?? 1350m; // 최후 기본값
}
```

관련 파일: `Services/ExchangeRateService.cs`, `Repository/ISettingsRepository.cs`, `Repository/SettingsRepository.cs`

- [ ]  한국수출입은행 Open API 키 발급 (https://www.koreaexim.go.kr)
- [ ]  `Services/` 폴더 생성 → `Services/ExchangeRateService.cs` 작성
- [ ]  `ExchangeRateDto` record 정의 (JSON 매핑)
- [ ]  환율 캐싱 로직 (SQLite Settings 테이블에 마지막 환율 + 날짜 저장)
- [ ]  오프라인 Fallback 처리 구현
- [ ]  `Subscription.cs` 에 `CurrencyCode` 속성 추가 (`"KRW"` / `"USD"` 구분)
- [ ]  구독 목록에 원화 환산 금액 열 추가 (달러 구독에만 표시)
- [ ]  Dashboard 월 지출 합계 계산 시 환율 반영
- [ ]  **✅ EPIC 3 완료 → GitHub Desktop으로 커밋 Push**

---

## 🔵 EPIC 4. 완성 & 포트폴리오

> 💡 **이 EPIC의 목표**
> "작동하는 앱" → "보여주고 싶은 앱"으로 업그레이드. 포트폴리오 제출 준비 완료.

### ⏳ Task 4-1 · 스타일 & 비주얼 폴리쉬 (Style + ControlTemplate)

> 💡 **WPF Style이란?**
> HTML/CSS의 CSS처럼, 여러 컨트롤에 동일한 디자인을 한 번에 적용하는 방법.
> `App.xaml` 에 전역 Style을 정의하면 앱 전체에서 자동으로 적용됨.

```xml
<!-- App.xaml — 전역 버튼 스타일 예시 -->
<Application.Resources>
    <Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="Background"       Value="#3498DB"/>
        <Setter Property="Foreground"       Value="White"/>
        <Setter Property="Padding"          Value="16 8"/>
        <Setter Property="BorderThickness"  Value="0"/>
        <Setter Property="Cursor"           Value="Hand"/>
        <Setter Property="FontSize"         Value="13"/>
    </Style>
</Application.Resources>
```

- [ ]  공통 버튼 Style 정의 (Primary / Secondary / Danger 3종)
- [ ]  DataGrid 행 hover 효과 + 교번색 행 적용
- [ ]  사이드바 네비게이션 선택 상태 강조 처리
- [ ]  전체 앱 폰트 통일 (`Segoe UI` 또는 `Noto Sans KR`)

### ⏳ Task 4-2 · 데이터 유효성 검사 & 예외 처리

- [ ]  IDataErrorInfo 또는 DataAnnotations 기반 입력 유효성 검사 완성
- [ ]  EF Core 예외 (중복, 제약조건 위반) try-catch 처리
- [ ]  사용자 친화적 에러 메시지 MessageBox 표시
- [ ]  앱 전역 예외 처리 (`App.xaml.cs` 의 `DispatcherUnhandledException`)

### ⏳ Task 4-3 · MSIX 패키징 (배포 준비)

> 💡 **MSIX란?**
> Windows 앱의 현대적 설치 패키지 형식. `.exe` + 의존성을 하나의 파일로 묶어서 다른 PC에 설치 가능.
> GitHub README에 다운로드 링크로 첨부하면 포트폴리오를 바로 실행해볼 수 있음.

- [ ]  Visual Studio → 게시 → MSIX 패키지 설정
- [ ]  패키지 생성 및 다른 PC에서 설치 테스트
- [ ]  GitHub Releases에 `.msix` 파일 업로드

### ⏳ Task 4-4 · GitHub README 완성 + GIF 데모 촬영

> 🛠️  **GIF 촬영 도구**
> - [ScreenToGif](https://www.screentogif.com/) — 무료, 화면 일부만 녹화 가능
> - 권장 해상도: 800×600, 15fps, 5초 내외로 핵심 기능 데모

- [ ]  스크린샷 촬영 (대시보드 / 구독 목록 / 추가 폼 / 통계 화면)
- [ ]  GIF 데모 촬영 (구독 추가 → 목록 자동 갱신 흐름)
- [ ]  `README.md` 스크린샷 및 GIF 삽입
- [ ]  `README.md` MSIX 다운로드 링크 추가

### ⏳ Task 4-5 · 최종 테스트 & 포트폴리오 리뷰

- [ ]  전체 CRUD 플로우 테스트 (추가 → 수정 → 삭제 → 목록 갱신)
- [ ]  앱 재시작 후 데이터 유지 확인 (SQLite 영속성 검증)
- [ ]  다크/라이트 테마 전환 확인
- [ ]  GitHub README 최종 검토 (오타, 링크 확인)
- [ ]  **✅ EPIC 4 완료 → GitHub Desktop으로 최종 Push 🎉**

---

## 📋 전체 Task 요약 일정

| Task | 내용 | EPIC | 우선순위 | 상태 |
| --- | --- | --- | --- | --- |
| 1-1 | WPF 프로젝트 생성 (VS2022) | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 1-2 | MVVM 폴더 구조 + App.xaml 수정 | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 1-3 | NuGet 패키지 5종 설치 | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 1-4 | Model 클래스 정의 (Subscription, Category) | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 1-5 | EF Core DbContext + SQLite + Migration | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 1-6 | Repository 패턴 구현 | 기반 구축 | 🔥 P1 | ✅ 완료 |
| 2-1 | MainWindow 레이아웃 (사이드바 네비게이션) | 핵심 화면 | 🔥 P1 | ✅ 완료 |
| 2-2 | DashboardView (요약 카드 + 차트) | 핵심 화면 | 🔥 P1 | ✅ 완료 |
| 2-3 | SubscriptionListView (DataGrid + 필터) | 핵심 화면 | 🔥 P1 | ✅ 완료 |
| 2-4 | AddEditSubscriptionDialog (추가/수정 폼) | 핵심 화면 | 🔥 P1 | ✅ 완료 |
| 2-5 | 구독 카탈로그 (빠른 추가 프리셋) 🆕 | 핵심 화면 | 🔥 P1 | ✅ 완료 |
| 3-1 | CategoryManagementView | 고급 기능 | P1 | ✅ 완료 |
| 3-2 | SettingsView (테마 · 통화 단위) | 고급 기능 | P1 | ⏳ 예정 |
| 3-3 | 결제일 알림 시스템 (D-Day 배지) | 고급 기능 | P2 | ⏳ 예정 |
| 3-4 | 통계 분석 뷰 (월별 막대 차트) | 고급 기능 | P1 | ⏳ 예정 |
| 3-5 | 한국수출입은행 환율 API 연동 | 고급 기능 | 🔥 P1 | ⏳ 예정 |
| 4-1 | 스타일 & 비주얼 폴리쉬 | 완성 | P1 | ⏳ 예정 |
| 4-2 | 데이터 유효성 검사 & 예외 처리 | 완성 | P1 | ⏳ 예정 |
| 4-3 | MSIX 패키징 | 완성 | P2 | ⏳ 예정 |
| 4-4 | GitHub README 완성 + GIF 데모 | 완성 | P1 | ⏳ 예정 |
| 4-5 | 최종 테스트 & 포트폴리오 리뷰 | 완성 | P1 | ⏳ 예정 |

---

> 🔥 **개발 우선순위**
>
> 1. ✅ Task 1-1 ~ 1-6: 프로젝트 생성 + NuGet 설치 + DB 구축 → 완료
> 2. ✅ Task 2-1 ~ 2-3: MainWindow + Dashboard + SubscriptionList → 완료
> 3. ✅ Task 2-4: AddEditSubscriptionDialog → 다음 작업 → 완료
> 4. ✅ Task 2-5: 구독 카탈로그 → 2-4 직후 연결 구현 → 완료
> 5. ⏳ Task 3-5: 환율 API 연동 → 포트폴리오 최대 차별화 포인트 (면접 어필 핵심)

---

## GitHub 링크

> https://github.com/sagming40/SubLog

---

*SubLog WORKFLOW v1.1 · 사공민규 · 최초 작성 2026.06.22 · 수정 2026.06.24*
