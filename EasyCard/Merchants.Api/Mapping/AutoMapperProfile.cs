using AutoMapper;
using IdentityServerClient;
using Merchants.Api.Models;
using Merchants.Api.Models.Audit;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.System;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.TerminalTemplate;
using Merchants.Api.Models.User;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Merchants.Business.Models.Integration;
using Merchants.Shared.Models;
using System;
using System.Collections.Generic;

namespace Merchants.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() => RegisterMappings();

        internal void RegisterMappings()
        {
            RegisterMerchantMappings();
            RegisterTerminalMappings();
            RegisterUserMappings();
            RegisterSystemSettingsMappings();
        }

        private void RegisterTerminalMappings()
        {
            CreateMap<TerminalRequest, Terminal>()
                .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));
            CreateMap<UpdateTerminalRequest, Terminal>();

            CreateMap<Terminal, TerminalResponse>();
            CreateMap<Terminal, TerminalSummary>()
                .ForMember(m => m.MerchantBusinessName, o => o.MapFrom(src => src.Merchant.BusinessName));
                //.ForMember(m => m.MerchantID, o => o.MapFrom(src => src.MerchantID));
            CreateMap<ExternalSystem, ExternalSystemSummary>();

            CreateMap<TerminalExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<ExternalSystemRequest, TerminalExternalSystem>();

            // Mappings for settings (override terminal settings from system settings if null)

            CreateMap<SystemSettings, TerminalResponse>()
                .ForMember(d => d.Settings, o => o.MapFrom(d => d.Settings))
                .ForMember(d => d.BillingSettings, o => o.MapFrom(d => d.BillingSettings))
                .ForMember(d => d.PaymentRequestSettings, o => o.MapFrom(d => d.PaymentRequestSettings))
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings))
                .ForMember(d => d.InvoiceSettings, o => o.MapFrom(d => d.InvoiceSettings))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<SystemInvoiceSettings, TerminalInvoiceSettings>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<SystemGlobalSettings, TerminalSettings>()
                .ForMember(d => d.VATRateGlobal, o => o.MapFrom(d => d.VATRate))
                .ForMember(d => d.VATRate, o => o.Ignore())
                .ForAllOtherMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<SystemPaymentRequestSettings, TerminalPaymentRequestSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemCheckoutSettings, TerminalCheckoutSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));
            CreateMap<SystemBillingSettings, TerminalBillingSettings>()
              .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => destMember == null));

            CreateMap<TerminalTemplate, Terminal>()
                .ForMember(d => d.Label, o => o.Ignore());
            CreateMap<TerminalTemplate, TerminalTemplateSummary>();
            CreateMap<TerminalTemplate, TerminalTemplateResponse>();
            CreateMap<TerminalTemplateRequest, TerminalTemplate>()
               .ForMember(m => m.Created, o => o.MapFrom((src, tgt) => tgt.Created = DateTime.UtcNow));

            CreateMap<SystemSettings, TerminalTemplateResponse>()
                .ForMember(d => d.Settings, o => o.MapFrom(d => d.Settings))
                .ForMember(d => d.BillingSettings, o => o.MapFrom(d => d.BillingSettings))
                .ForMember(d => d.PaymentRequestSettings, o => o.MapFrom(d => d.PaymentRequestSettings))
                .ForMember(d => d.CheckoutSettings, o => o.MapFrom(d => d.CheckoutSettings))
                .ForMember(d => d.InvoiceSettings, o => o.MapFrom(d => d.InvoiceSettings))
                .ForAllOtherMembers(d => d.Ignore());

            CreateMap<TerminalTemplateExternalSystem, TerminalExternalSystemDetails>();
            CreateMap<TerminalTemplateExternalSystem, TerminalExternalSystem>();
            CreateMap<ExternalSystemRequest, TerminalTemplateExternalSystem>();
            CreateMap<Feature, FeatureSummary>();
            CreateMap<Plan, PlanSummary>();
        }

        private void RegisterMerchantMappings()
        {
            CreateMap<Merchant, MerchantSummary>();
            CreateMap<Merchant, MerchantResponse>();
            CreateMap<MerchantRequest, Merchant>();
            CreateMap<UpdateMerchantRequest, Merchant>();
            CreateMap<MerchantHistory, AuditEntryResponse>()
                .ForMember(d => d.TerminalName, o => o.MapFrom(src => src.Terminal.Label))
                .ForMember(d => d.MerchantName, o => o.MapFrom(src => src.Merchant.BusinessName));
        }

        private void RegisterUserMappings()
        {
            CreateMap<UserProfileDataResponse, UserResponse>();
            CreateMap<InviteUserRequest, CreateUserRequestModel>();
            CreateMap<UserTerminalMapping, UserSummary>();
            CreateMap<Business.Entities.User.UserInfo, UserSummary>();
            CreateMap<UserProfileDataResponse, Business.Entities.User.UserInfo>();
            CreateMap<LinkUserToMerchantRequest, Business.Entities.User.UserInfo>();
        }

        private void RegisterSystemSettingsMappings()
        {
            CreateMap<SystemSettings, SystemSettingsResponse>();
            CreateMap<SystemSettingsRequest, SystemSettings>();
        }
    }
}
