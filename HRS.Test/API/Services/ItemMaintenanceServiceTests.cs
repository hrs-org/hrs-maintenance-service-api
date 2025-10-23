using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Enums;
using HRS.Shared.Core.Interfaces;
using MongoDB.Bson;
using NSubstitute;
using Xunit;

namespace HRS.Test.API.Services;

public class ItemMaintenanceServiceTests
{
    private readonly IItemMaintenanceRepository _repo = Substitute.For<IItemMaintenanceRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly IUserContextService _userCtx = Substitute.For<IUserContextService>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private FakeHttpMessageHandler _fakeHandler = new();
    private HttpClient _httpClient;
    private readonly ItemMaintenanceService _svc;

    public ItemMaintenanceServiceTests()
    {
        _httpClient = new HttpClient(_fakeHandler);
        _httpClient.BaseAddress = new Uri("http://localhost/");
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_httpClient);

        _userCtx.GetUserAsync().Returns(Task.FromResult(new UserResponseDto
        {
            Id = 10,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Role = "Employee"
        }));
        _userCtx.GetUserId().Returns(10);
        _userCtx.GetStoreId().Returns(1);
        _mapper.Map<ItemMaintenanceResponseDto>(Arg.Any<ItemMaintenance>()).Returns(x => new ItemMaintenanceResponseDto { Id = "test", ItemId = (x.Arg<ItemMaintenance>()?.ItemId) ?? "", Quantity = (x.Arg<ItemMaintenance>()?.Quantity ?? 0) });
        _svc = new ItemMaintenanceService(_repo, _userCtx, _mapper, _httpClientFactory);
    }

    // 用于注入不同响应的自定义 handler
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? SendAsyncFunc { get; set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (SendAsyncFunc != null)
                return SendAsyncFunc(request, cancellationToken);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsDto()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), ItemId = "1", RentalOrderId = "r1" };
        var dto = new ItemMaintenanceResponseDto { Id = "1" };
        _repo.GetByIdAsync("1").Returns(Task.FromResult<ItemMaintenance?>(record));
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);
        var result = await _svc.GetAsync("1");
        result.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetAsync_NotFound_Throws()
    {
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(null));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.GetAsync("1"));
    }

    [Fact]
    public async Task GetByItemIdAsync_ReturnsDtos()
    {
        var records = new List<ItemMaintenance> { new() { Id = ObjectId.GenerateNewId(), ItemId = "1", RentalOrderId = "r1" } };
        var dtos = new List<ItemMaintenanceResponseDto> { new() { Id = "1" } };
        _repo.GetByItemIdAsync("1").Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(records));
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records).Returns(dtos);
        var result = await _svc.GetByItemIdAsync("1");
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ReturnsInt()
    {
        _repo.GetRepairingQuantityAsync("item1").Returns(Task.FromResult(7));
        var result = await _svc.GetRepairingQuantityAsync("item1");
        result.Should().Be(7);
    }

    [Fact]
    public async Task GetByStoreIdAsync_ReturnsDtos()
    {
        var records = new List<ItemMaintenance> { new() { Id = ObjectId.GenerateNewId(), StoreId = 1, ItemId = "1", RentalOrderId = "r1" } };
        var dtos = new List<ItemMaintenanceResponseDto> { new() { Id = "1" } };
        _repo.GetByStoreIdAsync(1).Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(records));
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records).Returns(dtos);
        var result = await _svc.GetByStoreIdAsync(1);
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDtos()
    {
        var records = new List<ItemMaintenance> { new() { Id = ObjectId.GenerateNewId(), ItemId = "1", RentalOrderId = "r1" } };
        var dtos = new List<ItemMaintenanceResponseDto> { new() { Id = "1" } };
        _repo.GetAllAsync().Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(records));
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records).Returns(dtos);
        var result = await _svc.GetAllAsync();
        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task AddAsync_Valid_ReturnsDto()
    {
        var req = new CreateItemMaintenanceRequestDto { ItemId = "item1", Quantity = 2, RentalOrderId = "order1", Remarks = "test" };
        var expected = new ItemMaintenanceResponseDto { Id = "id", ItemId = "item1", Quantity = 2, Remarks = "test" };
        _mapper.Map<ItemMaintenanceResponseDto>(Arg.Any<ItemMaintenance>()).Returns(expected);
        var result = await _svc.AddAsync(req);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddAsync_QuantityInvalid_DoesNotThrow(int qty)
    {
        var req = new CreateItemMaintenanceRequestDto { ItemId = "item1", Quantity = qty, RentalOrderId = "order1" };
        var result = await _svc.AddAsync(req);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_UserIdOrStoreIdZero_DoesNotThrow()
    {
        _userCtx.GetUserId().Returns(0);
        _userCtx.GetStoreId().Returns(0);
        var req = new CreateItemMaintenanceRequestDto { ItemId = "item1", Quantity = 1, RentalOrderId = "order1" };
        var result = await _svc.AddAsync(req);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkAsFixedAsync_Valid_UpdatesAndReturnsDto()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Repair, Quantity = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 3, Remarks = "Fixed" };
        var dto = new ItemMaintenanceResponseDto { Id = "1" };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);
        var result = await _svc.MarkAsFixedAsync(req);
        result.Should().BeEquivalentTo(dto);
        record.Remarks.Should().Be("Fixed");
        record.UpdatedById.Should().Be(10);
    }

    [Fact]
    public async Task MarkAsFixedAsync_NotFound_Throws()
    {
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 1 };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(null));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _svc.MarkAsFixedAsync(req));
    }

    [Fact]
    public async Task MarkAsFixedAsync_TypeNotRepair_Throws()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Fixed, Quantity = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 1 };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.MarkAsFixedAsync(req));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task MarkAsFixedAsync_QuantityInvalid_Throws(int qty)
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Repair, Quantity = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = qty };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.MarkAsFixedAsync(req));
    }

    [Fact]
    public async Task MarkAsFixedAsync_QuantityExceeds_Throws()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Repair, Quantity = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 10 };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        await Assert.ThrowsAsync<ArgumentException>(() => _svc.MarkAsFixedAsync(req));
    }

    [Fact]
    public async Task MarkAsFixedAsync_RemarksNull_SetsDefault()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Repair, Quantity = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 2, Remarks = null };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(new ItemMaintenanceResponseDto { Id = "1" });
        var result = await _svc.MarkAsFixedAsync(req);
        result.Should().NotBeNull();
        record.Remarks.Should().Contain("Marked as fixed on");
    }

    [Fact]
    public async Task AddBatchAsync_Valid_ReturnsDtos()
    {
        var batch = new CreateItemMaintenanceBatchRequestDto
        {
            Entries = new List<CreateItemMaintenanceRequestDto>
            {
                new() { ItemId = "item1", Type = ItemMaintenanceType.Repair, RentalOrderId = "r1", Quantity = 2, Remarks = "r" },
                new() { ItemId = "item2", Type = ItemMaintenanceType.Broken, RentalOrderId = "r2", Quantity = 1, Remarks = "b" }
            }
        };
        var expected = new List<ItemMaintenanceResponseDto> { new() { Id = "1" }, new() { Id = "2" } };
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(Arg.Any<IEnumerable<ItemMaintenance>>()).Returns(expected);
        _fakeHandler.SendAsyncFunc = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        var result = await _svc.AddBatchAsync(batch);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AddBatchAsync_ExternalServiceFails_Throws()
    {
        var batch = new CreateItemMaintenanceBatchRequestDto
        {
            Entries = new List<CreateItemMaintenanceRequestDto>
            {
                new() { ItemId = "item2", Type = ItemMaintenanceType.Broken, RentalOrderId = "r2", Quantity = 1, Remarks = "b" }
            }
        };
        _fakeHandler.SendAsyncFunc = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.AddBatchAsync(batch));
    }

    [Fact]
    public async Task AddBatchAsync_EmptyEntries_ReturnsEmpty()
    {
        var batch = new CreateItemMaintenanceBatchRequestDto { Entries = new List<CreateItemMaintenanceRequestDto>() };
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(Arg.Any<IEnumerable<ItemMaintenance>>()).Returns(new List<ItemMaintenanceResponseDto>());
        var result = await _svc.AddBatchAsync(batch);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddBatchAsync_LostType_AdjustQuantity()
    {
        var batch = new CreateItemMaintenanceBatchRequestDto
        {
            Entries = new List<CreateItemMaintenanceRequestDto>
            {
                new() { ItemId = "item3", Type = ItemMaintenanceType.Lost, RentalOrderId = "r3", Quantity = 2, Remarks = "lost" }
            }
        };
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(Arg.Any<IEnumerable<ItemMaintenance>>()).Returns(new List<ItemMaintenanceResponseDto> { new() { Id = "3" } });
        _fakeHandler.SendAsyncFunc = (req, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        var result = await _svc.AddBatchAsync(batch);
        result.Should().NotBeNull();
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task AddBatchAsync_MultipleBrokenLost_CompensationOnFail()
    {
        var batch = new CreateItemMaintenanceBatchRequestDto
        {
            Entries = new List<CreateItemMaintenanceRequestDto>
            {
                new() { ItemId = "item4", Type = ItemMaintenanceType.Broken, RentalOrderId = "r4", Quantity = 1, Remarks = "broken" },
                new() { ItemId = "item5", Type = ItemMaintenanceType.Lost, RentalOrderId = "r5", Quantity = 1, Remarks = "lost" }
            }
        };
        int callCount = 0;
        _fakeHandler.SendAsyncFunc = (req, ct) =>
        {
            callCount++;
            // 第一次成功，第二次失败
            return Task.FromResult(new HttpResponseMessage(callCount == 1 ? HttpStatusCode.OK : HttpStatusCode.BadRequest));
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.AddBatchAsync(batch));
    }

    [Fact]
    public async Task MarkAsFixedAsync_AlreadyFixed_Throws()
    {
        var record = new ItemMaintenance { Id = ObjectId.GenerateNewId(), Type = ItemMaintenanceType.Fixed, Quantity = 5, QuantityFixed = 5, ItemId = "1", RentalOrderId = "1" };
        var req = new FixItemMaintenanceRequestDto { Id = "1", QuantityFixed = 1 };
        _repo.GetByIdAsync(Arg.Any<string>()).Returns(Task.FromResult<ItemMaintenance?>(record));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.MarkAsFixedAsync(req));
    }
}
