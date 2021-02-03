# TimeEntryPlugin
Plugin interact with the Time Entry Entity in D365

## Technical task
On creation of a Time Entry record the plugin should evaluate if the start and end date contain different values from each other. In the event that the start and end date are different then a time entry record should be created for every date in the date range from start to end date. The plugin should also ensure that there are no duplicate time entry records created per date. 

## Installation
1. Build project TimeEntryPlugin
1. Register the TimeEntryPlugin
1. Add a new step as shown in the image below:
![](/images/registernewstepdialog.png) 

 * "Message" = "Create"
 * "Primary Entity" = "msdyn_timeentry"
 * "Event Pipeline Stage of Execution" = "PostOperation"
 * "Execution Mode" = "Asynchronous"
 
## Reproduction steps
1. Go to https://sfgfsdgdfsgdfssdfhdfs.crm4.dynamics.com/ as dmtr@testapp12345.onmicrosoft.com/4062178#Q
1. Create new time entry with any shared resources.
1. Refresh any time entry view after a few seconds.

## Follow up items
### Time Entry Plugin
1. Provide for multi-user work with the entity plugin and entity db
1. Add business logic to validate filled time entry attributes, by implementing the "instead of trigger" pattern.
1. Add time zones handling.
1. Move business logic out of the plugin code into a separate class.
1. Work out security concepts: allocate a separate account for automation, restrict the set of rights, add tracing of user actions, etc.
### Testing / CI
1. Add load testing and complicate unit testing scripts (complicate the initial scene, add more attributes to the created entities).
1. Implement an CI/CD practices based on Azure DevOps
