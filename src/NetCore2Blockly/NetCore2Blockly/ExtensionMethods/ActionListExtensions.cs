﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore2Blockly
{
    /// <summary>
    /// extension to generate types
    /// </summary>
    static class ActionListExtensions
    {

        private static TypeArgumentBase[] recursive(TypeArgumentBase[] types)
        {
            var ids = types.Select(it => it.id).Distinct().ToArray();

            var allProps =
                 types.SelectMany(it => it.GetProperties())
                .Where(it => it != null && it.PropertyType != null)
                .Where(type => type.PropertyType.id != null)
                .ToArray();
            // find arrays;
            var propsExtra = allProps
                .Where(it => it.PropertyType.TranslateToBlocklyType() == null)
                .Select(it => it.PropertyType)
                .Where(type => !ids.Contains(type.id))
                .ToArray();

            var arrayProps = allProps
                .Where(it => it.IsArray)
                .Where(it => (it as PropertyCSharp) != null)
                .Select(it => it as PropertyCSharp)
                .Select(it=>it.PropertyType as TypeToGenerateFromCSharp)
                .Where(it =>it != null)
                .Select(it=> it.GetUnderlyingArrayType())
                .Where(it => it != null)
                .Where(it => it.TranslateToBlocklyType() == null)
                .Where(type => !ids.Contains(type.id))
                .ToArray();

            if (arrayProps.Length > 0)
            {
                propsExtra = propsExtra.Union(arrayProps).ToArray();
            }
            if (propsExtra.Length == 0)
                return types;

            propsExtra = propsExtra.Union(types).ToArray();
            var newValues = recursive(propsExtra);
            return types.Union(newValues).ToArray();
        }
        /// <summary>
        /// Gets the type with null blockly default types.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        internal static TypeArgumentBase[] GetAllTypesWithNullBlocklyType(this ActionInfo[] list)
        {
            var types= list

                .SelectMany(it => it.Params)
                .Select(param => param.Value.type)
                .Distinct()
                .Where(type => type.TranslateToBlocklyType()==null)
                
                .ToArray();


            types = recursive(types);
            var ids = types.Select(it => it.id).Distinct().ToArray();
            types = types.GroupBy(it => it.id).Select(it => it.First()).ToArray();
            var returnTypes = list

                .Select(it => it.ReturnType)
                .Where(it=>it != null)
                .Distinct()
                .Where(type => type.TranslateToBlocklyType() == null)
                .Where(type => type.id != null)
                .ToArray();

            
            return types;

        }

    }
}
