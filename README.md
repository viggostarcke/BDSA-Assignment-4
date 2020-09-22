# Assignment #4

## C&#35;

Fork this repository and implement the code required for the assignments below.

### Kanban Board part deux

[![Simple-kanban-board-](https://upload.wikimedia.org/wikipedia/commons/thumb/d/d3/Simple-kanban-board-.jpg/512px-Simple-kanban-board-.jpg)](https://commons.wikimedia.org/wiki/File:Simple-kanban-board-.jpg "Jeff.lasovski [CC BY-SA 3.0 (https://creativecommons.org/licenses/by-sa/3.0)], via Wikimedia Commons")

Implement and test the `ITaskRepository` and `ITagRepository` interfaces.

```csharp
public enum Response
{
    Created,
    Updated,
    Deleted,
    NotFound,
    BadRequest,
    Conflict
}

public interface ITaskRepository
{
    (Response response, int taskId) Create(TaskCreateDTO task);
    IQueryable<TaskListDTO> Read(bool includeRemoved = false);
    TaskDetailsDTO Read(int taskId);
    Response Update(TaskUpdateDTO task);
    Response Delete(int taskId);
}

public interface ITagRepository
{
    (Response response, int taskId) Create(TagCreateDTO tag);
    IQueryable<TagDTO> Read();
    TagDTO Read(int tagId);
    Response Update(TagUpdateDTO tag);
    Response Delete(int tagId, bool force = false);
}
```

with the following rules:

#### General

- Trying to updated or delete a non-existing entity should return `NotFound`.
- `CUD` should return a proper `Response`.
- Your are not allowed to write `throw new ...` - use the `Response` instead.
- Your code must use an in-memory database and/or mocks for testing.
- If a task or tag is not found, return `null`.

#### Task Repository

- Only tasks which have the state `New` can be deleted from the database.
- Deleting a task which is `Active` should set its state to `Removed`.
- Deleting a task which is `Resolved`, `Closed`, or `Removed` should return `Conflict`.
- Creating a task will set its state to `New`.
- Create/update task must allow for editing tags.
- Assigning a user which does not exist should return `Conflict`.
- TaskRepository may *not* reference *TagRepository*.
- Create/update should allow adding/removing a user - and return `BadRequest` if the user does not exist.

#### Tag Repository

- Tags which are in use may only be deleted using the `force`.
- Trying to delete a tag in use without the `force` should return `Conflict`.
- Trying to create a tag which exists already should return `Conflict`.

## Submitting the assignment

To submit the assignment you need to create a .pdf document using LaTeX containing the answers to the questions and a link to a public repository containing your fork of the completed code.
