using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubLog.Data;
using SubLog.Model;

namespace SubLog.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SubLogDbContext _context;

        // 생성자 - DbContext를 외부에서 주입받음
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
            await _context.SaveChangesAsync();  // DB에 실제 저장
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            // ❌ 기존 방식: 새 객체를 통째로 Update() → 추적 충돌 발생
            /* _context.Subscriptions.Update(subscription); */

            // ✅ 수정된 방식:
            // 이미 추적 중인 기존 객체를 찾고 → 속성만 바꾸고 → 저장
            var existing = await _context.Subscriptions.FindAsync(subscription.Id);
            if (existing is null) return;

            // 기존 추적 객체의 속성을 하나씩 업데이트 (객체 교체 X)
            existing.Name           = subscription.Name;
            existing.Price          = subscription.Price;
            existing.BillingDay     = subscription.BillingDay;
            existing.BillingCycle   = subscription.BillingCycle;
            existing.StartDate      = subscription.StartDate;
            existing.IsActive       = subscription.IsActive;
            existing.Memo           = subscription.Memo;
            existing.CategoryId     = subscription.CategoryId;

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
