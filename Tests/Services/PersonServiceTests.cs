using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using GiaPha.Services.Logging;
using Moq;

namespace GiaPha.UnitTests.Services;

public sealed class PersonServiceTests
{
    private static readonly Guid ClanId = Guid.NewGuid();

    [Fact]
    public async Task AddChildrenAsync_WhenFatherNotFound_ThrowsNotFoundException()
    {
        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddChildrenAsync(ClanId, Guid.NewGuid(), new AddChildrenRequest
            {
                Children = { new AddChildItem { FullName = "Nguyễn Văn B" } }
            }));
    }

    [Fact]
    public async Task AddChildrenAsync_ComputesGenerationPathAndSiblingOrder()
    {
        var father = new Person
        {
            Id = Guid.NewGuid(),
            ClanId = ClanId,
            Generation = 3,
            Path = "/1/5/"
        };

        var createdPersons = new List<Person>();
        var createdLinks = new List<ParentChild>();

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(father.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(father);
        personRepository
            .Setup(x => x.Create(It.IsAny<Person>()))
            .Callback<Person>(p => createdPersons.Add(p));

        var parentChildRepository = new Mock<IParentChildRepository>();
        parentChildRepository
            .Setup(x => x.GetMaxSiblingOrderAsync(father.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        parentChildRepository
            .Setup(x => x.Create(It.IsAny<ParentChild>()))
            .Callback<ParentChild>(p => createdLinks.Add(p));

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.ParentChild).Returns(parentChildRepository.Object);
        repositoryManager
            .Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var result = await service.AddChildrenAsync(ClanId, father.Id, new AddChildrenRequest
        {
            Children =
            {
                new AddChildItem { FullName = "Con 1" },
                new AddChildItem { FullName = "Con 2" },
                new AddChildItem { FullName = "Con 3" }
            }
        });

        Assert.Equal(3, result.Count);
        Assert.All(result, c => Assert.Equal(4, c.Generation));
        Assert.All(result, c => Assert.StartsWith("/1/5/", c.Path));
        Assert.All(createdPersons, p => Assert.Equal(4, p.Generation));
        Assert.Equal(new[] { 1, 2, 3 }, createdLinks.Select(x => x.SiblingOrder).ToArray());
    }

    [Theory]
    [InlineData("/1/5/12/", "/1/5/12/9/")] // cha mới là con cháu của người đang xét
    [InlineData("/1/5/12/", "/1/5/12/")]   // cha mới là chính người đó
    public void EnsureNoCycle_WhenCreatesCycle_ThrowsConflictException(string personPath, string newFatherPath)
    {
        Assert.Throws<ConflictException>(() => PersonService.EnsureNoCycle(personPath, newFatherPath));
    }

    [Theory]
    [InlineData("/1/5/12/", "/1/5/")]
    [InlineData("/1/5/12/", "/1/")]
    public void EnsureNoCycle_WhenNewFatherIsAncestor_DoesNotThrow(string personPath, string newFatherPath)
    {
        PersonService.EnsureNoCycle(personPath, newFatherPath);
    }

    [Fact]
    public async Task AddUnionAsync_WhenRankDuplicate_ThrowsConflictException()
    {
        var husband = new Person { Id = Guid.NewGuid(), ClanId = ClanId };
        var wife = new Person { Id = Guid.NewGuid(), ClanId = ClanId };

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(husband.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(husband);
        personRepository
            .Setup(x => x.GetByIdAsync(wife.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wife);

        var unionRepository = new Mock<IUnionRepository>();
        unionRepository
            .Setup(x => x.ExistsByHusbandAndRankAsync(husband.Id, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.Union).Returns(unionRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.AddUnionAsync(ClanId, husband.Id, new AddUnionRequest { WifeId = wife.Id, Rank = 1 }));
    }

    [Fact]
    public async Task AddUnionAsync_WhenWifeNotFound_ThrowsNotFoundException()
    {
        var husband = new Person { Id = Guid.NewGuid(), ClanId = ClanId };
        var wifeId = Guid.NewGuid();

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByIdAsync(husband.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(husband);
        personRepository
            .Setup(x => x.GetByIdAsync(wifeId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddUnionAsync(ClanId, husband.Id, new AddUnionRequest { WifeId = wifeId, Rank = 1 }));
    }

    [Fact]
    public async Task MoveSubtreeAsync_UpdatesPathAndGeneration()
    {
        var person = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Generation = 4, Path = "/1/5/12/" };
        var newFather = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Generation = 2, Path = "/9/" };
        var descendant = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Generation = 5, Path = "/1/5/12/30/" };
        var link = new ParentChild { ParentId = Guid.NewGuid(), ChildId = person.Id, RelationType = RelationType.Biological };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(person.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(person);
        personRepository.Setup(x => x.GetByIdAsync(newFather.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(newFather);
        personRepository
            .Setup(x => x.GetByPathPrefixAsync("/1/5/12/", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { person, descendant });

        var parentChildRepository = new Mock<IParentChildRepository>();
        parentChildRepository
            .Setup(x => x.GetByChildIdAsync(person.Id, RelationType.Biological, It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);
        parentChildRepository.Setup(x => x.GetMaxSiblingOrderAsync(newFather.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.ParentChild).Returns(parentChildRepository.Object);
        repositoryManager.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await service.MoveSubtreeAsync(ClanId, person.Id, newFather.Id);

        Assert.Equal($"/9/{person.Id}/", person.Path);
        Assert.Equal(3, person.Generation);
        Assert.Equal($"/9/{person.Id}/30/", descendant.Path);
        Assert.Equal(4, descendant.Generation);
        Assert.Equal(newFather.Id, link.ParentId);
        repositoryManager.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePersonAsync_BlockWhenHasChildren_ThrowsConflict()
    {
        var person = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Path = "/1/" };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(person.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(person);

        var parentChildRepository = new Mock<IParentChildRepository>();
        parentChildRepository.Setup(x => x.ExistsByParentAsync(person.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.ParentChild).Returns(parentChildRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.DeletePersonAsync(ClanId, person.Id, "block"));
    }

    [Fact]
    public async Task DeletePersonAsync_Cascade_SoftDeletesSubtree()
    {
        var person = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Path = "/1/5/12/", Status = PersonStatus.Active };
        var descendant = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Path = "/1/5/12/30/", Status = PersonStatus.Active };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(person.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(person);
        personRepository
            .Setup(x => x.GetByPathPrefixAsync(person.Path, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { person, descendant });

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await service.DeletePersonAsync(ClanId, person.Id, "cascade");

        Assert.Equal(PersonStatus.Deleted, person.Status);
        Assert.Equal(PersonStatus.Deleted, descendant.Status);
    }

    [Fact]
    public async Task GetTreeAsync_BuildsNestedTree()
    {
        var root = new Person { Id = Guid.NewGuid(), ClanId = ClanId, FullName = "Gia Long", Generation = 1, Path = "/r/" };
        var child1 = new Person { Id = Guid.NewGuid(), ClanId = ClanId, FullName = "Minh Mạng", Generation = 2, Path = "/r/c1/" };
        var child2 = new Person { Id = Guid.NewGuid(), ClanId = ClanId, FullName = "Khác", Generation = 2, Path = "/r/c2/" };

        var link1 = new ParentChild { ParentId = root.Id, ChildId = child1.Id, RelationType = RelationType.Biological, SiblingOrder = 1 };
        var link2 = new ParentChild { ParentId = root.Id, ChildId = child2.Id, RelationType = RelationType.Biological, SiblingOrder = 2 };

        var personRepository = new Mock<IPersonRepository>();
        personRepository
            .Setup(x => x.GetByClanAsync(ClanId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { root, child1, child2 });

        var parentChildRepository = new Mock<IParentChildRepository>();
        parentChildRepository
            .Setup(x => x.GetByParentIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), RelationType.Biological, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { link1, link2 });

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.ParentChild).Returns(parentChildRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var tree = await service.GetTreeAsync(ClanId);

        Assert.NotNull(tree);
        Assert.Equal("Gia Long", tree!.Person.FullName);
        Assert.Equal(2, tree.Children.Count);
        Assert.Equal("Minh Mạng", tree.Children[0].Person.FullName);
        Assert.Equal("Khác", tree.Children[1].Person.FullName);
    }

    [Fact]
    public async Task GetAncestorsAsync_ReturnsAncestorsInOrder()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();

        var person = new Person { Id = c, ClanId = ClanId, FullName = "C", Path = $"/{a}/{b}/{c}/" };
        var ancestorA = new Person { Id = a, ClanId = ClanId, FullName = "A", Path = $"/{a}/" };
        var ancestorB = new Person { Id = b, ClanId = ClanId, FullName = "B", Path = $"/{a}/{b}/" };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(c, false, It.IsAny<CancellationToken>())).ReturnsAsync(person);
        personRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ancestorA, ancestorB });

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var result = await service.GetAncestorsAsync(ClanId, c);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].FullName);
        Assert.Equal("B", result[1].FullName);
    }

    [Fact]
    public async Task MergePersonsAsync_WhenSameLineage_ThrowsConflict()
    {
        var keep = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Path = "/1/" };
        var remove = new Person { Id = Guid.NewGuid(), ClanId = ClanId, Path = "/1/5/" };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(keep.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(keep);
        personRepository.Setup(x => x.GetByIdAsync(remove.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(remove);

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.MergePersonsAsync(ClanId, keep.Id, remove.Id));
    }

    [Fact]
    public async Task MergePersonsAsync_SoftDeletesRemoveAndFillsFields()
    {
        var keep = new Person { Id = Guid.NewGuid(), ClanId = ClanId, FullName = "Keep", Path = "/1/" };
        var remove = new Person { Id = Guid.NewGuid(), ClanId = ClanId, FullName = "Remove", Path = "/9/", HuyName = "Húy" };

        var personRepository = new Mock<IPersonRepository>();
        personRepository.Setup(x => x.GetByIdAsync(keep.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(keep);
        personRepository.Setup(x => x.GetByIdAsync(remove.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(remove);

        var parentChildRepository = new Mock<IParentChildRepository>();
        parentChildRepository
            .Setup(x => x.GetByParentIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), RelationType.Biological, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ParentChild>());
        parentChildRepository
            .Setup(x => x.GetAllByChildIdAsync(remove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ParentChild>());

        var unionRepository = new Mock<IUnionRepository>();
        unionRepository
            .Setup(x => x.GetByPersonIdAsync(remove.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Union>());

        var repositoryManager = new Mock<IRepositoryManager>();
        repositoryManager.SetupGet(x => x.Person).Returns(personRepository.Object);
        repositoryManager.SetupGet(x => x.ParentChild).Returns(parentChildRepository.Object);
        repositoryManager.SetupGet(x => x.Union).Returns(unionRepository.Object);
        repositoryManager.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new PersonService(repositoryManager.Object, Mock.Of<ILoggerManager>());

        var result = await service.MergePersonsAsync(ClanId, keep.Id, remove.Id);

        Assert.Equal(PersonStatus.Deleted, remove.Status);
        Assert.Equal(keep.Id, remove.MergedIntoId);
        Assert.Equal("Húy", keep.HuyName);
        Assert.Equal("Keep", result.FullName);
    }
}
