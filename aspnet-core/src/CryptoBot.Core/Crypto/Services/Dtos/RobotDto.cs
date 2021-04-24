using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using CryptoBot.Crypto.Entities;
using System.ComponentModel.DataAnnotations;

namespace CryptoBot.Crypto.Services.Dtos
{
    [AutoMap(typeof(Robot))]
    public class RobotDto : EntityDto<long>
    {
        [Required]
        public bool IsActive { get; set; }

        [Required]
        public long FormulaId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
}
