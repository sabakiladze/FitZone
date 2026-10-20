using AutoMapper;
using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application
{
   public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequestDTO, User>();
            CreateMap<User, AuthResponseDTO>();
            CreateMap<User, RegisterResponseDTO>();
        }
    }
}
