using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Framework.Data.ResultSetMapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Framework.Data.Tests
{
    public class Author
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }

    public class Book
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public Author Author { get; set; }
    }

    /// <summary>
    /// This is a test class for EntityHydratorTest and is intended
    /// to contain all EntityHydratorTest Unit Tests
    /// </summary>
    [TestClass]
    public class EntityHydratorTest
    {
        private Dictionary<string, EntityMapping> mappings;

        private IDbConnection connection;
        private IDbTransaction transaction;

        private EntityHydrator hydrator;

        [TestInitialize]
        public void Setup()
        {
            var authorMapping = new EntityMapping
            {
                EntityType = typeof(Author),
                PrimaryKey = new PrimaryKeyMapping { ColumnName = "Id", PropertyInfo = typeof(Author).GetProperty("Id") }
            };

            authorMapping.AddColumn(new ColumnMapping { ColumnName = "FirstName", PropertyInfo = typeof(Author).GetProperty("FirstName") });
            authorMapping.AddColumn(new ColumnMapping { ColumnName = "MiddleName", PropertyInfo = typeof(Author).GetProperty("MiddleName") });
            authorMapping.AddColumn(new ColumnMapping { ColumnName = "LastName", PropertyInfo = typeof(Author).GetProperty("LastName") });

            var bookMapping = new EntityMapping
            {
                EntityType = typeof(Book),
                PrimaryKey = new PrimaryKeyMapping { ColumnName = "Id", PropertyInfo = typeof(Book).GetProperty("Id") }
            };

            bookMapping.AddColumn(new ColumnMapping { ColumnName = "Title", PropertyInfo = typeof(Book).GetProperty("Title") });
            bookMapping.AddColumn(new ColumnMapping { ColumnName = "Description", PropertyInfo = typeof(Book).GetProperty("Description") });

            bookMapping.AddRelation(new OneToOneRelationMapping { ColumnName = "AuthorId", PropertyInfo = typeof(Book).GetProperty("Author"), ReferenceType = typeof(Author) });

            mappings = new Dictionary<string, EntityMapping>
            {
                {"Author", authorMapping},
                {"Book", bookMapping}
            };

            var metadataStoreMock = new Mock<IMetadataStore>();
            var sessionMock = new Mock<ISession>();
            var sessionLevelCacheMock = new Mock<ISessionLevelCache>();

            metadataStoreMock.Setup(_ => _.GetMapping(It.IsAny<Type>())).Returns<Type>(_ => mappings[_.Name]);
            metadataStoreMock.Setup(_ => _.GetMapping(It.IsAny<string>())).Returns<string>(_ => mappings[_]);

            var cache = new Dictionary<Type, Dictionary<string, object>>();
            sessionLevelCacheMock.Setup(_ => _.Store(It.IsAny<Type>(), It.IsAny<object>(), It.IsAny<object>()))
                .Callback<Type, object, object>((entityType, entityKey, entity) =>
                {
                    Dictionary<string, object> entityCache;
                    if (!cache.TryGetValue(entityType, out entityCache))
                    {
                        cache.Add(entityType, entityCache = new Dictionary<string, object>());
                    }
                    entityCache.Add(entityKey.ToString(), entity);
                });
            sessionLevelCacheMock.Setup(_ => _.TryToFind(It.IsAny<Type>(), It.IsAny<object>()))
                .Returns<Type, object>((entityType, entityKey) =>
                {
                    Dictionary<string, object> entityCache;
                    if (!cache.TryGetValue(entityType, out entityCache))
                    {
                        return null;
                    }
                    object entity;
                    if (!entityCache.TryGetValue(entityKey.ToString(), out entity))
                    {
                        return null;
                    }
                    return entity;
                });

            hydrator = new EntityHydrator(metadataStoreMock.Object, sessionMock.Object, sessionLevelCacheMock.Object);

            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString);
            connection.Open();
            transaction = connection.BeginTransaction();
        }

        [TestCleanup]
        public void TearDown()
        {
            transaction.Rollback();
            connection.Close();
        }

        [TestMethod]
        public void It_should_hydrate_a_single_Author()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT * FROM Authors WHERE Id = 1";

                var entity = hydrator.HydrateEntity<Author>(command);
                Assert.IsNotNull(entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual("John", entity.FirstName);
                Assert.IsNull(entity.MiddleName);
                Assert.AreEqual("Doe", entity.LastName);
            }
        }

        [TestMethod]
        public void It_should_create_a_lazy_loagind_proxy_for_the_author()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT * FROM Books WHERE Id = 1";

                var book = hydrator.HydrateEntity<Book>(command);
                Assert.AreEqual(1, book.Id);
                Assert.IsNotNull(book.Author);
                Assert.IsNotNull(book.Author.Id);
            }
        }

        [TestMethod]
        public void It_should_uses_the_alias_when_provided()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText =
                    "SELECT Id as B_Id, Title as B_Title, Description as B_Description, AuthorId as B_AuthorId " +
                    "FROM Books " +
                    "WHERE Id = 1";

                var book = (Book)hydrator.HydrateEntity(
                    command,
                    new ResultSetMappingDefinition().AddQueryReturn(new QueryRootResult("B", "Book"))
                );

                Assert.AreEqual(1, book.Id);
                Assert.IsNotNull(book.Author);
                Assert.IsNotNull(book.Title);
            }
        }

        [TestMethod]
        public void It_should_eagerly_load_entity_from_a_join()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText =
                    "Select B.*, A.Id as A_Id, A.FirstName as A_FirstName, A.MiddleName as A_MiddleName, A.LastName as A_LastName " +
                    "FROM Books as B " +
                    "LEFT JOIN Authors as A ON B.AuthorId = A.Id " +
                    "WHERE B.Id = 1";

                var book = (Book)hydrator.HydrateEntity(command, new ResultSetMappingDefinition()
                    .AddQueryReturn(new QueryRootResult("Book"))
                    .AddQueryReturn(new QueryJoinResult("A", string.Empty, "AuthorId")));

                Assert.AreEqual(1, book.Id);
                Assert.IsNotNull(book.Author);
                Assert.IsNotNull(book.Title);
                Assert.IsNotNull(book.Author.Id);
                Assert.IsNotNull(book.Author.FirstName);
                Assert.IsNotNull(book.Author.LastName);
            }
        }

        [TestMethod]
        public void It_should_load_a_collection()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT * FROM Books";

                var books = hydrator.HydrateEntities<Book>(command);

                Assert.IsTrue(books.Count() > 1);
            }
        }
    }
}
