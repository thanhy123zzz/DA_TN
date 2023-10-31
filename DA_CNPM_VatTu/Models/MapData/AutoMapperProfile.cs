using AutoMapper;
using DA_CNPM_VatTu.Models.Entities;
using System.Globalization;

namespace DA_CNPM_VatTu.Models.MapData
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() {
            CreateMap<PhieuNhapKhoMap, PhieuNhapKho>()
            .ForMember(dest => dest.NgayTao, opt => opt.MapFrom(src => src.NgayTao != "" ? DateTime.ParseExact(src.NgayTao, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture) : (DateTime?)null))
            .ForMember(dest => dest.NgayHd, opt => opt.MapFrom(src => src.NgayHd != "" ? DateTime.ParseExact(src.NgayHd, "dd-MM-yyyy", CultureInfo.InvariantCulture) : (DateTime?)null));
            CreateMap<ChiTietPhieuNhapMap, ChiTietPhieuNhap>()
            .ForMember(dest => dest.Nsx, opt => opt.MapFrom(src => src.Nsx != "" ? DateTime.ParseExact(src.Nsx, "dd-MM-yyyy", CultureInfo.InvariantCulture) : (DateTime?)null))
            .ForMember(dest => dest.Hsd, opt => opt.MapFrom(src => src.Hsd != "" ? DateTime.ParseExact(src.Hsd, "dd-MM-yyyy", CultureInfo.InvariantCulture) : (DateTime?)null));
        }
    }
}
