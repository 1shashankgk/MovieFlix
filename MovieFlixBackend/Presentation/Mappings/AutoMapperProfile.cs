using AutoMapper;
using MovieFlixBackend.Domain.Entities;
using MovieFlixBackend.Application.ViewModels;

namespace MovieFlixBackend.Presentation.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Movie, MovieViewModel>();
        }
    }
}
