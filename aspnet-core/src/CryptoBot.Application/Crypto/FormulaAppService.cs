using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Services.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    [AbpAuthorize]
    public class FormulaAppService : AsyncCrudAppService<Formula, FormulaDto, long>, IFormulaAppService
    {
        private readonly ITraderService _traderService;
        public readonly IRepository<Robot, long> _robotRepository;

        public FormulaAppService(
            IRepository<Formula, long> repository,
            IRepository<Robot, long> robotRepository,
            ITraderService traderService)
            : base(repository)
        {
            _traderService = traderService;
            _robotRepository = robotRepository;
        }

        public override async Task<FormulaDto> CreateAsync(FormulaDto input)
        {
            var formula = await base.CreateAsync(input);

            if (formula.IsActive)
            {
                await _traderService.ScheduleGeneratePredictions(formula);
            }

            return formula;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            await base.DeleteAsync(input);

            await UnscheduleRobots(input.Id);

            await _traderService.UnscheduleGeneratePredictions(input.Id);
        }

        public override async Task<FormulaDto> UpdateAsync(FormulaDto input)
        {
            var formula = await base.UpdateAsync(input);

            await _traderService.UnscheduleGeneratePredictions(input.Id);

            await UnscheduleRobots(input.Id);

            if (formula.IsActive)
            {
                await _traderService.ScheduleGeneratePredictions(formula);

                await ScheduleRobots(formula);
            }

            return formula;
        }

        public async Task<bool> Disable(long? formulaId)
        {
            try
            {
                var formulas = await Repository
                    .GetAll()
                    .Where(x => x.IsActive)
                    .WhereIf(formulaId.HasValue, x => x.Id == formulaId.Value)
                    .ToListAsync();

                foreach (var formula in formulas)
                {
                    formula.IsActive = false;
                    await _traderService.UnscheduleGeneratePredictions(formula.Id);
                    await UnscheduleRobots(formula.Id);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> Enable(long formulaId)
        {
            try
            {
                var formula = await Repository
                    .GetAll()
                    .Where(x => !x.IsActive && x.Id == formulaId)
                    .FirstOrDefaultAsync();

                if (formula == null)
                {
                    return false;
                }

                formula.IsActive = true;
                var formulaDto = MapToEntityDto(formula);

                await _traderService.ScheduleGeneratePredictions(formulaDto);
                await ScheduleRobots(formulaDto);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private async Task UnscheduleRobots(long formulaId)
        {
            var robots = await _robotRepository
                .GetAll()
                .Where(x => x.IsActive && x.FormulaId == formulaId)
                .ToListAsync();

            foreach (var robot in robots)
            {
                await _traderService.UnscheduleBuyVirtualTrader(robot.Id, robot.UserId, robot.FormulaId);
            }
        }

        private async Task ScheduleRobots(FormulaDto formulaDto)
        {
            var robots = await _robotRepository
                .GetAll()
                .Where(x => x.IsActive && x.FormulaId == formulaDto.Id)
                .ToListAsync();

            foreach (var robot in robots)
            {
                var robotDto = ObjectMapper.Map<RobotDto>(robot);

                await _traderService.ScheduleBuyVirtualTrader(robotDto, formulaDto);
            }
        }
    }
}
