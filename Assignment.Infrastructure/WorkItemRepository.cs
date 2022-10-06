using System.Collections;
using Assignment.Core;

namespace Assignment.Infrastructure;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context)
    {
        _context = context;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
        var entity = _context.Items.FirstOrDefault(i => i.Id == item.AssignedToId);
        Response response;

        if (item.AssignedToId == null) return (Response.BadRequest, entity.Id);

        if (entity == null)
        {
            entity = new WorkItem(item.Title);
            entity.Created = DateTime.UtcNow;
            entity.StateUpdated = DateTime.UtcNow;

            if (item.AssignedToId != null) entity.Id = (int)item.AssignedToId;
            foreach (var t in item.Tags)
            {
                var tag = _context.Tags.FirstOrDefault(c => c.Name == t);
                if (tag != null) entity.Tags.Add(tag); 
            }

            _context.Items.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }
        
        return (response, entity.Id);
    }

    public Response Delete(int itemId)
    {
        var entity = _context.Items.FirstOrDefault(i => i.Id == itemId);
        Response response;

        if (entity == null)
        {
            response = Response.NotFound;
        } 
        else if (entity.State == State.New)
        {
            _context.Items.Remove(entity);
            _context.SaveChanges();
            response = Response.Deleted;
        } 
        else if (entity.State == State.Active)
        {
            entity.State = State.Removed;
            response = Response.BadRequest; //isn't clear if this this is the right response according to business rules
        } 
        else 
        {
            response = Response.Conflict;
        }

        return response;
    }

    public WorkItemDetailsDTO Find(int itemId)
    {
        var item =  from i in _context.Items
                    let tags = i.Tags.Select(i => i.Name).ToHashSet()
                    where i.Id == itemId
                    select new WorkItemDetailsDTO   (i.Id, 
                                                    i.Title, 
                                                    i.Description, 
                                                    i.Created, 
                                                    i.AssignedTo.Name, 
                                                    i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                    i.State, 
                                                    i.StateUpdated);


        return item.FirstOrDefault();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        var workItems = from i in _context.Items
                        select new WorkItemDTO  (i.Id, 
                                                i.Title, 
                                                i.AssignedTo.Name, 
                                                i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                i.State);

        return workItems.ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var workItems = from i in _context.Items
                        where i.State == state
                        select new WorkItemDTO  (i.Id, 
                                                i.Title, 
                                                i.AssignedTo.Name, 
                                                i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                i.State);

        return workItems.ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var workItems = from i in _context.Items
                        where i.Tags.Select(t => t.Name).ToString() == tag
                        select new WorkItemDTO  (i.Id, 
                                                i.Title, i.AssignedTo.Name, 
                                                i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                i.State);

        return workItems.ToList();    
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        var workItems = from i in _context.Items
                        where i.AssignedTo.Id == userId
                        select new WorkItemDTO  (i.Id, 
                                                i.Title, 
                                                i.AssignedTo.Name, 
                                                i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                i.State);

        return workItems.ToList();    
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        var workItems = from i in _context.Items
                        where i.State == State.Removed
                        select new WorkItemDTO  (i.Id, 
                                                i.Title, 
                                                i.AssignedTo.Name, 
                                                i.Tags.Select(t => t.Name).ToList().AsReadOnly(), 
                                                i.State);

        return workItems.ToList();
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        var entity = _context.Items.FirstOrDefault(c => c.Id == item.Id);

        if (entity == null) return Response.NotFound;
        else
        {
        entity.Title = item.Title;
        entity.AssignedTo = _context.Users.FirstOrDefault(u => u.Id == item.AssignedToId);
        entity.Description = item.Description;
        entity.StateUpdated = DateTime.UtcNow;
        entity.Tags = new List<Tag>();
        foreach (var t in item.Tags) 
        {
            var tag = _context.Tags.FirstOrDefault(c => c.Name == t);
            if (tag != null) entity.Tags.Add(tag);
        }

            _context.SaveChanges();
            return Response.Updated;
        }
    }
}
