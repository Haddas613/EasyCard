using CheckoutPortal.Models;
using System.Collections.Concurrent;

namespace CheckoutPortal.Services
{
    /// <summary>
    /// Temporary solution. Should store ChargeViewModel from HomeController.Charge in case it's needed afterwards.
    /// To be injected as singleton.
    /// </summary>
    public class MemoryPaymentRequestStorage
    {
        private ConcurrentDictionary<string, ChargeViewModel> Storage { get; set; }

        public MemoryPaymentRequestStorage()
        {
            Storage = new ConcurrentDictionary<string, ChargeViewModel>();
        }

        public void AddOrUpdate(string key, ChargeViewModel model)
        {
            Storage.AddOrUpdate(key, model, (k, v) =>
            {
                Storage.TryRemove(k, out _);
                Storage.TryAdd(k, v);
                return v;
            });
        }

        public ChargeViewModel Get(string key, bool removeAfter = false)
        {
            if (Storage.TryGetValue(key, out ChargeViewModel model))
            {
                if (removeAfter)
                {
                    Storage.TryRemove(key, out _);
                }

                return model;
            }

            return null;
        }
    }
}
