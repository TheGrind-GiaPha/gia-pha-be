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
}
