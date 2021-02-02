using FakeXrmEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TimeEntryPluginUnitTest
{
   [TestClass]
   public abstract class BaseUnitTest
   {
 
      public XrmFakedContext FakeСontext = new XrmFakedContext();
      public XrmFakedPluginExecutionContext PluginContext;

      /// <summary>
      /// Sets up.
      /// </summary>
      [TestInitialize]
      public void TestInitialize()
      {
         TestInitializeCustomSettings();
      }

      /// <summary>
      /// Sets down.
      /// </summary>
      [TestCleanup]
      public void TestClear()
      {
         TestClearCustomSettings();
      }

      /// <summary>
      /// Initialize custom setting, before every unit test
      /// </summary>
      protected virtual void TestInitializeCustomSettings()
      {
      }

      /// <summary>
      /// Clear custom settings, after every unit test
      /// </summary>
      protected virtual void TestClearCustomSettings()
      {
      }
   }
}
