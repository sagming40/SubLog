using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SubLog.Repository;

namespace SubLog.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ISubscriptionRepository _repo;

        // MainViewModel에서 _subscriptionRepo를 넘겨받아 보관
        public DashboardViewModel(ISubscriptionRepository repo)
        {
            _repo = repo;
        }
    }
}
