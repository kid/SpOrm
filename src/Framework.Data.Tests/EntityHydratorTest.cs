using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

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
        private Dictionary<Type, EntityMapping> mappings;

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

            mappings = new Dictionary<Type, EntityMapping>
            {
                {typeof(Author), authorMapping},
                {typeof(Book), bookMapping}
            };

            var metadataStoreMock = new Mock<IMetadataStore>();
            var sessionMock = new Mock<ISession>();
            var sessionLevelCacheMock = new Mock<ISessionLevelCache>();

            metadataStoreMock.Setup(_ => _.GetMapping(It.IsAny<Type>())).Returns<Type>(_ => mappings[_]);

            hydrator = new EntityHydrator(metadataStoreMock.Object, sessionMock.Object, sessionLevelCacheMock.Object);

            connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=TestDb;Integrated Security=True;Pooling=False");
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
    }
}
