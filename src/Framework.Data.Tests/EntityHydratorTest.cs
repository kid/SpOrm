using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Framework.Data.Tests
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }

    /// <summary>
    /// This is a test class for EntityHydratorTest and is intended
    /// to contain all EntityHydratorTest Unit Tests
    /// </summary>
    [TestClass]
    public class EntityHydratorTest
    {
        private IDbConnection connection;
        private IDbTransaction transaction;

        private EntityHydrator hydrator;

        [TestInitialize]
        public void Setup()
        {
            var metadataStoreMock = new Mock<IMetadataStore>();
            var sessionMock = new Mock<ISession>();
            var sessionLevelCacheMock = new Mock<ISessionLevelCache>();

            metadataStoreMock
                .Setup(_ => _.GetMapping(It.Is<Type>(entityType => entityType == typeof(Customer))))
                .Returns(
                () =>
                {
                    var entityMapping = new EntityMapping
                    {
                        EntityType = typeof(Customer),
                        PrimaryKey = new PrimaryKeyMapping { ColumnName = "Id", PropertyInfo = typeof(Customer).GetProperty("Id") }
                    };
                    entityMapping.AddColumn(new ColumnMapping { ColumnName = "FirstName", PropertyInfo = typeof(Customer).GetProperty("FirstName") });
                    entityMapping.AddColumn(new ColumnMapping { ColumnName = "MiddleName", PropertyInfo = typeof(Customer).GetProperty("MiddleName") });
                    entityMapping.AddColumn(new ColumnMapping { ColumnName = "LastName", PropertyInfo = typeof(Customer).GetProperty("LastName") });
                    return entityMapping;
                });

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
        public void HydrateEntityTest()
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "SELECT * FROM Customers WHERE Id = 1";

                var entity = hydrator.HydrateEntity<Customer>(command);
                Assert.IsNotNull(entity);
                Assert.AreEqual(1, entity.Id);
                Assert.AreEqual("John", entity.FirstName);
                Assert.IsNull(entity.MiddleName);
                Assert.AreEqual("Doe", entity.LastName);
            }
        }
    }
}
