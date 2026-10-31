using AutoMapper;
using FitZone.Application.DTOs.Auth;
using FitZone.Application.DTOs.AuthDTOs;
using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.DTOs.TrainerDTOs;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MembershipPlan, MembershipPlanDTO>();


            CreateMap<UserMembershipPlan, UserMembershipDTO>()

           .ForMember(d => d.PlanName, o => o.MapFrom(s => s.MembershipPlan.Name))
           .ForMember(d => d.IsActive, o => o.MapFrom(s => s.EndDate > DateTime.UtcNow));


          

            CreateMap<Visit, VisitDTO>()
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.UserMembershipPlan.MembershipPlan.Name));

            CreateMap<Trainer, TrainerDTO>();

            CreateMap<CreateTrainerRequestDTO, Trainer>()
                      .ForMember(d => d.Id, o => o.Ignore())
                      .ForMember(d => d.WorkoutSessions, o => o.Ignore());


            CreateMap<WorkoutSession, WorkoutSessionDTO>()
                    .ForMember(d => d.TrainerName, o => o.MapFrom(s => s.Trainer!.FullName))
                     .ForMember(d => d.FreeSpots, o => o.MapFrom(s =>
                     Math.Max(0, s.Capacity - s.Bookings.Count(b =>
                          b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.NoShow))));

            CreateMap<UpdateTrainerRequestDTO, Trainer>()
                 .ForMember(d => d.Id, o => o.Ignore())              
                 .ForMember(d => d.WorkoutSessions, o => o.Ignore());

            CreateMap<CreateWorkoutSessionRequestDTO, WorkoutSession>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Trainer, o => o.Ignore())
                .ForMember(d => d.Bookings, o => o.Ignore());

            CreateMap<Booking, BookingDTO>()
                .ForMember(d => d.StartsAt, o => o.MapFrom(s => s.WorkoutSession.StartsAt))
                .ForMember(d => d.EndsAt, o => o.MapFrom(s => s.WorkoutSession.EndsAt))
                .ForMember(d => d.TrainerName, o => o.MapFrom(s => s.WorkoutSession.Trainer!.FullName));
        }
    }
}
