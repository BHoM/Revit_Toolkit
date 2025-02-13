/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        public class RevitParameterFilterService : IRevitParameterFilterService
        {
            public List<IBHoMObject> Filter(IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<RevitFilterCriteria> criteria)
            {
                foreach (var crit in criteria)
                {
                    IRevitParameterFilter strategy = Create(crit.FilterType);
                    bHoMObjects = bHoMObjects.Where(x => strategy.IsMatch(x, crit.ParameterName, crit.ParameterValue));
                }
                return bHoMObjects.ToList();
            }
        }

        public interface IRevitParameterFilterService
        {
            List<IBHoMObject> Filter(IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<RevitFilterCriteria> criteria);
        }

        public class RevitFilterCriteria
        {
            public string ParameterName { get; set; }
            public object ParameterValue { get; set; }
            public FilterType FilterType { get; set; }
        }

        public enum FilterType
        {
            Equal,
            NotEqual,
            Existance,
            GreaterThan,
            LessThan,
            GreaterThanOrEqual,
            LessThanOrEqual,
            Contains,
            DoesNotContain,
            StartsWith,
            EndsWith,
            NoFilter,
        }

        public static IRevitParameterFilter Create(FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Equal:
                    return new EqualFilter();
                case FilterType.NotEqual:
                    return new NotEqualFilter();
                case FilterType.Existance:
                    return new ExistanceFilter();
                case FilterType.GreaterThan:
                    return new GreaterThanFilter();
                case FilterType.LessThan:
                    return new LessThanFilter();
                case FilterType.GreaterThanOrEqual:
                    return new GreaterThanOrEqualFilter();
                case FilterType.LessThanOrEqual:
                    return new LessThanOrEqualFilter();
                case FilterType.Contains:
                    return new ContainsFilter();
                case FilterType.StartsWith:
                    return new StartsWithFilter();
                case FilterType.EndsWith:
                    return new EndsWithFilter();
                case FilterType.NoFilter:
                    return new NoFilter();
                default:
                    throw new ArgumentException($"Invalid filter type: {filterType}");
            }
        }


        public interface IRevitParameterFilter
        {
            bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue);
        }

        // NO FILTER
        public class NoFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                return true;
            }
        }

        // EQUAL
        public class EqualFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);
                return Equals(objValue, parameterValue);
            }
        }

        // NOT EQUAL
        public class NotEqualFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);
                return !Equals(objValue, parameterValue);
            }
        }

        // EXISTANCE
        public class ExistanceFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                // If the parameter is simply present (non-null), it "exists"
                // You could refine this logic if "Existance" means something else in your context.
                object objValue = obj.GetRevitParameterValue(parameterName);
                return objValue != null;
            }
        }

        // GREATER THAN
        public class GreaterThanFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                int? comparison = SafeCompare(objValue, parameterValue);
                return comparison.HasValue && comparison.Value > 0;
            }
        }

        // LESS THAN
        public class LessThanFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                int? comparison = SafeCompare(objValue, parameterValue);
                return comparison.HasValue && comparison.Value < 0;
            }
        }

        // GREATER THAN OR EQUAL
        public class GreaterThanOrEqualFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                int? comparison = SafeCompare(objValue, parameterValue);
                return comparison.HasValue && comparison.Value >= 0;
            }
        }

        // LESS THAN OR EQUAL
        public class LessThanOrEqualFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                int? comparison = SafeCompare(objValue, parameterValue);
                return comparison.HasValue && comparison.Value <= 0;
            }
        }

        // CONTAINS
        public class ContainsFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                // Basic string "Contains" logic
                if (objValue is string strVal && parameterValue is string strParam)
                    return strVal.Contains(strParam);

                return false;
            }
        }

        // STARTS WITH
        public class StartsWithFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                if (objValue is string strVal && parameterValue is string strParam)
                    return strVal.StartsWith(strParam, StringComparison.OrdinalIgnoreCase);

                return false;
            }
        }

        // ENDS WITH
        public class EndsWithFilter : IRevitParameterFilter
        {
            public bool IsMatch(IBHoMObject obj, string parameterName, object parameterValue)
            {
                object objValue = obj.GetRevitParameterValue(parameterName);

                if (objValue is string strVal && parameterValue is string strParam)
                    return strVal.EndsWith(strParam, StringComparison.OrdinalIgnoreCase);

                return false;
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/


        private static int? SafeCompare(object value1, object value2)
        {
            if (value1 == null || value2 == null)
                return null;

            if (value1 is IComparable c1 && value2 is IComparable c2)
                try
                {
                    return c1.CompareTo(c2);
                }
                catch
                {
                    return null;
                }
            return null;
        }

    }
}