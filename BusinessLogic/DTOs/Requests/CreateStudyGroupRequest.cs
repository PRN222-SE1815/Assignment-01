namespace BusinessLogic.DTOs.Requests;

public class CreateStudyGroupRequest
{
    public string GroupName { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public int CourseId { get; set; } // NEW: Study group linked to course
    public int[]? InvitedUserIds { get; set; }
}