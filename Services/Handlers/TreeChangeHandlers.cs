using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Changes;
using GiaPha.BusinessObjects.DTOs.Persons;
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

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        var p = PayloadJson.Read<MergePersonsChangePayload>(payload);
        await _personService.MergePersonsAsync(p.ClanId, p.KeepId, p.RemoveId, cancellationToken);
    }
}
