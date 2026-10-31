using AutoMapper;
using FitZone.Application.ServiceInterfaces;
using FitZone.Application.DTOs.TrainerDTOs;
using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FitZone.Testing.ServiceTests
{
    public class TrainerServiceTests
    {
        private readonly Mock<ITrainerRepository> _repo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly IMapper _mapper;
        private readonly TrainerService _service;
        private readonly Mock<IEmailSender> _email = new();

        private const string AdminEmail = "admin@test.com";
        public TrainerServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
             {
                 cfg.CreateMap<Trainer, TrainerDTO>();
                 cfg.CreateMap<CreateTrainerRequestDTO, Trainer>();
                 cfg.CreateMap<UpdateTrainerRequestDTO, Trainer>();
             },
                    NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();

            _service = new TrainerService(
                    _repo.Object, _mapper, _uow.Object,
                    NullLogger<TrainerService>.Instance,
                    _email.Object);
        }

        private static Trainer NewTrainer(string name = "Nino Kapanadze", string spec = "Yoga", int years = 5) => new()
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Specialization = spec,
            ExperienceYears = years
        };

        private void SetupGetById(Guid id, Trainer? result) =>
            _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(result);


        [Fact]
        public async Task AddAsync_Should_Add_Save_And_Return_Dto()
        {
            Trainer? added = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Trainer>(), It.IsAny<CancellationToken>()))
                 .Callback<Trainer, CancellationToken>((t, _) => added = t)
                 .Returns(Task.CompletedTask);

            var request = new CreateTrainerRequestDTO
            {
                FullName = "Giorgi Beridze",
                Specialization = "Strength",
                ExperienceYears = 8
            };

            var result=await  _service.AddAsync(request, AdminEmail);

            added.Should().NotBeNull();
            result.FullName.Should().Be("Giorgi Beridze");
            result.ExperienceYears.Should().Be(8);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_When_Request_Is_Null()
        {
            var act = () => _service.AddAsync(null!, AdminEmail);

            await act.Should().ThrowAsync<ArgumentNullException>();
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_When_Trainer_Exists()
        {
            var trainer = NewTrainer();
            SetupGetById(trainer.Id, trainer);

            var result = await _service.GetByIdAsync(trainer.Id);

            result.Id.Should().Be(trainer.Id);
            result.FullName.Should().Be(trainer.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_Trainer_Not_Found()
        {
            var id = Guid.NewGuid();
            SetupGetById(id, null);

            var act = () => _service.GetByIdAsync(id);

            await act.Should().ThrowAsync<TrainersDoNotFoundException>();
        }


        [Fact]
        public async Task UpdateAsync_Should_Change_Fields_And_Save()
        {
            var trainer = NewTrainer("Old Name", "Yoga", 2);
            SetupGetById(trainer.Id, trainer);

            var request = new UpdateTrainerRequestDTO
            {
                FullName = "New Name",
                Specialization = "Cardio",
                ExperienceYears = 7
            };

            await _service.UpdateAsync(trainer.Id, request, AdminEmail);

            trainer.FullName.Should().Be("New Name");
            trainer.Specialization.Should().Be("Cardio");
            trainer.ExperienceYears.Should().Be(7);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

       

        [Fact]
        public async Task UpdateAsync_Should_Throw_And_Save_Nothing_When_Not_Found()
        {
            var id = Guid.NewGuid();
            SetupGetById(id, null);

            var act = () => _service.UpdateAsync(id, new UpdateTrainerRequestDTO
            {
                FullName = "X",
                Specialization = "Y",
                ExperienceYears = 1
            }, AdminEmail);

            await act.Should().ThrowAsync<TrainersDoNotFoundException>();
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task RemoveAsync_Should_Remove_And_Save()
        {
            var trainer = NewTrainer();
            SetupGetById(trainer.Id, trainer);

            await _service.RemoveAsync(trainer.Id, AdminEmail);

            _repo.Verify(r => r.Remove(trainer), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_Should_Throw_And_Remove_Nothing_When_Not_Found()
        {
            var id = Guid.NewGuid();
            SetupGetById(id, null);

            var act = () => _service.RemoveAsync(id, AdminEmail);

            await act.Should().ThrowAsync<TrainersDoNotFoundException>();
            _repo.Verify(r => r.Remove(It.IsAny<Trainer>()), Times.Never);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }


        [Fact]
        public async Task SearchAsync_Should_Trim_Values_Before_Querying()
        {
            _repo.Setup(r => r.SearchAsync("Nino", "Yoga", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Trainer> { NewTrainer() });

            var result = await _service.SearchAsync("  Nino  ", "  Yoga ");

            result.Should().HaveCount(1);
            _repo.Verify(r => r.SearchAsync("Nino", "Yoga", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchAsync_Should_Pass_Null_When_Value_Is_Empty(string? value)
        {
            _repo.Setup(r => r.SearchAsync(null, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Trainer>());

            await _service.SearchAsync(value, value);

            _repo.Verify(r => r.SearchAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_Should_Return_Empty_List_When_Nothing_Found()
        {
            _repo.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Trainer>());

            var result = await _service.SearchAsync("not exists", null);

            result.Should().BeEmpty();
        }
    }
}