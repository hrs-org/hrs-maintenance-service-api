using AutoMapper;
using FluentAssertions;
using HRS.API.Mappings.Profiles;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Shared.Core.Dtos;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace HRS.Test.API.Mappings.Profiles;

public class ItemMaintenanceProfileTests
{
    private readonly IMapper _mapper;

    public ItemMaintenanceProfileTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<ItemMaintenanceProfile>(); }, loggerFactory);
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Should_Map_ItemMaintenance_To_ItemMaintenanceResponseDto()
    {
        // Arrange
        var entity = new ItemMaintenance
        {
            Id = ObjectId.GenerateNewId(),
            ItemId = "1",
            RentalOrderId = "1",
            Type = ItemMaintenanceType.Broken,
            Quantity = 5,
            QuantityFixed = 0,
            Remarks = "Broken zipper"
        };

        // Act
        var dto = _mapper.Map<ItemMaintenanceResponseDto>(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id.ToString());
        dto.ItemId.Should().Be("1");
        dto.RentalOrderId.Should().Be("1");
        dto.Type.Should().Be(ItemMaintenanceType.Broken.ToString());
        dto.Quantity.Should().Be(5);
        dto.QuantityFixed.Should().Be(0);
        dto.Remarks.Should().Be("Broken zipper");
    }
}
