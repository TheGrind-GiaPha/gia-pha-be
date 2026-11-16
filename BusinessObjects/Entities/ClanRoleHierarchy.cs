namespace GiaPha.BusinessObjects.Entities;

/// <summary>
/// Kế thừa vai trò theo use case:
///   Biên tập viên (BranchEditor) ⊃ Thành viên (Member)
///   Quản trị họ (ClanAdmin) ⊃ Biên tập viên ⊃ Thành viên
///   Người duyệt (Approver) ⊃ Thành viên
/// Lưu ý: Quản trị họ KHÔNG tự động là Người duyệt; hai nhánh tách riêng.
/// </summary>
public static class ClanRoleHierarchy
{
    public static bool Implies(ClanRole actual, ClanRole required)
    {
        if (actual == required)
        {
            return true;
        }

        return required switch
        {
            // Ai cũng là Thành viên.
            ClanRole.Member => true,

            // Chỉ Quản trị họ mới ngầm là Biên tập viên.
            ClanRole.BranchEditor => actual == ClanRole.ClanAdmin,

            // Người duyệt và Quản trị họ không ngầm chứa nhau.
            _ => false
        };
    }
}
