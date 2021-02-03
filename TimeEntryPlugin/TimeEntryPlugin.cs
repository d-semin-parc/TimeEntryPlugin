using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace TimeEntry
{
   /// <summary>
   /// Create some Time Entry entities
   /// </summary>
   public class TimeEntryPlugin : IPlugin
   {
      #region Constants

      /// <summary>
      ///   Target input parameter key
      /// </summary>
      private const string TARGET = "Target";
      /// <summary>
      ///   Time Entry logicalName
      /// </summary>
      private const string MSDYN_TIMEENTRY = "msdyn_timeentry";
      /// <summary>
      ///   Start Time Entry atribute key
      /// </summary>
      private const string MSDYN_START = "msdyn_start";
      /// <summary>
      ///   End Time Entry atribute key
      /// </summary>
      private const string MSDYN_END = "msdyn_end";
      /// <summary>
      ///   Owner of Time Entry atribute key
      /// </summary>
      private const string OWNERID = "ownerid";
      /// <summary>
      ///  Bookable source Time Entry atribute key
      /// </summary>
      private const string MSDYN_BOOKABLERESOURCE = "msdyn_bookableresource";
      /// <summary>
      ///  Ettings ID for Time Entry atribute key
      /// </summary>
      private const string MSDYN_TIMEENTRYSETTINGSID = "msdyn_timeentrysettingid";
      /// <summary>
      ///  Duration of Time Entry atribute key
      /// </summary>
      private const string MSDYN_DURATION = "msdyn_duration";
      /// <summary>
      ///   Max count of plugin launches
      /// </summary>
      private const int MAX_DEPTH = 1;

      #endregion

      #region Fields

      /// <summary>
      /// FS organization service
      /// </summary>
      private IOrganizationService organizationService;

      #endregion

      #region Public methods

      public void Execute(IServiceProvider serviceProvider)
      {
         // Obtain the tracing service
         ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

         IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

         if (context.Depth == MAX_DEPTH)
         {
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            organizationService = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains(TARGET) && context.InputParameters[TARGET] is Entity)
            {
               Entity timeEntryEntity = (Entity)context.InputParameters[TARGET];

               /// The enterprise solutions make sense to make the business logic below 
               /// in a separate class for better reusability and test coverage, 
               /// but because the test plugin has a minimal logic, left this code here.
               if (timeEntryEntity.LogicalName == MSDYN_TIMEENTRY
                  && timeEntryEntity.Attributes.Keys.Contains(MSDYN_START)
                  && timeEntryEntity.Attributes.Keys.Contains(MSDYN_END))
               {
                  var startTime = (DateTime)timeEntryEntity[MSDYN_START];
                  var endTime = (DateTime)timeEntryEntity[MSDYN_END];

                  try
                  {
                     // Check for inequality of parameters and check that start date precedes end date,
                     // additional to the business logic of D365.
                     if (startTime.Date < endTime.Date)
                     {
                        var exEntitiesList = getEхistingTimeEntryEntitites(startTime, endTime, (Guid)timeEntryEntity[OWNERID]);

                        while (startTime.Date <= endTime.Date)
                        {
                           if (!exEntitiesList.Contains(startTime.Date))
                           {
                              copyTimeEntryEntity(timeEntryEntity, startTime, endTime);
                           }
                           startTime = startTime.AddDays(1);
                        }
                     }
                  }
                  catch (FaultException<OrganizationServiceFault> ex)
                  {
                     throw new InvalidPluginExecutionException($"An error occurred in {nameof(TimeEntryPlugin)}.", ex);
                  }
                  catch (Exception ex)
                  {
                     tracingService.Trace("TimeEntryPlugin: {0}", ex.ToString());
                     throw;
                  }
               }
            }
            else
            {
               throw new InvalidPluginExecutionException($"Invalid plugin {nameof(TimeEntryPlugin)} registration");
            }
         }
      }

      #endregion

      #region Private methods

      /// <summary>
      /// Copy time entry entity to new entity
      /// </summary>
      /// <param name="timeEntryEntity">Source entity</param>
      /// <param name="start">Start time</param>
      /// <param name="end">End time</param>
      private void copyTimeEntryEntity(Entity timeEntryEntity, DateTime start, DateTime end)
      {
         Entity newEntity = new Entity(MSDYN_TIMEENTRY);
         newEntity[OWNERID] = timeEntryEntity[OWNERID];
         newEntity[MSDYN_BOOKABLERESOURCE] = timeEntryEntity[MSDYN_BOOKABLERESOURCE];
         newEntity[MSDYN_TIMEENTRYSETTINGSID] = timeEntryEntity[MSDYN_TIMEENTRYSETTINGSID];
         newEntity[MSDYN_DURATION] = timeEntryEntity[MSDYN_DURATION];
         newEntity[MSDYN_START] = start;
         newEntity[MSDYN_END] = end;

         organizationService.Create(newEntity);
      }

      /// <summary>
      /// Get existing entities in D365
      /// </summary>
      /// <param name="start">From time</param>
      /// <param name="end">To time</param>
      /// <param name="ownerId">Owner of entry</param>
      /// <returns>List of entity dates</returns>
      private IEnumerable<DateTime> getEхistingTimeEntryEntitites(DateTime start, DateTime end, Guid ownerId)
      {
         var timeEntryEntityRequest = new QueryExpression
         {
            EntityName = MSDYN_TIMEENTRY,
            ColumnSet = new ColumnSet(MSDYN_START, MSDYN_END),
            Criteria = new FilterExpression
            {
               Conditions =
                           {
                              new ConditionExpression
                              {
                                 AttributeName = MSDYN_START,
                                 Operator = ConditionOperator.Between,
                                 Values = { start, end }
                              },
                              new ConditionExpression
                              {
                              AttributeName = OWNERID,
                              Operator = ConditionOperator.Equal,
                              Values = { ownerId }
            }
                           }
            }
         };

         var collection = organizationService.RetrieveMultiple(timeEntryEntityRequest);
         return collection.Entities.Select(e => ((DateTime)e.Attributes[MSDYN_START]).Date);
      }
      #endregion
   }
}

