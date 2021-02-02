using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TimeEntry;
using TimeEntryPluginUnitTest;

namespace TimeEntryUnitTest
{
   [TestClass]
   public class TimeEntryPluginUnitTest : BaseUnitTest
   { 
      /// <summary>
      /// Checks 
      /// </summary>
      [TestMethod ("Check time entry creation with start and end on the same day")]
      public void Add_SameDayTimeEntry()
      {
         Entity target = createFakeTimeEntryEntity(DateTime.Now, DateTime.Now);
         executePluginWithTarget(target);
         var entries = getCountTimeEntries();

         Assert.AreEqual(1, entries);
      }

      [TestMethod("Check time entry creation with a duration of 1 day")]
      public void Add_OneDayDurationDayTimeEntry()
      {
         Entity target = createFakeTimeEntryEntity(DateTime.Now, DateTime.Now.AddDays(1));
         executePluginWithTarget(target);
         var entries = getCountTimeEntries();

         Assert.AreEqual(2, entries);
      }

      [TestMethod("Check time entry creation with a duration of 5 days")]
      public void Add_ManyDayTimeEntry()
      {
         Entity target = createFakeTimeEntryEntity(DateTime.Now, DateTime.Now.AddDays(5));
         executePluginWithTarget(target);
         var entries = getCountTimeEntries();

         Assert.AreEqual(6, entries);
      }

      [TestMethod("Check time entry creation with invalid start and end dates")]
      public void Add_NotValidDates()
      {
         Entity target = createFakeTimeEntryEntity(DateTime.Now, DateTime.Now.AddDays(-2));
         executePluginWithTarget(target);
         var entries = getCountTimeEntries();

         Assert.AreEqual(1, entries);
      }

      [TestMethod("Check time entry creation if there are records in the database")]
      public void Check_ExistsEntryOnThisDay()
      {
         var service = FakeСontext.GetOrganizationService();
         var exEntity = createFakeTimeEntryEntity(DateTime.Now, DateTime.Now);

         service.Create(exEntity);

         var entries = getCountTimeEntries();
         Assert.AreEqual(1, entries);

         var target = createFakeTimeEntryEntity(DateTime.Now.AddDays(-2), DateTime.Now.AddDays(2));
         executePluginWithTarget(target);

         entries = getCountTimeEntries();
         Assert.AreEqual(5, entries);
      }

      /// <summary>
      /// Initialize some test custom settings
      /// </summary>
      protected override void TestInitializeCustomSettings()
      {
         PluginContext = FakeСontext.GetDefaultPluginContext();
         PluginContext.Stage = 20;
         PluginContext.MessageName = "Create";
         PluginContext.PrimaryEntityName = "msdyn_timeentry";
      }

      /// <summary>
      /// Create fake time entry entity
      /// </summary>
      /// <param name="start">Start date of time entity</param>
      /// <param name="end">End date of time entity</param>
      /// <returns>Time entity</returns>
      private Entity createFakeTimeEntryEntity(DateTime start, DateTime end)
      {
         Entity entity = new Entity()
         {
            Id = Guid.NewGuid(),
            Attributes = new AttributeCollection()
         };
         entity.LogicalName = "msdyn_timeentry";
         entity.Attributes.Add("msdyn_start", start);
         entity.Attributes.Add("msdyn_end", end);
         entity.Attributes.Add("msdyn_bookableresource", Guid.NewGuid());
         entity.Attributes.Add("msdyn_duration", (end - start).TotalDays);
         entity.Attributes.Add("ownerid", Guid.NewGuid());
         entity.Attributes.Add("msdyn_timeentrysettingid", Guid.NewGuid());
         return entity;
      }

      /// <summary>
      /// Get total count of time entry entities in organization DB
      /// </summary>
      /// <returns>Count of entities</returns>
      private int getCountTimeEntries()
      {
         var timeEntryEntityRequest = new QueryExpression
         {
            EntityName = "msdyn_timeentry",
            ColumnSet = new ColumnSet("msdyn_start"),
         };        

         var service = FakeСontext.GetOrganizationService();
         var collection = service.RetrieveMultiple(timeEntryEntityRequest);
         return collection == null ? 0 : collection.Entities.Count;
      }


      /// <summary>
      /// Triggered pagin execution
      /// </summary>
      /// <param name="target">Primary entity</param>
      private void executePluginWithTarget(Entity target)
      {
         var inputParameter = new ParameterCollection
         {
            { "Target", target }
         };
         PluginContext.PrimaryEntityId = Guid.NewGuid();
         PluginContext.InputParameters = inputParameter;

         FakeСontext.Initialize(new System.Collections.Generic.List<Entity>() { target });

         FakeСontext.ExecutePluginWith<TimeEntryPlugin>(PluginContext);
      }
   }
}
