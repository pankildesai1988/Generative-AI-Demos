using ArNir.Core.DTOs.Documents;
using ArNir.Core.Entities;
using AutoMapper;

namespace ArNir.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Document → DTO
            CreateMap<Document, DocumentResponseDto>();
            CreateMap<DocumentChunk, DocumentChunkDto>();

            // DTO → Document (useful for imports or seeding)
            CreateMap<DocumentResponseDto, Document>();
            CreateMap<DocumentChunkDto, DocumentChunk>();
        }
    }
}
