using SmartCareerPath.Application.Abstraction.ServicesContracts.Payment;
using SmartCareerPath.Domain.Enums;

namespace SmartCareerPath.Application.Strategies.Payment
{
    public class PaymentStrategyFactory
    {
        private readonly IEnumerable<IPaymentStrategy> _strategies;

        public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
        {
            _strategies = strategies;
        }

        /// <summary>
        /// Get payment strategy for specified provider.
        /// </summary>
        public IPaymentStrategy GetStrategy(PaymentProvider provider)
        {
            var strategy = _strategies.FirstOrDefault(s => s.Provider == provider);

            if (strategy == null)
            {
                throw new NotSupportedException($"Payment provider {provider} is not supported");
            }

            return strategy;
        }

        /// <summary>
        /// Check if provider is supported.
        /// </summary>
        public bool IsProviderSupported(PaymentProvider provider)
        {
            return _strategies.Any(s => s.Provider == provider);
        }

        /// <summary>
        /// Get all supported providers.
        /// </summary>
        public List<PaymentProvider> GetSupportedProviders()
        {
            return _strategies.Select(s => s.Provider).ToList();
        }
    }
}
