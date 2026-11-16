using GiaPha.BusinessObjects.Entities;

namespace GiaPha.UnitTests.Services;

public sealed class ClanRoleHierarchyTests
{
    [Theory]
    // Quản trị họ ⊃ Biên tập viên ⊃ Thành viên.
    [InlineData(ClanRole.ClanAdmin, ClanRole.ClanAdmin, true)]
    [InlineData(ClanRole.ClanAdmin, ClanRole.BranchEditor, true)]
    [InlineData(ClanRole.ClanAdmin, ClanRole.Member, true)]
    [InlineData(ClanRole.ClanAdmin, ClanRole.Approver, false)]
    // Biên tập viên ⊃ Thành viên.
    [InlineData(ClanRole.BranchEditor, ClanRole.BranchEditor, true)]
    [InlineData(ClanRole.BranchEditor, ClanRole.Member, true)]
    [InlineData(ClanRole.BranchEditor, ClanRole.ClanAdmin, false)]
    [InlineData(ClanRole.BranchEditor, ClanRole.Approver, false)]
    // Người duyệt ⊃ Thành viên (nhánh riêng, không chứa Biên tập viên).
    [InlineData(ClanRole.Approver, ClanRole.Approver, true)]
    [InlineData(ClanRole.Approver, ClanRole.Member, true)]
    [InlineData(ClanRole.Approver, ClanRole.BranchEditor, false)]
    [InlineData(ClanRole.Approver, ClanRole.ClanAdmin, false)]
    // Thành viên là gốc.
    [InlineData(ClanRole.Member, ClanRole.Member, true)]
    [InlineData(ClanRole.Member, ClanRole.BranchEditor, false)]
    [InlineData(ClanRole.Member, ClanRole.Approver, false)]
    [InlineData(ClanRole.Member, ClanRole.ClanAdmin, false)]
    public void Implies_MatchesUseCaseHierarchy(ClanRole actual, ClanRole required, bool expected)
        => Assert.Equal(expected, ClanRoleHierarchy.Implies(actual, required));
}
