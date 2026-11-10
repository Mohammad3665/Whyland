using Whyland.Domain.Model.Blog.BlogPost.Repositories;
using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Infrastructure.Persistence.Repositories.Blog.BlogPost;

/// <summary>
/// The EF Core command repository for the <c>BlogPost</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
public class BlogPostCommandRepository(WhylandDbContext context) : IBlogPostCommandRepository
{
    /// <summary>The rows the listing and the ordering walk still show.</summary>
    private IQueryable<BlogPostEntity> Live => context.BlogPosts.Where(x => !x.IsDeleted);

    public Task<BlogPostEntity?> GetByIdAsync(BlogPostId id, CancellationToken ct = default)
        => Live.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> IsSlugTakenAsync(
        string slug,
        BlogPostId? excludeId = null,
        CancellationToken ct = default)
    {
        var excluded = excludeId is { } skip ? skip : (BlogPostId?)null;

        return Live.AsNoTracking()
            .AnyAsync(
                x => x.Slug == slug && (excluded == null || x.Id != excluded),
                ct);
    }

    public async Task AddAsync(BlogPostEntity post, CancellationToken ct = default)
        => await context.BlogPosts.AddAsync(post, ct);

    public void Update(BlogPostEntity post)
        => context.Entry(post).State = EntityState.Modified;

    public void SoftDelete(BlogPostEntity post) => post.MarkDeleted();

    public async Task ReorderAsync(
        IReadOnlyList<BlogPostOrdering> ordering, CancellationToken ct = default)
    {
        if (ordering.Count == 0)
            return;

        var ids = ordering.Select(x => x.Id).ToList();
        var rows = await Live
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var item in ordering)
            if (rows.TryGetValue(item.Id, out var post))
                post.SetDisplayOrder(item.DisplayOrder);
    }

    public async Task RenumberAsync(CancellationToken ct = default)
    {
        var posts = await Live
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < posts.Count; i++)
            posts[i].SetDisplayOrder(i + 1);
    }
}
