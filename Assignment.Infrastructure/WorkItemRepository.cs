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

        if (entity == null)
        {
            entity = new WorkItem(item.Title);
            //we want to set the Created and StateUpdated to now

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
        //var item =  from i in _context.Items
        //            let tags = i.Tags.Select(i => i.Name).ToHashSet()
        //            where i.Id == itemId
        //            select new WorkItemDetailsDTO(i.Id, i.Title, i.AssignedTo.De, i.)
        //same problem more or less, how do I get description (amongst others) here?


        //var entity = _context.Items.FirstOrDefault(i => i.Id == itemId);

        //return new WorkItemDetailsDTO(itemId, entity.Title);

        throw new NotImplementedException();

    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        //var workItems = from i in _context.Items
        //                select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name, i.Tags == null ? null : i.AssignedTo.Items, i.State);
        throw new NotImplementedException();

    }

    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        throw new NotImplementedException();
    }

    public Response Update(WorkItemUpdateDTO item)
    {
        throw new NotImplementedException();
    }
}
