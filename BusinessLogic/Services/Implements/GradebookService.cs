using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public sealed class GradebookService : IGradebookService
{
    private readonly IGradebookRepository _gradebookRepository;
    private readonly IGradeItemRepository _gradeItemRepository;
    private readonly IGradeEntryRepository _gradeEntryRepository;
    private readonly IGradeAuditLogRepository _gradeAuditLogRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IClassSectionRepository _classSectionRepository;

    public GradebookService(
        IGradebookRepository gradebookRepository,
        IGradeItemRepository gradeItemRepository,
        IGradeEntryRepository gradeEntryRepository,
        IGradeAuditLogRepository gradeAuditLogRepository,
        IEnrollmentRepository enrollmentRepository,
        IClassSectionRepository classSectionRepository)
    {
        _gradebookRepository = gradebookRepository;
        _gradeItemRepository = gradeItemRepository;
        _gradeEntryRepository = gradeEntryRepository;
        _gradeAuditLogRepository = gradeAuditLogRepository;
        _enrollmentRepository = enrollmentRepository;
        _classSectionRepository = classSectionRepository;
    }

    public async Task<IReadOnlyList<ClassSectionDto>> GetTeacherSectionsAsync(int teacherId)
    {
        var sections = await _classSectionRepository.GetSectionsByTeacherAsync(teacherId);
        return sections.Select(section => new ClassSectionDto
        {
            ClassSectionId = section.ClassSectionId,
            CourseId = section.CourseId,
            CourseCode = section.Course.CourseCode,
            CourseName = section.Course.CourseName,
            SemesterId = section.SemesterId,
            SemesterName = section.Semester.SemesterName,
            SectionCode = section.SectionCode,
            Credits = section.Course.Credits,
            IsOpen = section.IsOpen,
            MaxCapacity = section.MaxCapacity,
            CurrentEnrollment = section.CurrentEnrollment,
            Room = section.Room,
            OnlineUrl = section.OnlineUrl
        }).ToList();
    }

    public async Task<GradebookMatrixDto?> GetGradebookAsync(int classSectionId, int actorUserId)
    {
        if (!await _classSectionRepository.IsTeacherAssignedAsync(classSectionId, actorUserId))
        {
            return null;
        }

        var gradebook = await EnsureGradebookAsync(classSectionId);
        if (gradebook == null)
        {
            return null;
        }

        var items = await _gradeItemRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var entries = await _gradeEntryRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var roster = await _enrollmentRepository.GetRosterBySectionAsync(classSectionId, GetGradebookStatuses());

        var studentRows = roster
            .Select(enrollment => MapStudentRow(enrollment, items, entries))
            .ToList();

        return new GradebookMatrixDto
        {
            Gradebook = MapGradebook(gradebook),
            Items = items.Select(MapItem).ToList(),
            Students = studentRows,
            Entries = entries.Select(MapEntry).ToList(),
            Stats = BuildStats(studentRows)
        };
    }

    public async Task<OperationResult> SaveStructureAsync(int classSectionId, int actorUserId, IReadOnlyList<GradeItemDto> items)
    {
        if (!await _classSectionRepository.IsTeacherAssignedAsync(classSectionId, actorUserId))
        {
            return OperationResult.Failed("You are not assigned to this class section.");
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                return OperationResult.Failed("Grade item name is required.");
            }

            if (item.MaxScore <= 0 || item.MaxScore > 100)
            {
                return OperationResult.Failed("Grade item max score must be between 0 and 100.");
            }

            if (item.Weight.HasValue && (item.Weight.Value < 0 || item.Weight.Value > 1))
            {
                return OperationResult.Failed("Grade item weight must be between 0 and 1.");
            }
        }

        var gradebook = await EnsureGradebookAsync(classSectionId);
        if (gradebook == null)
        {
            return OperationResult.Failed("Gradebook not found.");
        }

        if (gradebook.Status == GradeBookStatus.LOCKED.ToString() || gradebook.Status == GradeBookStatus.ARCHIVED.ToString())
        {
            return OperationResult.Failed("Gradebook is locked and cannot be modified.");
        }

        var existingItems = await _gradeItemRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var existingItemLookup = existingItems.ToDictionary(item => item.GradeItemId);
        var requestedIds = items.Where(item => item.GradeItemId > 0)
            .Select(item => item.GradeItemId)
            .ToHashSet();

        var itemsToDelete = existingItems
            .Where(item => !requestedIds.Contains(item.GradeItemId))
            .ToList();

        if (itemsToDelete.Count > 0)
        {
            var entries = await _gradeEntryRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
            var lockedItemIds = entries.Select(entry => entry.GradeItemId).ToHashSet();
            if (itemsToDelete.Any(item => lockedItemIds.Contains(item.GradeItemId)))
            {
                return OperationResult.Failed("Cannot remove grade items that already have grades.");
            }
        }

        var itemsToAdd = new List<GradeItem>();
        var itemsToUpdate = new List<GradeItem>();

        foreach (var item in items)
        {
            if (item.GradeItemId <= 0)
            {
                itemsToAdd.Add(new GradeItem
                {
                    GradeBookId = gradebook.GradeBookId,
                    ItemName = item.ItemName.Trim(),
                    MaxScore = item.MaxScore,
                    Weight = item.Weight,
                    IsRequired = item.IsRequired,
                    SortOrder = item.SortOrder,
                    CreatedAt = DateTime.UtcNow
                });

                continue;
            }

            if (!existingItemLookup.TryGetValue(item.GradeItemId, out var existingItem))
            {
                return OperationResult.Failed("Grade item not found.");
            }

            existingItem.ItemName = item.ItemName.Trim();
            existingItem.MaxScore = item.MaxScore;
            existingItem.Weight = item.Weight;
            existingItem.IsRequired = item.IsRequired;
            existingItem.SortOrder = item.SortOrder;
            itemsToUpdate.Add(existingItem);
        }

        var timestamp = DateTime.UtcNow;

        await _gradebookRepository.ExecuteInTransactionAsync(async () =>
        {
            if (itemsToDelete.Count > 0)
            {
                await _gradeItemRepository.DeleteRangeAsync(itemsToDelete);
            }

            if (itemsToAdd.Count > 0)
            {
                await _gradeItemRepository.AddRangeAsync(itemsToAdd);
            }

            if (itemsToUpdate.Count > 0)
            {
                await _gradeItemRepository.UpdateRangeAsync(itemsToUpdate);
            }

            gradebook.UpdatedAt = timestamp;
            gradebook.Version += 1;
            await _gradebookRepository.UpdateAsync(gradebook);
        });

        return OperationResult.Ok("Gradebook structure saved.");
    }

    public async Task<OperationResult> SaveGradesAsync(int classSectionId, int actorUserId, IReadOnlyList<GradeEntryCellDto> entries, string? reason = null)
    {
        if (!await _classSectionRepository.IsTeacherAssignedAsync(classSectionId, actorUserId))
        {
            return OperationResult.Failed("You are not assigned to this class section.");
        }

        var gradebook = await EnsureGradebookAsync(classSectionId);
        if (gradebook == null)
        {
            return OperationResult.Failed("Gradebook not found.");
        }

        if (gradebook.Status == GradeBookStatus.LOCKED.ToString() || gradebook.Status == GradeBookStatus.ARCHIVED.ToString())
        {
            return OperationResult.Failed("Gradebook is locked and cannot be modified.");
        }

        var items = await _gradeItemRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var itemIds = items.Select(item => item.GradeItemId).ToHashSet();

        var roster = await _enrollmentRepository.GetRosterBySectionAsync(classSectionId, GetGradebookStatuses());
        var rosterIds = roster.Select(enrollment => enrollment.EnrollmentId).ToHashSet();

        var existingEntries = await _gradeEntryRepository.GetByGradeBookIdAsync(gradebook.GradeBookId);
        var existingEntryLookup = existingEntries.ToDictionary(entry => (entry.GradeItemId, entry.EnrollmentId));

        var entriesToAdd = new List<GradeEntry>();
        var entriesToUpdate = new List<GradeEntry>();
        var auditLogs = new List<GradeAuditLog>();
        var trimmedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        var timestamp = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (!itemIds.Contains(entry.GradeItemId))
            {
                return OperationResult.Failed("Grade item not found.");
            }

            if (!rosterIds.Contains(entry.EnrollmentId))
            {
                return OperationResult.Failed("Enrollment not found for this class section.");
            }

            if (entry.Score.HasValue && (entry.Score.Value < 0 || entry.Score.Value > 10))
            {
                return OperationResult.Failed("Scores must be between 0 and 10.");
            }

            if (existingEntryLookup.TryGetValue((entry.GradeItemId, entry.EnrollmentId), out var existingEntry))
            {
                if (existingEntry.Score != entry.Score)
                {
                    var oldScore = existingEntry.Score;
                    existingEntry.Score = entry.Score;
                    existingEntry.UpdatedBy = actorUserId;
                    existingEntry.UpdatedAt = timestamp;
                    entriesToUpdate.Add(existingEntry);

                    auditLogs.Add(new GradeAuditLog
                    {
                        GradeEntryId = existingEntry.GradeEntryId,
                        ActorUserId = actorUserId,
                        OldScore = oldScore,
                        NewScore = entry.Score,
                        Reason = trimmedReason,
                        CreatedAt = timestamp
                    });
                }

                continue;
            }

            entriesToAdd.Add(new GradeEntry
            {
                GradeItemId = entry.GradeItemId,
                EnrollmentId = entry.EnrollmentId,
                Score = entry.Score,
                UpdatedBy = actorUserId,
                UpdatedAt = timestamp
            });
        }

        if (entriesToAdd.Count == 0 && entriesToUpdate.Count == 0)
        {
            return OperationResult.Ok("No grade changes detected.");
        }

        await _gradebookRepository.ExecuteInTransactionAsync(async () =>
        {
            if (entriesToAdd.Count > 0)
            {
                await _gradeEntryRepository.AddRangeAsync(entriesToAdd);
                auditLogs.AddRange(entriesToAdd.Select(entry => new GradeAuditLog
                {
                    GradeEntryId = entry.GradeEntryId,
                    ActorUserId = actorUserId,
                    OldScore = null,
                    NewScore = entry.Score,
                    Reason = trimmedReason,
                    CreatedAt = timestamp
                }));
            }

            if (entriesToUpdate.Count > 0)
            {
                await _gradeEntryRepository.UpdateRangeAsync(entriesToUpdate);
            }

            if (auditLogs.Count > 0)
            {
                await _gradeAuditLogRepository.AddRangeAsync(auditLogs);
            }

            gradebook.UpdatedAt = timestamp;
            gradebook.Version += 1;
            await _gradebookRepository.UpdateAsync(gradebook);
        });

        return OperationResult.Ok("Grades saved successfully.");
    }

    public async Task<OperationResult> PublishAsync(int classSectionId, int actorUserId)
    {
        return await UpdateStatusAsync(classSectionId, actorUserId, GradeBookStatus.PUBLISHED);
    }

    public async Task<OperationResult> LockAsync(int classSectionId, int actorUserId)
    {
        return await UpdateStatusAsync(classSectionId, actorUserId, GradeBookStatus.LOCKED);
    }

    public async Task<OperationResult> ArchiveAsync(int classSectionId, int actorUserId)
    {
        return await UpdateStatusAsync(classSectionId, actorUserId, GradeBookStatus.ARCHIVED);
    }

    private async Task<OperationResult> UpdateStatusAsync(int classSectionId, int actorUserId, GradeBookStatus targetStatus)
    {
        if (!await _classSectionRepository.IsTeacherAssignedAsync(classSectionId, actorUserId))
        {
            return OperationResult.Failed("You are not assigned to this class section.");
        }

        var gradebook = await EnsureGradebookAsync(classSectionId);
        if (gradebook == null)
        {
            return OperationResult.Failed("Gradebook not found.");
        }

        if (gradebook.Status == GradeBookStatus.ARCHIVED.ToString())
        {
            return OperationResult.Failed("Gradebook is archived.");
        }

        if (targetStatus == GradeBookStatus.LOCKED && gradebook.Status == GradeBookStatus.LOCKED.ToString())
        {
            return OperationResult.Ok("Gradebook is already locked.");
        }

        if (targetStatus == GradeBookStatus.PUBLISHED && gradebook.Status == GradeBookStatus.PUBLISHED.ToString())
        {
            return OperationResult.Ok("Gradebook is already published.");
        }

        var timestamp = DateTime.UtcNow;
        gradebook.Status = targetStatus.ToString();
        gradebook.UpdatedAt = timestamp;
        gradebook.Version += 1;

        if (targetStatus == GradeBookStatus.PUBLISHED)
        {
            gradebook.PublishedAt ??= timestamp;
        }

        if (targetStatus == GradeBookStatus.LOCKED)
        {
            gradebook.LockedAt ??= timestamp;
        }

        if (targetStatus == GradeBookStatus.ARCHIVED)
        {
            gradebook.LockedAt ??= timestamp;
        }

        await _gradebookRepository.UpdateAsync(gradebook);
        return OperationResult.Ok("Gradebook status updated.");
    }

    private async Task<GradeBook?> EnsureGradebookAsync(int classSectionId)
    {
        var gradebook = await _gradebookRepository.GetByClassSectionIdAsync(classSectionId);
        if (gradebook != null)
        {
            return gradebook;
        }

        var classSection = await _classSectionRepository.GetSectionDetailAsync(classSectionId);
        if (classSection == null)
        {
            return null;
        }

        var newGradebook = new GradeBook
        {
            ClassSectionId = classSectionId,
            Status = GradeBookStatus.DRAFT.ToString(),
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _gradebookRepository.AddAsync(newGradebook);
        return await _gradebookRepository.GetByClassSectionIdAsync(classSectionId);
    }

    private static IReadOnlyList<string> GetGradebookStatuses()
    {
        return new[]
        {
            EnrollmentStatus.ENROLLED.ToString(),
            EnrollmentStatus.WITHDRAWN.ToString(),
            EnrollmentStatus.COMPLETED.ToString()
        };
    }

    private static GradebookDto MapGradebook(GradeBook gradebook)
    {
        return new GradebookDto
        {
            GradeBookId = gradebook.GradeBookId,
            ClassSectionId = gradebook.ClassSectionId,
            CourseId = gradebook.ClassSection.CourseId,
            CourseCode = gradebook.ClassSection.Course.CourseCode,
            CourseName = gradebook.ClassSection.Course.CourseName,
            SectionCode = gradebook.ClassSection.SectionCode,
            SemesterId = gradebook.ClassSection.SemesterId,
            SemesterName = gradebook.ClassSection.Semester.SemesterName,
            Status = gradebook.Status,
            Version = gradebook.Version,
            PublishedAt = gradebook.PublishedAt,
            LockedAt = gradebook.LockedAt,
            CreatedAt = gradebook.CreatedAt,
            UpdatedAt = gradebook.UpdatedAt
        };
    }

    private static GradeItemDto MapItem(GradeItem item)
    {
        return new GradeItemDto
        {
            GradeItemId = item.GradeItemId,
            GradeBookId = item.GradeBookId,
            ItemName = item.ItemName,
            MaxScore = item.MaxScore,
            Weight = item.Weight,
            IsRequired = item.IsRequired,
            SortOrder = item.SortOrder,
            CreatedAt = item.CreatedAt
        };
    }

    private static GradeEntryCellDto MapEntry(GradeEntry entry)
    {
        return new GradeEntryCellDto
        {
            GradeEntryId = entry.GradeEntryId,
            GradeItemId = entry.GradeItemId,
            EnrollmentId = entry.EnrollmentId,
            Score = entry.Score,
            UpdatedBy = entry.UpdatedBy,
            UpdatedAt = entry.UpdatedAt
        };
    }

    private static StudentRowDto MapStudentRow(Enrollment enrollment, IReadOnlyList<GradeItem> items, IReadOnlyList<GradeEntry> entries)
    {
        var studentEntries = entries
            .Where(entry => entry.EnrollmentId == enrollment.EnrollmentId)
            .ToList();

        return new StudentRowDto
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            StudentCode = enrollment.Student.StudentCode,
            StudentName = enrollment.Student.StudentNavigation.FullName,
            FinalScore = CalculateFinalScore(items, studentEntries)
        };
    }

    private static decimal? CalculateFinalScore(IReadOnlyList<GradeItem> items, IReadOnlyList<GradeEntry> entries)
    {
        if (items.Count == 0)
        {
            return null;
        }

        var weightedItems = items.Where(item => item.Weight.HasValue).ToList();
        var hasWeights = weightedItems.Count > 0;
        var entryLookup = entries
            .Where(entry => entry.Score.HasValue)
            .ToDictionary(entry => entry.GradeItemId, entry => entry);

        decimal total = 0m;
        decimal weightSum = 0m;
        var scoredCount = 0;

        foreach (var item in items)
        {
            if (!entryLookup.TryGetValue(item.GradeItemId, out var entry))
            {
                continue;
            }

            var normalized = item.MaxScore > 0
                ? (entry.Score!.Value / item.MaxScore) * 10m
                : entry.Score!.Value;

            if (hasWeights)
            {
                if (!item.Weight.HasValue)
                {
                    continue;
                }

                total += normalized * item.Weight.Value;
                weightSum += item.Weight.Value;
            }
            else
            {
                total += normalized;
                scoredCount++;
            }
        }

        if (hasWeights)
        {
            if (weightSum <= 0)
            {
                return null;
            }

            return Math.Round(total / weightSum, 2);
        }

        if (scoredCount == 0)
        {
            return null;
        }

        return Math.Round(total / scoredCount, 2);
    }

    private static StatsDto BuildStats(IReadOnlyList<StudentRowDto> students)
    {
        var histogram = Enumerable.Repeat(0, 11).ToArray();
        var above = 0;
        var below = 0;
        var notGraded = 0;

        foreach (var student in students)
        {
            if (!student.FinalScore.HasValue)
            {
                notGraded++;
                continue;
            }

            var score = Math.Clamp(student.FinalScore.Value, 0m, 10m);
            var bucket = (int)Math.Floor(score);
            histogram[bucket]++;

            if (score >= 5m)
            {
                above++;
            }
            else
            {
                below++;
            }
        }

        return new StatsDto
        {
            Histogram = histogram,
            AboveCount = above,
            BelowCount = below,
            NotGradedCount = notGraded
        };
    }
}
