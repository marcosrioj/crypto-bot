using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Quartz;
using CryptoBot.Crypto.Background.Jobs;
using CryptoBot.Crypto.Services.Dtos;
using CryptoBot.Crypto.Entities;
using CryptoBot.Crypto.Services;
using Microsoft.AspNetCore.Authorization;
using Quartz;
using System;
using System.Threading.Tasks;

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
            _traderService.UnscheduleBuyVirtualTrader(3, input.Id);
        }

        public override async Task<FormulaDto> UpdateAsync(FormulaDto input)
        {
            var formula = await base.UpdateAsync(input);
            await _traderService.UnscheduleGeneratePredictions(input.Id);
            _traderService.UnscheduleBuyVirtualTrader(3, input.Id);

            if (formula.IsActive)
            {
                await _traderService.ScheduleGeneratePredictions(formula);
                await _traderService.ScheduleBuyVirtualTrader(3, formula);
            }

            return formula;
        }
    }
}
