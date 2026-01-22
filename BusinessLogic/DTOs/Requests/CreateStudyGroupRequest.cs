namespace BusinessLogic.DTOs.Requests;

public class CreateStudyGroupRequest
{
    public string GroupName { get; set; }
    public int CreatedByUserId { get; set; }
    public List<int> InvitedUserIds { get; set; } = new();
}