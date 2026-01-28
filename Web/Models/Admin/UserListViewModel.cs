using System;
using System.Collections.Generic;

namespace Web.Models.Admin;

public sealed class UserListViewModel
{
    public IReadOnlyList<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
    public string? Role { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
