using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IVisitService
    {
        public interface IVisitService
        {
            Task<CheckInResponseDTO> CheckInAsync(Guid userId, CancellationToken ct = default);
            Task<ICollection<VisitDTO>> GetMyVisitsAsync(Guid userId, CancellationToken ct = default);
        }
    }
}
