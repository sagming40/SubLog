using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SubLog.Repository;
using System.Text;

namespace SubLog.Services
{
    // ══════════════════════════════════════════════════════
    // ExchangeRateDto — JSON 응답을 담는 그릇
    //
    // record 타입이란?
    // class와 비슷하지만 데이터를 담기만 할 때 쓰는 가벼운 타입.
    // 아래처럼 한 줄로 속성 여러 개를 한 번에 선언할 수 있음.
    //
    // [property: JsonPropertyName("cur_unit")]
    // → API가 보내는 JSON 키 이름("cur_unit")과
    //   C# 속성 이름(CurUnit)을 연결하는 매핑 설정
    // ══════════════════════════════════════════════════════
    public record ExchangeRateDto(
        [property: JsonPropertyName("cur_unit")]   string CurUnit,  // "USD"
        [property: JsonPropertyName("deal_bas_r")] string DealBasR, // "1,535.13"
        [property: JsonPropertyName("cur_nm")]     string CurNm     // "미국 달러"
    );

    public class ExchangeRateService
    {
        // ══════════════════════════════════════════════════════
        // 상수 (절대 안 바뀌는 값)
        // ══════════════════════════════════════════════════════
        private const string API_KEY = Secrets.ExchangeRateApiKey;
        private const string BASE_URL =
            "https://oapi.koreaexim.go.kr/site/program/financial/exchangeJSON";

        // HttpClient = 인터넷 우편배달부
        // static readonly = 앱 전체에서 딱 한명만 고용 (재사용 권장)
        // new() 만들때 마다 새 인스턴스 = 배달부를 매번 새로 고용하는 것 → 비효율
        private static readonly HttpClient _http = new();

        // AppSettings 테이블에 환율 캐싱하기 위한 Repository
        private readonly ISettingsRepository _settingsRepo;

        // 캐싱에 쓸 키 이름 (오타 방지용 상수)
        private const string RATE_KEY = "LastExchangeRate";
        private const string DATE_KEY = "LastExchangeRateDate";

        public ExchangeRateService(ISettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo;
        }

        // ══════════════════════════════════════════════════════
        // GetUsdToKrwAsync — 환율 가져오기 (Fallback 포함)
        //
        // 동작 순서:
        // 1순위) API 호출 성공 → 최신 환율 반환 + DB에 저장
        // 2순위) API 실패    → DB에 저장된 마지막 환율 반환
        // 3순위) DB에도 없음 → 기본값 1,350원 반환
        // ══════════════════════════════════════════════════════
        public async Task<decimal> GetUsdToKrwAsync()
        {
            try
            {
                // 주말이면 직전 금요일 날짜로 요청
                // (주말엔 영업일이 아니라 빈 배열을 반환하기 때문)
                var targetDate = DateTime.Today;
                if (targetDate.DayOfWeek == DayOfWeek.Saturday)
                    targetDate = targetDate.AddDays(-1);    // 토 → 금
                else if (targetDate.DayOfWeek == DayOfWeek.Sunday)
                    targetDate = targetDate.AddDays(-2);    // 일 → 금

                // 오늘 날짜 형식: "20260627"
                var today = targetDate.ToString("yyyyMMdd");

                // URL 조합 (문자열 보간 $"..." 방식)
                // 실제 요청: https://oapi.../exchangeJSON?authkey=키&searchdate=20260627&data=AP01
                var url = $"{BASE_URL}?authkey={API_KEY}&searchdate={today}&data=AP01";

                // ① API 호출 — "이 URL로 GET 요청 보내줘, 응답 문자열 줘"
                // GetStringAsync = 편지 보내고 답장(문자열)을 받아오는 메서드
                var json = await _http.GetStringAsync(url);

                // ✅ CaseInsensitive 옵션 추가 ㅡ 대소문자 오류 방지
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                // ② JSON 파싱 — 문자열 → C# 리스트로 번역
                // API 응답이 배열 형태 [{ }, { }, ...] 이라서 List<>로 받음
                var rates = JsonSerializer.Deserialize<List<ExchangeRateDto>>(json, options);

                // ③ USD 항목만 골라냄 (CurUnit == "USD")
                var usd = rates?.FirstOrDefault(r => r.CurUnit == "USD");

                if (usd != null)
                {
                    // ④ "1,535.13" → 쉼표 제거 → "1535.13" → decimal 변환
                    // TryParse = 변환 성공하면 true, out var rate에 결과 담김
                    if (decimal.TryParse(
                            usd.DealBasR.Replace(",", ""),
                            out var rate))
                    {
                        // ⑤ DB에 저장 (다음번 오프라인 시 쓸 캐시)
                        await _settingsRepo.SetAsync(RATE_KEY, rate.ToString());
                        await _settingsRepo.SetAsync(DATE_KEY, today);

                        return rate;    // 최신 환율 반환 ✅
                    }
                }
            }
            catch (Exception)
            {
                // 네트워크 오류, 파싱 오류 등 모든 예외 → 아래 Fallback으로 넘어감
                // (예외를 무시하고 계속 진행하는 것이 목적이라 catch 블록 비워둠)
            }

            // ─── Fallback 1: DB에 저장된 마지막 환율 ───
            var cached = await _settingsRepo.GetAsync(RATE_KEY);
            if (cached != null && decimal.TryParse(cached, out var cachedRate))
                return cachedRate;  // 저장된 환율 반환 ✅

            // ─── Fallback 2: 최후 기본값 ───
            return 1_530m; // 1530원 (숫자 가독성을 위한 _ 구분자)
        }
    }
}
