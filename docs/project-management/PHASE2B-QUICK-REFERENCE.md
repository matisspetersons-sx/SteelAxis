# Phase 2B - Quick Reference Guide

## 🎯 What Was Implemented

**Advanced Project Task Management** with Critical Path Method (CPM) algorithm, supporting:
- ✅ Task hierarchy and dependencies
- ✅ Critical path calculations
- ✅ Schedule impact analysis
- ✅ Gantt chart data export
- ✅ Real-time UI updates

## 📍 Key Files

| File | Purpose | Location |
|------|---------|----------|
| `ProjectTasks.razor` | Main task management page | `Manimp.Web/Components/Pages/` |
| `TaskFormDialog.razor` | Create/edit task dialog | `Manimp.Web/Components/Dialogs/` |
| `TaskDependencyDialog.razor` | Manage task dependencies | `Manimp.Web/Components/Dialogs/` |
| `ProjectTaskController.cs` | REST API endpoints | `Manimp.Api/Controllers/` |
| `ProjectTaskService.cs` | Business logic & CPM algorithm | `Manimp.Services/Implementation/` |
| `ProjectTaskHttpService.cs` | API client wrapper | `Manimp.Web/Services/` |

## 🚀 Accessing the Feature

### URL
```
/projects/{projectId}/tasks
```

### Example
```
/projects/1/tasks
```

### Feature Flag
```csharp
FeatureKeys.TaskDependencies
```

## 📝 Usage Examples

### Creating a Task
1. Navigate to `/projects/{projectId}/tasks`
2. Click **"Add Task"** button
3. Fill in task details in dialog
4. Click **"Save"**

### Managing Dependencies
1. Click **"Deps"** button on any task
2. Select predecessor task
3. Choose dependency type (Finish-to-Start, Start-to-Start, etc.)
4. Set lag/lead days if needed
5. Click **"Add Dependency"**

### Viewing Critical Path
1. Click **"Critical Path"** tab
2. View all tasks on the critical path
3. Tasks highlighted in yellow on main view

### Viewing Statistics
1. Click **"Statistics"** tab
2. See summary metrics:
   - Total Tasks
   - Critical Path Tasks
   - Completed Tasks
   - In Progress Tasks

## 🔌 API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/projects/{id}/tasks` | Create task |
| GET | `/api/projects/{id}/tasks` | List all tasks |
| GET | `/api/projects/{id}/tasks/{taskId}` | Get single task |
| PUT | `/api/projects/{id}/tasks/{taskId}` | Update task |
| DELETE | `/api/projects/{id}/tasks/{taskId}` | Delete task |
| POST | `/api/projects/{id}/tasks/dependencies` | Add dependency |
| GET | `/api/projects/{id}/tasks/{taskId}/dependencies` | List dependencies |
| DELETE | `/api/projects/{id}/tasks/dependencies/{depId}` | Remove dependency |
| GET | `/api/projects/{id}/tasks/critical-path` | Get critical path |
| POST | `/api/projects/{id}/tasks/calculate-critical-path` | Recalculate |
| GET | `/api/projects/{id}/tasks/gantt` | Export Gantt data |

## 💻 Developer Guide

### Adding a New Task Operation

1. **Add method to `IProjectTaskService`**:
```csharp
Task<(bool Success, string Message, dynamic? Data)> MyOperationAsync(int projectId, int taskId);
```

2. **Implement in `ProjectTaskService`**:
```csharp
public async Task<(bool Success, string Message, dynamic? Data)> MyOperationAsync(int projectId, int taskId)
{
    // Implementation
}
```

3. **Add API endpoint in `ProjectTaskController`**:
```csharp
[HttpPost("projects/{projectId}/tasks/{taskId}/my-operation")]
[RequireFeature(FeatureKeys.TaskDependencies)]
public async Task<IActionResult> MyOperation(int projectId, int taskId)
{
    var result = await _taskService.MyOperationAsync(projectId, taskId);
    if (result.Success)
        return Ok(result.Data);
    return BadRequest(result.Message);
}
```

4. **Add method to `ProjectTaskHttpService`**:
```csharp
public async Task<(bool Success, string Message, dynamic? Data)> MyOperationAsync(
    int projectId, int taskId)
{
    var response = await _httpClient.PostAsJsonAsync(
        $"api/projects/{projectId}/tasks/{taskId}/my-operation", new { });
    // Handle response
}
```

