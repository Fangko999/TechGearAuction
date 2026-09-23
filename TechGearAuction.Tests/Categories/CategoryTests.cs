using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Categories.Commands;
using TechGearAuction.Application.Features.Categories.Queries;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Categories;

public class CategoryTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public CategoryTests() => _factory = new TestDbFactory();

    [Fact]
    public async Task CreateCategory_ShouldPersist()
    {
        var ctx = _factory.CreateContext();
        var handler = new CreateCategoryCommandHandler(ctx);

        var id = await handler.Handle(new CreateCategoryCommand
        {
            Name = "Smartphones",
            ParentId = TestDbFactory.ParentCategoryId
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var category = await verifyCtx.Categories.FindAsync(id);
        category.Should().NotBeNull();
        category!.Name.Should().Be("Smartphones");
        category.ParentId.Should().Be(TestDbFactory.ParentCategoryId);
    }

    [Fact]
    public async Task UpdateCategory_ShouldChangeName()
    {
        var ctx = _factory.CreateContext();
        var handler = new UpdateCategoryCommandHandler(ctx);

        await handler.Handle(new UpdateCategoryCommand
        {
            Id = TestDbFactory.ChildCategoryId,
            Name = "Gaming Laptops",
            ParentId = TestDbFactory.ParentCategoryId
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var category = await verifyCtx.Categories.FindAsync(TestDbFactory.ChildCategoryId);
        category.Should().NotBeNull();
        category!.Name.Should().Be("Gaming Laptops");
    }

    [Fact]
    public async Task DeleteCategory_WithoutRelations_ShouldSucceed()
    {
        // First, create a new category without relations
        Guid newId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Categories.Add(new TechGearAuction.Domain.Entities.Category { Id = newId, Name = "Accessories" });
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new DeleteCategoryCommandHandler(ctx);

        await handler.Handle(new DeleteCategoryCommand { Id = newId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var category = await verifyCtx.Categories.FindAsync(newId);
        category.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategory_WithAuctions_ShouldThrow()
    {
        var ctx = _factory.CreateContext();
        var handler = new DeleteCategoryCommandHandler(ctx);

        // ChildCategory has ActiveAuctionId associated with it
        var act = () => handler.Handle(new DeleteCategoryCommand { Id = TestDbFactory.ChildCategoryId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*associated auctions*");
    }

    [Fact]
    public async Task DeleteCategory_WithSubcategories_ShouldThrow()
    {
        var ctx = _factory.CreateContext();
        var handler = new DeleteCategoryCommandHandler(ctx);

        // ParentCategory has ChildCategory
        var act = () => handler.Handle(new DeleteCategoryCommand { Id = TestDbFactory.ParentCategoryId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*sub-categories*");
    }

    [Fact]
    public async Task GetCategories_ShouldReturnHierarchy()
    {
        var ctx = _factory.CreateContext();
        var handler = new GetCategoriesQueryHandler(ctx);

        var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().NotBeEmpty();
        var parent = result.FirstOrDefault(c => c.Id == TestDbFactory.ParentCategoryId);
        parent.Should().NotBeNull();
        parent!.SubCategories.Should().NotBeEmpty();
        parent.SubCategories.Should().Contain(c => c.Id == TestDbFactory.ChildCategoryId);
    }

    public void Dispose() => _factory.Dispose();
}
