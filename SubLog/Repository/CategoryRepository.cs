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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SubLogDbContext _context;

        public CategoryRepository(SubLogDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        // ✅ SubscriptionRepository와 동일한 FindAsync 패턴으로 수정
        // 이유: _context.Categories.Update()는 이미 추적 중인 엔티티와
        //       충돌할 수 있음 (InvalidOperationException 위험)
        public async Task UpdateAsync(Category category)
        {
            /* _context.Categories.Update(category);
            await _context.SaveChangesAsync(); */

            // 1단계: DbContext가 이미 추적 중인 엔티티를 ID로 찾아옴
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing is null) return; // 없으면 조용히 종료

            // 2단계: 찾아온 추적 객체의 속성을 하나씩 덮어씀
            //       (객체 자체를 교체하면 추적이 끊김 — 속성만 변경!)
            existing.Name = category.Name;
            existing.ColorHex = category.ColorHex;

            // 3단계: 변경 사항을 DB에 저장
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
