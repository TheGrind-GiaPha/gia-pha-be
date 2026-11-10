using GiaPha.BusinessObjects.DTOs.Clans;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Implementations;

public sealed class ClanService : IClanService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public ClanService(IRepositoryManager repository, ILoggerManager logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClanDto>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var clans = await _repository.Clan.GetAllAsync(
            search,
            trackChanges: false,
            cancellationToken);

        return clans.Select(Map).ToList();
    }

    public async Task<ClanDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(
            id,
            trackChanges: false,
            cancellationToken);

        return clan is null ? null : Map(clan);
    }

    public async Task<ClanDto> CreateAsync(
        CreateClanRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(request.Name);

        if (await _repository.Clan.ExistsByNameAsync(name, cancellationToken))
        {
            throw new ConflictException($"Dòng họ '{name}' đã tồn tại.");
        }

        var clan = new Clan
        {
            Name = name,
            Description = NormalizeDescription(request.Description)
        };

        _repository.Clan.Create(clan);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã tạo dòng họ '{clan.Name}' với id {clan.Id}.");
        return Map(clan);
    }

    public async Task<ClanDto?> UpdateAsync(
        Guid id,
        UpdateClanRequest request,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(
            id,
            trackChanges: true,
            cancellationToken);

        if (clan is null)
        {
            return null;
        }

        var name = NormalizeName(request.Name);
        var isNameChanged = !string.Equals(clan.Name, name, StringComparison.OrdinalIgnoreCase);

        if (isNameChanged && await _repository.Clan.ExistsByNameAsync(name, cancellationToken))
        {
            throw new ConflictException($"Dòng họ '{name}' đã tồn tại.");
        }

        clan.Name = name;
        clan.Description = NormalizeDescription(request.Description);
        clan.UpdatedAt = DateTime.UtcNow;

        _repository.Clan.Update(clan);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã cập nhật dòng họ {clan.Id}.");
        return Map(clan);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(
            id,
            trackChanges: true,
            cancellationToken);

        if (clan is null)
        {
            return false;
        }

        _repository.Clan.Delete(clan);
        await _repository.SaveAsync(cancellationToken);

        _logger.LogWarn($"Đã xóa dòng họ {clan.Id} ('{clan.Name}').");
        return true;
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Tên dòng họ không được để trống.", nameof(name));
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
        => string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static ClanDto Map(Clan clan)
        => new(clan.Id, clan.Name, clan.Description, clan.CreatedAt, clan.UpdatedAt);
}
