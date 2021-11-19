﻿using System.Threading.Tasks;
using Smartstore.Engine.Modularity;
using Smartstore.OfflinePayment.Settings;

namespace Smartstore.Payment
{
    internal class Module : ModuleBase
    {
        public override async Task InstallAsync(ModuleInstallationContext context)
        {
            await SaveSettingsAsync<CashOnDeliveryPaymentSettings>();
            await SaveSettingsAsync<InvoicePaymentSettings>();
            await SaveSettingsAsync<PayInStorePaymentSettings>();
            await SaveSettingsAsync<PrepaymentPaymentSettings>();
            await SaveSettingsAsync<ManualPaymentSettings>();
            await SaveSettingsAsync<DirectDebitPaymentSettings>();
            await SaveSettingsAsync<PurchaseOrderNumberPaymentSettings>();
            
            await ImportLanguageResourcesAsync();
            await base.InstallAsync(context);
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync<CashOnDeliveryPaymentSettings>();
            await DeleteSettingsAsync<InvoicePaymentSettings>();
            await DeleteSettingsAsync<PayInStorePaymentSettings>();
            await DeleteSettingsAsync<PrepaymentPaymentSettings>();
            await DeleteSettingsAsync<ManualPaymentSettings>();
            await DeleteSettingsAsync<DirectDebitPaymentSettings>();
            await DeleteSettingsAsync<PurchaseOrderNumberPaymentSettings>();

            await DeleteLanguageResourcesAsync();
            await base.UninstallAsync();
        }
    }
}
