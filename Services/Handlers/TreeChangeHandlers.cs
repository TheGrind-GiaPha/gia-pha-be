using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Handlers;

internal static class PayloadJson
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static T Read<T>(string payload)
        => JsonSerializer.Deserialize<T>(payload, Options)
           ?? throw new ArgumentException("Payload thay đổi không hợp lệ.", nameof(payload));
}

/// <summary>Handler cho ChangeRequest.Type = AddPerson (thêm 1 người con).</summary>
public sealed class AddPersonHandler : IChangeHandler
{
    private readonly IPersonService _personService;

    public AddPersonHandler(IPersonService personService) => _personService = personService;

    public string Type => "AddPerson";

    public async Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<AddPersonChangePayload>(payload);
        var father = await _personService.GetByIdAsync(p.ClanId, p.FatherId, cancellationToken);
        var relation = p.RelationType switch
        {
            RelationType.Adopted => "con nuôi",
            RelationType.Heir => "con thừa tự",
            _ => "con"
        };

        return father is null
            ? $"Thêm {relation}: {p.FullName}."
            : $"Thêm {relation}: {p.FullName} cho {father.FullName} (đời {father.Generation}).";
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<AddPersonChangePayload>(payload);

        await _personService.AddChildrenAsync(p.ClanId, p.FatherId, new AddChildrenRequest
        {
            Children =
            {
                new AddChildItem
                {
                    FullName = p.FullName,
                    Gender = p.Gender,
                    BirthDate = p.BirthDate,
                    RelationType = p.RelationType
                }
            }
        }, cancellationToken);
    }
}

/// <summary>Handler cho ChangeRequest.Type = MoveSubtree.</summary>
public sealed class MoveSubtreeHandler : IChangeHandler
{
    private readonly IPersonService _personService;

    public MoveSubtreeHandler(IPersonService personService) => _personService = personService;

    public string Type => "MoveSubtree";

    public async Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<MoveSubtreeChangePayload>(payload);
        var person = await _personService.GetByIdAsync(p.ClanId, p.PersonId, cancellationToken);
        var father = await _personService.GetByIdAsync(p.ClanId, p.NewFatherId, cancellationToken);
        return $"Di chuyển {person?.FullName ?? "một người"} sang làm con của {father?.FullName ?? "người mới"}.";
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<MoveSubtreeChangePayload>(payload);
        await _personService.MoveSubtreeAsync(p.ClanId, p.PersonId, p.NewFatherId, cancellationToken);
    }
}

/// <summary>Handler cho ChangeRequest.Type = DeletePerson.</summary>
public sealed class DeletePersonHandler : IChangeHandler
{
    private readonly IPersonService _personService;

    public DeletePersonHandler(IPersonService personService) => _personService = personService;

    public string Type => "DeletePerson";

    public async Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<DeletePersonChangePayload>(payload);
        var person = await _personService.GetByIdAsync(p.ClanId, p.PersonId, cancellationToken);
        return $"Xóa người {person?.FullName ?? "(không rõ)"} (cách xóa: {p.Strategy}).";
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<DeletePersonChangePayload>(payload);
        await _personService.DeletePersonAsync(p.ClanId, p.PersonId, p.Strategy, p.NewFatherId, cancellationToken);
    }
}

/// <summary>Handler cho ChangeRequest.Type = MergePersons.</summary>
public sealed class MergePersonsHandler : IChangeHandler
{
    private readonly IPersonService _personService;

    public MergePersonsHandler(IPersonService personService) => _personService = personService;

    public string Type => "MergePersons";

    public async Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<MergePersonsChangePayload>(payload);
        var keep = await _personService.GetByIdAsync(p.ClanId, p.KeepId, cancellationToken);
        var remove = await _personService.GetByIdAsync(p.ClanId, p.RemoveId, cancellationToken);
        return $"Gộp {remove?.FullName ?? "người trùng"} vào {keep?.FullName ?? "người giữ lại"}.";
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<MergePersonsChangePayload>(payload);
        await _personService.MergePersonsAsync(p.ClanId, p.KeepId, p.RemoveId, cancellationToken);
    }
}

/// <summary>Handler cho ChangeRequest.Type = AddUnion (thêm vợ/chồng).</summary>
public sealed class AddUnionHandler : IChangeHandler
{
    private readonly IPersonService _personService;

    public AddUnionHandler(IPersonService personService) => _personService = personService;

    public string Type => "AddUnion";

    public async Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<AddUnionChangePayload>(payload);
        var husband = await _personService.GetByIdAsync(p.ClanId, p.HusbandId, cancellationToken);
        var wife = await _personService.GetByIdAsync(p.ClanId, p.WifeId, cancellationToken);
        return $"Thêm vợ/chồng: {wife?.FullName ?? "người mới"} cho {husband?.FullName ?? "người chồng"} (thứ {p.Rank}).";
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<AddUnionChangePayload>(payload);
        await _personService.AddUnionAsync(p.ClanId, p.HusbandId, new AddUnionRequest
        {
            WifeId = p.WifeId,
            Rank = p.Rank
        }, cancellationToken);
    }
}
