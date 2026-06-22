using SubLog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubLog.Repository
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetAllAsync();         // 전체 목록
        Task<Subscription?> GetByIdAsync(int id);       // 단건 조회
        Task AddAsync(Subscription subscription);       // 추가
        Task UpdateAsync(Subscription subscription);    // 수정
        Task DeleteAsync(int id);                       // 삭제
    }
}
