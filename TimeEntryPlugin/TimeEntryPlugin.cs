using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

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
      ///   Max count of plugin launches
      /// </summary>
      private const int MAX_DEPTH = 1;

      #endregion

      #region Fields

      /// <summary>
      ///   Конфигурация фильтра.
      /// </summary>
      private IOrganizationService organizationService;

      #endregion

      #region Public methods

      public void Execute(IServiceProvider serviceProvider)
      {
         IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

         if (context.Depth == MAX_DEPTH)
         {
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            organizationService = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains(TARGET) && context.InputParameters[TARGET] is Entity)
            {
               Entity timeEntryEntity = (Entity)context.InputParameters[TARGET];

               if (timeEntryEntity.LogicalName == MSDYN_TIMEENTRY
                  && timeEntryEntity.Attributes.Keys.Contains(MSDYN_START)
                  && timeEntryEntity.Attributes.Keys.Contains(MSDYN_END))
               {
                  var startTime = (DateTime)timeEntryEntity.Attributes[MSDYN_START];
                  var endTime = (DateTime)timeEntryEntity.Attributes[MSDYN_END];

                  if (startTime < endTime)
                  {
                     var exEntitiesList = getEхistingTimeEntryEntitites(startTime, endTime);

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
         newEntity["ownerid"] = timeEntryEntity["ownerid"];
         newEntity["msdyn_bookableresource"] = timeEntryEntity["msdyn_bookableresource"];
         newEntity["msdyn_timeentrysettingid"] = timeEntryEntity["msdyn_timeentrysettingid"];
         newEntity["msdyn_duration"] = timeEntryEntity["msdyn_duration"];
         newEntity[MSDYN_START] = start;
         newEntity[MSDYN_END] = end;

         organizationService.Create(newEntity);
      }

      /// <summary>
      /// Get existing entities in D365
      /// </summary>
      /// <param name="start">From time</param>
      /// <param name="end">To time</param>
      /// <returns>List of entity dates</returns>
      private IEnumerable<DateTime> getEхistingTimeEntryEntitites(DateTime start, DateTime end)
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
