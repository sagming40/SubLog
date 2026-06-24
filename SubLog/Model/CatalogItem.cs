using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Model
{
    // ══════════════════════════════════════════════════
    // 카탈로그 항목 하나를 표현하는 일반 C# 클래스
    // EF Core Entity가 아님! DB 저장 안 함.
    // ══════════════════════════════════════════════════
    public class CatalogItem
    {
        public string Emoji        { get; init; } = "📦";
        public string Name         { get; init; } = string.Empty;
        public decimal Price       { get; init; }
        public string CurrencyCode { get; init; } = "KRW";                // "KRW" 또는 "USD"
        public BillingCycle Cycle  { get; init; } = BillingCycle.Monthly;
        public string CategoryName { get; init; } = string.Empty;         // DB 카테고리명과 매핑
        public string Desciption   { get; init; } = string.Empty;

        // ── 화면 표시용 계산 속성 ──
        // KRW → "14,900원", USD → "$15.99"
        public string PriceText => CurrencyCode == "KRW"
            ? $"{Price:N0}원"
            : $"${Price}";

        // 결제 주기 한국어
        public string CycleText => Cycle switch
        {
            BillingCycle.Monthly => "월",
            BillingCycle.Yearly  => "년",
            BillingCycle.Weekly  => "주",
            _                    => "월"
        };

        // ListBox에 표시될 가격 + 주기 문자열 (예: "14,900원 / 월")
        public string PriceDisplay => $"{PriceText} / {CycleText}";
    }

    // ═════════════════════════════════════════════════════════
    // 카탈로그 정적 데이터 — 앱에 내장된 프리셋 목록
    // static: 인스턴스 없이 SubscriptionCatalog.Items 로 바로 접근
    // ═════════════════════════════════════════════════════════
    public static class SubscriptionCatalog
    {
        public static readonly List<CatalogItem> Items = new()
        {
            // ── 영상 ──
            new() { Emoji="🎬", Name="NETFLIX",          Price=15.99m, CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="글로벌 스트리밍 서비스" },
            new() { Emoji="📺", Name="Youtube Premium",  Price=14900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="광고 없는 유튜브 + 백그라운드 재생" },
            new() { Emoji="🏰", Name="Disney+",          Price=9900m,  CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="디즈니 · 마블 · 스타워즈" },
            new() { Emoji="🌊", Name="Wavve",            Price=13900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="국내 드라마 · 예능 OTT" },
            new() { Emoji="🎭", Name="TVING",            Price=13900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="tvN · OCN · JTBC 컨텐츠" },
            new() { Emoji="🎥", Name="WATCHA",           Price=12900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="영상", Desciption="영화 · 드라마 · 애니" },

            // ── 음악 ──
            new() { Emoji="🍈", Name="Melon",             Price=10900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="음악", Desciption="국내 1위 음악 스트리밍" },
            new() { Emoji="🎧", Name="Spotify",           Price=10900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="음악", Desciption="글로벌 음악 스트리밍" },
            new() { Emoji="🎼", Name="Apple Music",       Price=8900m,  CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="음악", Desciption="Apple 음악 서비스" },
            new() { Emoji="🎤", Name="genie",             Price=7900m,  CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="음악", Desciption="KT 음악 스트리밍" },

            // ── 업무 ──
            new() { Emoji="💼", Name="Microsoft 365",     Price=8900m,  CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="업무", Desciption="Word · Excel · PowerPoint" },
            new() { Emoji="🎨", Name="Adobe CC",          Price=54.99m, CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="업무", Desciption="Photoshop · Illustrator · Premiere" },
            new() { Emoji="📝", Name="Notion",            Price=10m,    CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="업무", Desciption="노트 · 위키 · 데이터베이스" },
            new() { Emoji="🤖", Name="ChatGPT Plus",      Price=20m,    CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="업무", Desciption="GPT-4o AI 어시스턴트" },

            // ── 게임 ──
            new() { Emoji="🎮", Name="Xbox Game Pass",        Price=14.99m, CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="게임", Desciption="100+ 게임 무제한 플레이" },
            new() { Emoji="🎯", Name="PlayStation Plus",      Price=8900m,  CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="게임", Desciption="온라인 멀티플레이 · 무료 게임" },
            new() { Emoji="🕹️", Name="Nitendo Switch Online", Price=14.99m, CurrencyCode="USD",
                    Cycle=BillingCycle.Monthly, CategoryName="게임", Desciption="온라인 플레이 · 클래식 게임" },

            // ── 기타 ──
            new() { Emoji="🌐", Name="네이버 플러스",       Price=4900m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="기타", Desciption="네이버 쇼핑 적립 · 컨텐츠" },
            new() { Emoji="🛒", Name="쿠팡 로켓와우",       Price=7890m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="기타", Desciption="무료/새벽/당일 배송 · 배달비 면제/할인" },
            new() { Emoji="☁️", Name="iCloud 200GB",       Price=1400m, CurrencyCode="KRW",
                    Cycle=BillingCycle.Monthly, CategoryName="기타", Desciption="Apple 클라우드 저장소" },
        };
    }
}
