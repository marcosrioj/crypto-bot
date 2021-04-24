using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CryptoBot.Crypto
{
    [Authorize]
    public class FormulaAppService : AsyncCrudAppService<Formula, FormulaDto, long>, IFormulaAppService
    {
        private readonly ITraderService _traderService;

        public FormulaAppService(
            IRepository<Formula, long> repository,
            ITraderService traderService)
            : base(repository)
        {
            _traderService = traderService;
        }

        public override async Task<FormulaDto> CreateAsync(FormulaDto input)
        {
            var formula = await base.CreateAsync(input);

            if (formula.IsActive)
            {
                await _traderService.ScheduleGeneratePredictions(formula);
                await _traderService.ScheduleBuyVirtualTrader(3, formula);
            }

            return formula;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            await base.DeleteAsync(input);
            await _traderService.UnscheduleGeneratePredictions(input.Id);
            await _traderService.UnscheduleBuyVirtualTrader(3, input.Id);
        }

        public override async Task<FormulaDto> UpdateAsync(FormulaDto input)
        {
            var formula = await base.UpdateAsync(input);

            await _traderService.UnscheduleGeneratePredictions(input.Id);
            await _traderService.UnscheduleBuyVirtualTrader(3, input.Id);

            if (formula.IsActive)
            {
                await _traderService.ScheduleGeneratePredictions(formula);
                await _traderService.ScheduleBuyVirtualTrader(3, formula);
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

                if (formulas.Count > 0)
                {
                    foreach (var formula in formulas)
                    {
                        formula.IsActive = false;
                        await _traderService.UnscheduleGeneratePredictions(formula.Id);
                        await _traderService.UnscheduleBuyVirtualTrader(3, formula.Id);
                    }
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

                formula.IsActive = false;
                var formulaDto = MapToEntityDto(formula);

                await _traderService.ScheduleGeneratePredictions(formulaDto);
                await _traderService.ScheduleBuyVirtualTrader(3, formulaDto);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
