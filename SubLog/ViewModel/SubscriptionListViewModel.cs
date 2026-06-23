using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SubLog.Repository;

namespace SubLog.ViewModel
{
    public partial class SubscriptionListViewModel : ObservableObject
    {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly ICategoryRepository _categoryRepo;

        public SubscriptionListViewModel(
            ISubscriptionRepository subscriptionRepo,
            ICategoryRepository categoryRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _categoryRepo = categoryRepo;
        }
    }
}
