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
        Task<CheckInResponseDTO> AddVisitAsync(Visit visit, CancellationToken ct);
        Task<ICollection<VisitDTO>> GetMyVisitHistory(Visit visit, CancellationToken ct);
    }
}
