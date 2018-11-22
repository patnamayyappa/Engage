using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanSolutionXml
{
    public class RootComponentCompare : IComparer<RootComponent>
    {
        public int Compare(RootComponent x, RootComponent y)
        {
            int ret = 0;

            ret = x.Type.CompareTo(y.Type);
            if (ret != 0)
                return ret;
            if (x.SchemaName != null && y.SchemaName != null)
            {
                ret = x.SchemaName.CompareTo(y.SchemaName);
                if (ret != 0)
                    return ret;
            }

            if (x.Id != null && y.Id != null)
            {
                ret = x.Id.CompareTo(y.Id);
                if (ret != 0)
                    return ret;
            }

            if (x.ParentId != null && y.ParentId != null)
            {
                ret = x.ParentId.CompareTo(y.ParentId);
                if (ret != 0)
                    return ret;
            }

            if (x.Behavior != null && y.Behavior != null)
            {
                ret = x.Behavior.CompareTo(y.Behavior);
                if (ret != 0)
                    return ret;
            }

            return ret;
        }
    }
    public class MissingDependencyComparer : IComparer<MissingDependency>
    {
        public int Compare(MissingDependency x, MissingDependency y)
        {
            int ret = 0;

            if (x.Required.Type != null && y.Required.Type != null)
            {
                ret = x.Required.Type.CompareTo(y.Required.Type);
                if (ret != 0)
                    return ret;
            }

           

            if (x.Required.ParentSchemaName != null && y.Required.ParentSchemaName != null)
            {
                ret = x.Required.ParentSchemaName.CompareTo(y.Required.ParentSchemaName);
                if (ret != 0)
                    return ret;
            }

            if (x.Required.SchemaName!= null && y.Required.SchemaName != null)
            {
                ret = x.Required.SchemaName.CompareTo(y.Required.SchemaName);
                if (ret != 0)
                    return ret;
            }
            if (x.Required.Key != null && y.Required.Key != null)
            {
                ret = x.Required.Key.CompareTo(y.Required.Key);
                if (ret != 0)
                    return ret;
            }

            if (x.Dependent.Type != null && y.Dependent.Type != null)
            {
                ret = x.Dependent.Type.CompareTo(y.Dependent.Type);
                if (ret != 0)
                    return ret;
            }
            

            if (x.Dependent.ParentSchemaName != null && y.Dependent.ParentSchemaName != null)
            {

                ret = x.Dependent.ParentSchemaName.CompareTo(y.Dependent.ParentSchemaName);
                if (ret != 0)
                    return ret;
            }

            if (x.Dependent.SchemaName != null && y.Dependent.SchemaName != null)
            {
                ret = x.Dependent.SchemaName.CompareTo(y.Dependent.SchemaName);
                if (ret != 0)
                    return ret;
            }
            if (x.Dependent.Key != null && y.Dependent.Key != null)
            {
                ret = x.Dependent.Key.CompareTo(y.Dependent.Key);
                if (ret != 0)
                    return ret;
            }
            return ret;
        }
    }
}
