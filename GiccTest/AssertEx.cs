using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Reflection;

using NUnit.Framework;

namespace Gicc.Test
{
	// Use reflection to Compare two objects.
	// At first I tried to use JSON serialization to compare objects,
	// but it failed cuz it didn't serialize properties in alphabetical order.
	public class AssertEx
	{
		public static void PropertyValuesAreEquals(object expected, object actual)
		{
			PropertyInfo[] properties = expected.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				object expectedValue = property.GetValue(expected, null);
				object actualValue = property.GetValue(actual, null);

				if (actualValue is IList)
					AssertListsAreEquals(property, (IList)actualValue, (IList)expectedValue);
				else if (!Equals(expectedValue, actualValue))
					Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue, actualValue);
			}
		}

		public static void PropertyValuesAreNotEquals(object expected, object actual)
		{
			try
			{
				PropertyValuesAreEquals(expected, actual);
			}
			catch (AssertionException)
			{
				return;
			}

			Assert.Fail("The objects are equal.");
		}

		private static void AssertListsAreEquals(PropertyInfo property, IList actualList, IList expectedList)
		{
			if (actualList.Count != expectedList.Count)
				Assert.Fail("Property {0}.{1} does not match. Expected IList containing {2} elements but was IList containing {3} elements", property.PropertyType.Name, property.Name, expectedList.Count, actualList.Count);

			for (int i = 0; i < actualList.Count; i++)
				if (!Equals(actualList[i], expectedList[i]))
					Assert.Fail("Property {0}.{1} does not match. Expected IList with element {1} equals to {2} but was IList with element {1} equals to {3}", property.PropertyType.Name, property.Name, expectedList[i], actualList[i]);
		}
	}
}