### Understanding the Critical Path Algorithm

```csharp
// 1. Forward Pass - Calculate earliest times
foreach (var task in tasks.OrderBy(t => t.StartDate))
{
    task.EarliestStartDate = task.StartDate ?? DateTime.Now;
    task.EarliestFinishDate = task.EarliestStartDate.AddDays(task.DurationDays);
}

// 2. Backward Pass - Calculate latest times
foreach (var task in tasks.OrderByDescending(t => t.EndDate))
{
    task.LatestFinishDate = task.EndDate ?? DateTime.Now.AddDays(30);
    task.LatestStartDate = task.LatestFinishDate.AddDays(-task.DurationDays);
}

// 3. Calculate Slack Time
foreach (var task in tasks)
{
    task.SlackTimeDays = (task.LatestStartDate - task.EarliestStartDate).Days;
    task.IsOnCriticalPath = task.SlackTimeDays == 0;
}
```

## 📊 Data Models

### ProjectTask
```csharp
public int TaskId { get; set; }
public string TaskName { get; set; }
public string? Description { get; set; }
public int DurationDays { get; set; }
public DateTime? StartDate { get; set; }
public DateTime? EndDate { get; set; }
public string Status { get; set; } // Not Started, In Progress, Completed, On Hold
public int? SlackTimeDays { get; set; }
public bool IsOnCriticalPath { get; set; }
public int AssignedResourceCount { get; set; }
```

### TaskDependency
```csharp
public int TaskDependencyId { get; set; }
public int PredecessorTaskId { get; set; }
public int SuccessorTaskId { get; set; }
public string DependencyType { get; set; } // FinishToStart, StartToStart, etc.
public int LagDays { get; set; }
```

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run ProjectTask Tests Only
```bash
dotnet test --filter "ProjectTask"
```

### Add New Test
```csharp
[Fact]
public async Task CreateTask_WithValidData_ReturnsSuccessful()
{
    // Arrange
    var service = new ProjectTaskService(mockContext, mockLogger);
    
    // Act
    var result = await service.CreateTaskAsync(1, new ProjectTask { /* ... */ });
    
    // Assert
    Assert.True(result.Success);
}
```

## ⚠️ Common Pitfalls

1. **Circular Dependencies**: Don't create cycles in task dependencies
   - ✗ Task A → Task B → Task C → Task A
   - ✓ Use validation in service

2. **Missing Tenant ID**: All operations are tenant-scoped
   - Always pass `projectId` from authenticated user's tenant

3. **Cascade Deletes**: Deleting a task removes all dependencies
   - Document this behavior to users

4. **Recalculation Performance**: Large projects may take time
   - Consider async notification for long operations

## 📈 Performance Tips

1. **Index Critical Path Queries**:
```csharp
modelBuilder.Entity<ProjectTask>()
    .HasIndex(t => new { t.ProjectId, t.IsOnCriticalPath });
```

2. **Cache Critical Path Results**:
```csharp
var cacheKey = $"critical_path_{projectId}";
var cachedPath = await _cache.GetAsync(cacheKey);
```

3. **Batch Operations**:
```csharp
// Instead of calling one-by-one
await _context.ProjectTasks.AddRangeAsync(tasks);
await _context.SaveChangesAsync();
```

## 🐛 Debugging

### Enable Detailed Logging
```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Manimp.Services.Implementation.ProjectTaskService": "Debug"
    }
  }
}
```

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Tasks not appearing | Feature not enabled | Check tenant feature flags |
| CPM calculation slow | Large dataset | Add indexes, consider pagination |
| Dependency creation fails | Circular reference | Add validation to prevent cycles |
| UI doesn't update | Dialog not closed properly | Ensure `Dialog.Close()` is called |

## 📚 Related Documentation

- [Phase 2B Implementation Summary](./PHASE2B-IMPLEMENTATION-SUMMARY.md)
- [CPM Algorithm Details](./cpm-algorithm.md) *(future)*
- [API Documentation](../general/README.md)
- [Feature Gating Guide](../security/feature-gating.md)

---
**Last Updated**: October 20, 2025
**Status**: Ready for Production Testing
