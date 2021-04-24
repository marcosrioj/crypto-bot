using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
using CryptoBot.Crypto.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoBot.Crypto
{
    [Authorize]
    public class RobotAppService : AsyncCrudAppService<Robot, RobotDto, long>, IRobotAppService
    {
        private readonly ITraderService _traderService;
        private readonly IRepository<Formula, long> _formulaRepository;

        public RobotAppService(
            IRepository<Robot, long> repository,
            IRepository<Formula, long> formulaRepository,
            ITraderService traderService)
            : base(repository)
        {
            _traderService = traderService;
            _formulaRepository = formulaRepository;
        }

        public override async Task<RobotDto> CreateAsync(RobotDto input)
        {
            var robot = await base.CreateAsync(input);

            if (robot.IsActive)
            {
                var formula = await _formulaRepository.GetAsync(input.FormulaId);
                var formulaDto = ObjectMapper.Map<FormulaDto>(formula);

                await _traderService.ScheduleBuyVirtualTrader(robot.Id, robot.UserId, formulaDto);
            }

            return robot;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var robot = await Repository
                .GetAll()
                .AsNoTracking()
                .Where(x => !x.IsActive && x.Id == input.Id)
                .FirstOrDefaultAsync();

            if (robot == null)
            {
                return;
            }

            await base.DeleteAsync(input);

            await _traderService.UnscheduleBuyVirtualTrader(input.Id, robot.UserId, robot.FormulaId);
        }

        public override async Task<RobotDto> UpdateAsync(RobotDto input)
        {
            var robot = await base.UpdateAsync(input);

            await _traderService.UnscheduleBuyVirtualTrader(input.Id, input.UserId, input.FormulaId);

            if (robot.IsActive)
            {
                var formula = await _formulaRepository.GetAsync(input.FormulaId);
                var formulaDto = ObjectMapper.Map<FormulaDto>(formula);

                await _traderService.ScheduleBuyVirtualTrader(robot.Id, robot.UserId, formulaDto);
            }

            return robot;
        }

        public async Task<bool> Disable(long? robotId)
        {
            try
            {
                var robots = await Repository
                    .GetAll()
                    .Where(x => x.IsActive)
                    .WhereIf(robotId.HasValue, x => x.Id == robotId.Value)
                    .ToListAsync();

                foreach (var robot in robots)
                {
                    robot.IsActive = false;

                    await _traderService.UnscheduleBuyVirtualTrader(robot.Id, robot.UserId, robot.FormulaId);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> Enable(long robotId)
        {
            try
            {
                var robot = await Repository
                    .GetAll()
                    .Where(x => !x.IsActive && x.Id == robotId)
                    .FirstOrDefaultAsync();

                if (robot == null)
                {
                    return false;
                }

                robot.IsActive = true;

                var formula = await _formulaRepository.GetAsync(robot.FormulaId);
                var formulaDto = ObjectMapper.Map<FormulaDto>(formula);

                await _traderService.ScheduleBuyVirtualTrader(robot.Id, robot.UserId, formulaDto);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
