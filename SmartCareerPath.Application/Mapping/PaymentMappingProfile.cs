using AutoMapper;
using SmartCareerPath.Application.Abstraction.DTOs.Payment;
using SmartCareerPath.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // PaymentTransaction -> PaymentTransactionResponse
            CreateMap<PaymentTransaction, PaymentTransactionResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.ToString()))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src =>
                    src.PaymentMethod.HasValue ? src.PaymentMethod.Value.ToString() : null))
                .ForMember(dest => dest.BillingCycle, opt => opt.MapFrom(src =>
                    src.BillingCycle.HasValue ? src.BillingCycle.Value.ToString() : null));

            // RefundRequest -> RefundRequestResponse
            CreateMap<RefundRequest, RefundRequestResponse>()
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
                .ForMember(dest => dest.DisplayAmount, opt => opt.MapFrom(src => src.GetDisplayAmount()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ReviewedByAdminName, opt => opt.MapFrom(src =>
                    src.ReviewedByAdmin != null ? src.ReviewedByAdmin.FullName : null));
        }
    }
}
