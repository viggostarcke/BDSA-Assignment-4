using Assignment.Core;

namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }

    (Response Response, int TagId) ITagRepository.Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(c => c.Name == tag.Name);
        Response response;

        if (entity is null)
        {
            entity = new Tag(tag.Name);

            _context.Tags.Add(entity);
            _context.SaveChanges();

            response = Response.Created;
        }
        else
        {
            response = Response.Conflict;
        }

        return (response, entity.Id);
    }

    IReadOnlyCollection<TagDTO> ITagRepository.Read()
    {
        List<TagDTO> list = new();
        var entity = _context.Tags;
        foreach (var e in entity)
        {
            list.Add(new TagDTO(e.Id, e.Name));
        }
        return list;    
    }

    TagDTO ITagRepository.Find(int tagId)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Id == tagId);

        return new TagDTO(entity.Id, entity.Name);
    }

    Response ITagRepository.Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(c => c.Id == tag.Id);

        if (entity is null) return Response.NotFound;
        else {
            entity.Name = tag.Name;
            _context.SaveChanges();
            return Response.Updated;
        }
    }

    Response ITagRepository.Delete(int tagId, bool force)
    {
        var tagExisting = _context.Tags.FirstOrDefault(c => c.Id == tagId);
        Response response;

        var tags = _context.Tags;
        tags.Find(tagId);
        WorkItem workItemInUse = null;
        foreach (var t in tags) foreach (var workItem in t.WorkItems) if (workItem.Id == tagId) workItemInUse = workItem;

        if (tagExisting is null)
            response = Response.NotFound;
        else if (workItemInUse is null || force)
        {
            _context.Tags.Remove(tagExisting);
            _context.SaveChanges();

            if (workItemInUse is not null) foreach (var task in _context.Items) task.Tags.Remove(tagExisting);

            response = Response.Deleted;
        }
        else response = Response.Conflict;

        return response;
    }
    
}
