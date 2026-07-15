using AutoMapper;
using StudentPetitionAPI.Domain.Entities;
using StudentPetitionAPI.Application.DTOs.Requests;
using StudentPetitionAPI.Application.DTOs.Responses;

namespace StudentPetitionAPI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateStudentRequest, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Petitions, opt => opt.Ignore());

        CreateMap<Student, StudentResponse>();

        CreateMap<CreatePetitionRequest, Petition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewComment, opt => opt.Ignore());

        CreateMap<UpdatePetitionRequest, Petition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StudentId, opt => opt.Ignore())
            .ForMember(dest => dest.Student, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewComment, opt => opt.Ignore());

        CreateMap<Petition, PetitionResponse>()
            .ForMember(
                dest => dest.StudentFirstName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.FirstName : string.Empty))
            .ForMember(
                dest => dest.StudentLastName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.LastName : string.Empty))
            .ForMember(
                dest => dest.StudentNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.StudentNumber : string.Empty));
    }
}
