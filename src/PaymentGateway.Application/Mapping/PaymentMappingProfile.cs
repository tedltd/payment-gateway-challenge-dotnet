using AutoMapper;
using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Request;

namespace PaymentGateway.Application.Mappings
{
    // probsbly should use mapper, since auto Mapper is becoming commercial
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<PaymentRequest, PaymentGatewayRequest>()
                .ForMember(x => x.ExpiryDate, opt => opt.MapFrom(src => $"{src.ExpiryMonth:D2}/{src.ExpiryYear}"))
                .ForMember(x => x.CardNumber, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(x => x.Cvv, opt => opt.MapFrom(src => src.Cvv))
                .ForMember(x => x.Amount,opt => opt.MapFrom(src => src.Amount))
                .ForMember(x => x.Currency, opt => opt.MapFrom(src => src.Currency));

            CreateMap<PaymentGatewayResponse, PostPaymentResponse>()
                .ForMember(x => x.Id,
                    opt => opt.MapFrom(src => Guid.Parse(src.AuthorizationCode)))
                .ForMember(x => x.Status,
                    opt => opt.MapFrom(src => src.Authorized ? PaymentStatus.Authorized.ToString()  : PaymentStatus.Declined.ToString()));
        }
    }
}